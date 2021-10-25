import React, { useState, Fragment, useMemo } from 'react';
import { connect } from "react-redux";
import { withLayout } from '../../utils/layout';
import { validators, createForm, useCustomErrors } from '../../utils/forms';
import * as nationalSocietyUsersActions from './logic/nationalSocietyUsersActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { ValidationMessage } from '../forms/ValidationMessage';
import SelectField from '../forms/SelectField';
import { MenuItem, Typography, Button, Box, Grid } from '@material-ui/core';

const NationalSocietyUsersAddExistingPageComponent = (props) => {
  const [form] = useState(() => {
    const fields = {
      email: "",
      modemId: "",
      organizationId: "",
    };

    const validation = {
      email: [validators.required, validators.email, validators.maxLength(100)],
      modemId: [validators.requiredWhen(_ => canSelectModem)],
      organizationId: [validators.requiredWhen(_ => canChooseOrganization)],
    };

    return createForm(fields, validation);
  });

  const canSelectModem = props.modems != null && props.modems.length > 0;
  const canChooseOrganization = true;

  useCustomErrors(form, props.error);

  useMount(() => {
    props.openAddExisting(props.nationalSocietyId);
  });

  const availableOrganizations = useMemo(() => {
    console.log("availableOrganizations", props.data);
    if (!props.data) {
      return [];
    }

    return props.data.organizations;
  }, [props.data]);  

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    props.addExisting({
      nationalSocietyId: props.nationalSocietyId,
      email: values.email,
      modemId: !!values.modemId ? parseInt(values.modemId) : null,
      organizationId: !!values.organizationId ? parseInt(values.organizationId) : null,
    });
  };

  return (
    <Fragment>
      {props.error && <ValidationMessage message={props.error.message} />}

      <Form onSubmit={handleSubmit}>
        <Box mb={3}>
          <Typography variant="body1">{strings(stringKeys.nationalSocietyUser.form.addExistingDescription)}</Typography>
        </Box>

        <Grid container spacing={2}>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.nationalSocietyUser.form.email)}
              name="email"
              field={form.fields.email}
              autoFocus
              inputMode={"email"}
            />
          </Grid>

          {canChooseOrganization && (
            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.nationalSocietyUser.form.organization)}
                field={form.fields.organizationId}
                name="organizationId">
                {availableOrganizations.map(organization => (
                  <MenuItem key={`organization_${organization.id}`} value={organization.id.toString()}>
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
                {props.modems.map(modem => (
                  <MenuItem key={`modemId_${modem.id}`} value={modem.id.toString()}>
                    {modem.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}

        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList(props.nationalSocietyId)}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.nationalSocietyUser.form.addExisting)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

NationalSocietyUsersAddExistingPageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isSaving: state.nationalSocietyUsers.formSaving,
  error: state.nationalSocietyUsers.formError,
  modems: state.nationalSocietyUsers.formModems,
  data: state.nationalSocietyUsers.formAdditionalData,
});

const mapDispatchToProps = {
  openAddExisting: nationalSocietyUsersActions.openAddExisting.invoke,
  addExisting: nationalSocietyUsersActions.addExisting.invoke,
  goToList: nationalSocietyUsersActions.goToList
};

export const NationalSocietyUsersAddExistingPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyUsersAddExistingPageComponent)
);
