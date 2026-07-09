import React, {
  useState,
  Fragment,
  useEffect,
  useMemo,
  useCallback,
  createRef,
} from "react";
import { connect, useSelector } from "react-redux";
import { withLayout } from "../../utils/layout";
import { validators, createForm, useCustomErrors } from "../../utils/forms";
import * as projectsActions from "./logic/projectsActions";
import Layout from "../layout/Layout";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import TextInputField from "../forms/TextInputField";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { Grid, Typography, MenuItem } from "@material-ui/core";
import { MultiSelect } from "../forms/MultiSelect";
import { ProjectsHealthRiskItem } from "./ProjectHealthRiskItem";
import { getSaveFormModel } from "./logic/projectsService";
import SelectField from "../forms/SelectField";
import { ValidationMessage } from "../forms/ValidationMessage";
import CheckboxField from "../forms/CheckboxField";
import * as roles from "../../authentication/roles";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import { trackPageView } from "../../utils/appInsightsHelper";

const ProjectsCreatePageComponent = (props) => {
  const [healthRiskDataSource, setHealthRiskDataSource] = useState([]);
  const [selectedHealthRisks, setSelectedHealthRisks] = useState([]);
  const [healthRisksFieldTouched, setHealthRisksFieldTouched] = useState(false);
  const useRtlDirection = useSelector(
    (state) => state.appData.user.languageCode === "ar",
  );

  useEffect(() => {
    props.data &&
      setHealthRiskDataSource(
        props.data.healthRisks.map((hr) => ({
          label: hr.healthRiskName,
          value: hr.healthRiskId,
          data: hr,
        })),
      );
    props.data &&
      setSelectedHealthRisks(
        props.data.healthRisks.filter((hr) => hr.healthRiskType === "Activity"),
      );
  }, [props.data]);

  const canChangeOrganization = useCallback(
    () =>
      props.callingUserRoles.some(
        (r) => r === roles.Administrator || r === roles.Coordinator,
      ),
    [props.callingUserRoles],
  );

  const form = useMemo(() => {
    const fields = {
      name: "",
      allowMultipleOrganizations: false,
      organizationId: "",
      alertNotHandledNotificationRecipientId: "",
    };

    const validation = {
      name: [
        validators.required,
        validators.minLength(1),
        validators.maxLength(100),
      ],
      organizationId: [validators.requiredWhen((f) => canChangeOrganization())],
      alertNotHandledNotificationRecipientId: [validators.required],
    };

    const refs = {
      name: createRef(),
      organizationId: createRef(),
      alertNotHandledNotificationRecipientId: createRef(),
    };

    return createForm(fields, validation, refs);
  }, [canChangeOrganization]);

  useCustomErrors(form, props.error);

  useMount(() => {
    props.openCreation(props.nationalSocietyId);

    // Track page view
    trackPageView("ProjectsCreatePage");
  });

  const handleSubmit = (e) => {
    e.preventDefault();

    const preventSubmit = selectedHealthRisks.length === 1;

    if (preventSubmit) {
      setHealthRisksFieldTouched(true);
    }

    if (!form.isValid()) {
      Object.values(form.fields)
        .filter((e) => e.error)[0]
        .scrollTo();
      return;
    }

    !preventSubmit &&
      props.create(
        props.nationalSocietyId,
        getSaveFormModel(form.getValues(), selectedHealthRisks),
      );
  };

  const onHealthRiskChange = (value, eventData) => {
    if (eventData.action === "select-option") {
      setSelectedHealthRisks([...selectedHealthRisks, eventData.option.data]);
    } else if (
      (eventData.action === "remove-value" ||
        eventData.action === "pop-value") &&
      eventData.removedValue.data.healthRiskType !== "Activity"
    ) {
      setSelectedHealthRisks(
        selectedHealthRisks.filter(
          (hr) => hr.healthRiskId !== eventData.removedValue.value,
        ),
      );
    } else if (eventData.action === "clear") {
      setSelectedHealthRisks(
        props.data.healthRisks.filter((hr) => hr.healthRiskType === "Activity"),
      );
    }
  };

  const getSelectedHealthRiskValue = () =>
    healthRiskDataSource
      .filter((hr) =>
        selectedHealthRisks.some((shr) => shr.healthRiskId === hr.value),
      )
      .sort((a, b) => (a.data.healthRiskType === "Activity" ? -1 : 1));

  if (!props.data) {
    return null;
  }

  return (
    <Fragment>
      {props.error && <ValidationMessage message={props.error.message} />}

      <Form onSubmit={handleSubmit} fullWidth style={{ maxWidth: 800 }}>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.project.form.name)}
              name="name"
              field={form.fields.name}
              autoFocus
              fieldRef={form.fields.name.ref}
            />
          </Grid>

          {canChangeOrganization() && (
            <Fragment>
              <Grid item xs={12}>
                <CheckboxField
                  label={strings(
                    stringKeys.project.form.allowMultipleOrganizations,
                  )}
                  name="allowMultipleOrganizations"
                  field={form.fields.allowMultipleOrganizations}
                  color="primary"
                />
              </Grid>

              <Grid item xs={12}>
                <SelectField
                  label={strings(stringKeys.project.form.organization)}
                  field={form.fields.organizationId}
                  name="organizationId"
                  fieldRef={form.fields.organizationId.ref}
                >
                  {props.data.organizations.map((organization) => (
                    <MenuItem
                      key={`organization_${organization.id}`}
                      value={organization.id.toString()}
                    >
                      {organization.name}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>
            </Fragment>
          )}

          <Grid item xs={12}>
            <SelectField
              label={strings(
                stringKeys.project.form.alertNotHandledNotificationRecipient,
              )}
              name="alertNotHandledNotificationRecipientId"
              field={form.fields.alertNotHandledNotificationRecipientId}
              fieldRef={form.fields.alertNotHandledNotificationRecipientId.ref}
            >
              {props.data.alertNotHandledRecipients.map((recipient) => (
                <MenuItem
                  key={`alertNotHandledRecipient_${recipient.id}`}
                  value={recipient.id.toString()}
                >
                  {recipient.name}
                </MenuItem>
              ))}
            </SelectField>
          </Grid>

          <Grid item xs={12}>
            <MultiSelect
              label={strings(stringKeys.project.form.healthRisks)}
              options={healthRiskDataSource}
              onChange={onHealthRiskChange}
              value={getSelectedHealthRiskValue()}
              onBlur={(e) => setHealthRisksFieldTouched(true)}
              error={
                healthRisksFieldTouched && selectedHealthRisks.length < 2
                  ? `${strings(stringKeys.validation.noHealthRiskSelected)}`
                  : null
              }
              rtl={useRtlDirection}
            />
          </Grid>

          {selectedHealthRisks.length > 0 && (
            <Grid item xs={12}>
              <Typography variant="h3">
                {strings(stringKeys.project.form.healthRisksSection)}
              </Typography>
            </Grid>
          )}

          {selectedHealthRisks.map((selectedHealthRisk) => (
            <Grid
              item
              xs={12}
              key={`healthRisk.${selectedHealthRisk.healthRiskId}`}
            >
              <ProjectsHealthRiskItem
                key={`projectsHealthRiskItem_${selectedHealthRisk.healthRiskId}`}
                form={form}
                projectHealthRisk={{ id: null }}
                healthRisk={selectedHealthRisk}
              />
            </Grid>
          ))}
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

ProjectsCreatePageComponent.propTypes = {};

const mapStateToProps = (state, ownProps) => ({
  data: state.projects.formData,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isSaving: state.projects.formSaving,
  error: state.projects.formError,
  callingUserRoles: state.appData.user.roles,
});

const mapDispatchToProps = {
  openCreation: projectsActions.openCreation.invoke,
  create: projectsActions.create.invoke,
  goToList: projectsActions.goToList,
};

export const ProjectsCreatePage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsCreatePageComponent),
);
