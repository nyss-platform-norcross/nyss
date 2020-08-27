import React, { useState, Fragment, useEffect } from 'react';
import { connect, useSelector } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as projectAlertRecipientsActions from './logic/projectAlertRecipientsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import Button from "@material-ui/core/Button";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { ValidationMessage } from '../forms/ValidationMessage';
import AutocompleteTextInputField from '../forms/AutocompleteTextInputField';
import TextInputField from '../forms/TextInputField';
import { Administrator } from '../../authentication/roles';
import SelectField from '../forms/SelectField';
import { MenuItem } from '@material-ui/core';
import { MultiSelect } from '../forms/MultiSelect';

const ProjectAlertRecipientsCreatePageComponent = (props) => {
  const [organizations, setOrganizations] = useState([]);
  const [supervisorsDataSource, setSupervisorsDataSource] = useState([]);
  const [healtHRiskDataSource, setHealtHRiskDataSource] = useState([]);
  const userRoles = useSelector(state => state.appData.user.roles);

  const [form] = useState(() => {
    const fields = {
      role: '',
      organization: '',
      email: '',
      phoneNumber: '',
      organizationId: ''
    };

    const validation = {
      role: [validators.required],
      organization: [validators.required],
      email: [validators.emailWhen(x => x.phoneNumber === '')],
      phoneNumber: [validators.phoneNumber, validators.requiredWhen(x => x.email === '')]
    };

    return createForm(fields, validation)
  });

  useMount(() => {
    props.openCreation(props.projectId);
  });

  useEffect(() => {
    const uniqueOrganizations = [...new Set(props.listData.map(ar => ar.organization))];
    setOrganizations(uniqueOrganizations.map(o => ({ title: o })));
  }, [props.listData]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    props.create(props.projectId, {
      role: values.role,
      organization: values.organization,
      email: values.email,
      phoneNumber: values.phoneNumber,
      organizationId: parseInt(values.organizationId) || null
    });
  };

  if (!props.formData) {
    return null;
  }

  return (
    <Fragment>
      {props.error && <ValidationMessage message={props.error} />}

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.projectAlertRecipient.form.role)}
              field={form.fields.role}
              name="role"
            />
          </Grid>

          <Grid item xs={12}>
            <AutocompleteTextInputField
              label={strings(stringKeys.projectAlertRecipient.form.organization)}
              field={form.fields.organization}
              options={organizations}
              freeSolo
              autoSelect
              allowAddingValue
              name="organization"
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.projectAlertRecipient.form.email)}
              field={form.fields.email}
              name="email"
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.projectAlertRecipient.form.phoneNumber)}
              field={form.fields.phoneNumber}
              name="phoneNumber"
            />
          </Grid>

          {userRoles.some(r => r === Administrator) && (
            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.projectAlertRecipient.form.projectOrganization)}
                field={form.fields.organizationId}
                name="organizationId"
              >
                {props.formData.projectOrganizations.map(org => (
                  <MenuItem key={org.id} value={JSON.stringify(org.id)}>
                    {org.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}

          <Grid item xs={12}>
            <MultiSelect
              label={strings(stringKeys.projectAlertRecipient.form.supervisors)}
              options={supervisorsDataSource}
            />
          </Grid>
        </Grid>
        <FormActions>
          <Button onClick={() => props.goToList(props.projectId)}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.projectAlertRecipient.form.create)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment >
  );
}

ProjectAlertRecipientsCreatePageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  listData: state.projectAlertRecipients.listData,
  formData: state.projectAlertRecipients.formData,
  isSaving: state.projectAlertRecipients.formSaving,
  error: state.projectAlertRecipients.formError
});

const mapDispatchToProps = {
  openCreation: projectAlertRecipientsActions.openCreation.invoke,
  create: projectAlertRecipientsActions.create.invoke,
  goToList: projectAlertRecipientsActions.goToList
};

export const ProjectAlertRecipientsCreatePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectAlertRecipientsCreatePageComponent)
);
