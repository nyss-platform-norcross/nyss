import React, { useState, Fragment, useEffect } from 'react';
import { connect } from "react-redux";
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

const ProjectAlertRecipientsEditPageComponent = (props) => {
  const [organizations, setOrganizations] = useState([]);

  const [form, setForm] = useState(null);

  useMount(() => {
    props.openEdition(props.projectId, props.alertRecipientId);
  });

  useEffect(() => {
    if (props.alertRecipient === null) {
      return;
    }

    const uniqueOrganizations = [...new Set(props.listData.map(ar => ar.organization))];
    setOrganizations(uniqueOrganizations.map(o => ({ title: o })));
    
    const fields = {
      role: props.alertRecipient.role,
      organization: props.alertRecipient.organization,
      email: props.alertRecipient.email || '',
      phoneNumber: props.alertRecipient.phoneNumber || ''
    };

    const validation = {
      role: [validators.required],
      organization: [validators.required],
      email: [validators.emailWhen(x => x.phoneNumber === '')],
      phoneNumber: [validators.phoneNumber, validators.requiredWhen(x => x.email === '')]
    };

    setForm(createForm(fields, validation));
  }, [props.listData, props.alertRecipient]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    props.edit(props.projectId, {
      id: props.alertRecipient.id,
      role: values.role,
      organization: values.organization,
      email: values.email,
      phoneNumber: values.phoneNumber
    });
  };

  if (!form || !props.alertRecipient) {
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
              value={props.alertRecipient.organization}
              freeSolo
              autoSelect
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
        </Grid>
        <FormActions>
          <Button onClick={() => props.goToList(props.projectId)}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.projectAlertRecipient.form.update)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

ProjectAlertRecipientsEditPageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  alertRecipientId: ownProps.match.params.alertRecipientId,
  listData: state.projectAlertRecipients.listData,
  alertRecipient: state.projectAlertRecipients.formData,
  isSaving: state.projectAlertRecipients.formSaving,
  error: state.projectAlertRecipients.formError
});

const mapDispatchToProps = {
  openEdition: projectAlertRecipientsActions.openEdition.invoke,
  edit: projectAlertRecipientsActions.edit.invoke,
  goToList: projectAlertRecipientsActions.goToList
};

export const ProjectAlertRecipientsEditPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectAlertRecipientsEditPageComponent)
);
