import { useState, Fragment, useMemo, useEffect } from "react";
import { connect } from "react-redux";
import { withLayout } from "../../utils/layout";
import { validators, createForm, useCustomErrors } from "../../utils/forms";
import * as nationalSocietyUsersActions from "./logic/nationalSocietyUsersActions";
import Layout from "../layout/Layout";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import TextInputField from "../forms/TextInputField";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { ValidationMessage } from "../forms/ValidationMessage";
import SelectField from "../forms/SelectField";
import { MenuItem, Typography, Box, Grid } from "@material-ui/core";
import { Administrator } from "../../authentication/roles";
import { trackPageView } from "../../utils/appInsightsHelper";

const NationalSocietyUsersAddExistingPageComponent = (props) => {
  const [form] = useState(() => {
    const fields = {
      email: "",
      modemId: "",
      organizationId: "",
    };

    const validation = {
      email: [validators.required, validators.email, validators.maxLength(100)],
      modemId: [validators.requiredWhen((_) => canSelectModem)],
      organizationId: [validators.requiredWhen((_) => canSelectOrganization)],
    };

    return createForm(fields, validation);
  });

  const canSelectModem =
    props.formData?.modems && props.formData.modems.length > 0;
  const canSelectOrganization =
    props.formData?.organizations &&
    props.formData.organizations.length > 0 &&
    props.user.roles.some((r) => r === Administrator);

  useCustomErrors(form, props.error);

  useMount(() => {
    props.openAddExisting(props.nationalSocietyId);

    // Track page view
    trackPageView("NationalSocietyUsersAddExistingPage");
  });

  const availableOrganizations = useMemo(() => {
    if (!props.formData) {
      return [];
    }

    return props.formData.organizations;
  }, [props.formData]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    }

    const values = form.getValues();
    props.addExisting({
      nationalSocietyId: parseInt(props.nationalSocietyId),
      email: values.email,
      modemId: !!values.modemId ? parseInt(values.modemId) : null,
      organizationId: !!values.organizationId
        ? parseInt(values.organizationId)
        : null,
    });
  };

  useEffect(() => {
    if (canSelectOrganization || !props.formData) return;

    const organizations = props.formData.organizations.filter(
      (o) => o.isDefaultOrganization,
    );
    const preSelected =
      organizations.length > 0
        ? organizations[0]
        : props.formData.organizations[0];

    form.fields.organizationId.update(preSelected.id);
  }, [props.formData, form, canSelectOrganization]);

  return (
    <Fragment>
      {props.error && <ValidationMessage message={props.error.message} />}

      <Form onSubmit={handleSubmit}>
        <Box mb={3}>
          <Typography variant="h5">
            {strings(
              stringKeys.nationalSocietyUser.form.addExistingDescription,
            )}
          </Typography>
        </Box>

        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.common.email)}
              name="email"
              field={form.fields.email}
              autoFocus
              inputMode={"email"}
            />
          </Grid>

          {canSelectOrganization && (
            <Grid item xs={12}>
              <SelectField
                label={strings(
                  stringKeys.nationalSocietyUser.form.organization,
                )}
                field={form.fields.organizationId}
                name="organizationId"
                customProps={{
                  disabled: false,
                }}
              >
                {availableOrganizations.map((organization) => (
                  <MenuItem
                    key={`organization_${organization.id}`}
                    value={organization.id.toString()}
                  >
                    {organization.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}

          {canSelectModem && (
            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.nationalSocietyUser.form.modem)}
                field={form.fields.modemId}
                name="modemId"
              >
                {props.formData.modems.map((modem) => (
                  <MenuItem
                    key={`modemId_${modem.id}`}
                    value={modem.id.toString()}
                  >
                    {modem.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}
        </Grid>

        <FormActions>
          <CancelButton onClick={() => props.goToList(props.nationalSocietyId)}>
            {strings(stringKeys.form.cancel)}
          </CancelButton>
          <SubmitButton isFetching={props.isSaving}>
            {strings(stringKeys.common.buttons.add)}
          </SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  user: state.appData.user,
  isSaving: state.nationalSocietyUsers.formSaving,
  error: state.nationalSocietyUsers.formError,
  formData: state.nationalSocietyUsers.addExistingFormData,
});

const mapDispatchToProps = {
  openAddExisting: nationalSocietyUsersActions.openAddExisting.invoke,
  addExisting: nationalSocietyUsersActions.addExisting.invoke,
  goToList: nationalSocietyUsersActions.goToList,
};

export const NationalSocietyUsersAddExistingPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(NationalSocietyUsersAddExistingPageComponent),
);
