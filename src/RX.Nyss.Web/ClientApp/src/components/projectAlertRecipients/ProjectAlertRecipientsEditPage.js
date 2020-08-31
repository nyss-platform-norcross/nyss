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
import { CardContent, Card, Typography, FormControlLabel, Checkbox } from '@material-ui/core';
import { MultiSelect } from '../forms/MultiSelect';

const ProjectAlertRecipientsEditPageComponent = (props) => {
  const [freeTextOrganizations, setFreeTextOrganizations] = useState([]);
  const [selectedSupervisors, setSelectedSupervisors] = useState([]);
  const [selectedHealthRisks, setSelectedHealthRisks] = useState([]);
  const [acceptAnySupervisor, setAcceptAnySupervisor] = useState(false);
  const [acceptAnyHealthRisk, setAcceptAnyHealthRisk] = useState(false);

  const [form, setForm] = useState(null);

  useMount(() => {
    props.openEdition(props.projectId, props.alertRecipientId);
  });

  useEffect(() => {
    if (props.alertRecipient === null || props.formData === null) {
      return;
    }

    const uniqueOrganizations = [...new Set(props.listData.map(ar => ar.organization))];
    setFreeTextOrganizations(uniqueOrganizations.map(o => ({ title: o })));
    setSelectedSupervisors(props.alertRecipient.supervisors.map((s) => { return { label: s.name, value: s.id } }));
    setSelectedHealthRisks(props.alertRecipient.healthRisks.map((s) => { return { label: s.healthRiskName, value: s.id } }));
    setAcceptAnySupervisor(props.alertRecipient.supervisors.length === 0);
    setAcceptAnyHealthRisk(props.alertRecipient.healthRisks.length === 0);

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
  }, [props.listData, props.alertRecipient, props.formData]);

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
      phoneNumber: values.phoneNumber,
      supervisors: acceptAnySupervisor ? [] : selectedSupervisors.map(s => s.value),
      healthRisks: acceptAnyHealthRisk ? [] : selectedHealthRisks.map(hr => hr.value)
    });
  };

  const onSupervisorChange = (value, eventData) => {
    if (eventData.action === "select-option") {
      setSelectedSupervisors([...selectedSupervisors, eventData.option]);
    } else if (eventData.action === "remove-value" || eventData.action === "pop-value") {
      setSelectedSupervisors(selectedSupervisors.filter(hr => hr.value !== eventData.removedValue.value));
    } else if (eventData.action === "clear") {
      setSelectedSupervisors([]);
    }
  }

  const onHealthRiskChange = (value, eventData) => {
    if (eventData.action === "select-option") {
      setSelectedHealthRisks([...selectedHealthRisks, eventData.option]);
    } else if (eventData.action === "remove-value" || eventData.action === "pop-value") {
      setSelectedHealthRisks(selectedHealthRisks.filter(hr => hr.value !== eventData.removedValue.value));
    } else if (eventData.action === "clear") {
      setSelectedHealthRisks([]);
    }
  }

  if (!props.formData || !form) {
    return null;
  }

  return (
    <Fragment>
      {props.error && <ValidationMessage message={props.error} />}

      <Form onSubmit={handleSubmit} fullWidth style={{ maxWidth: 800 }}>
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Grid container spacing={3}>

                  <Grid item xs={12}>
                    <Typography variant="h3">{strings(stringKeys.projectAlertRecipient.form.receiverDetails)}</Typography>
                  </Grid>

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
                      options={freeTextOrganizations}
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
                </Grid>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} md={6}>
            <Card style={{ overflow: "visible" }}>
              <CardContent>
                <Grid container spacing={3}>

                  <Grid item xs={12}>
                    <Typography variant="h3">{strings(stringKeys.projectAlertRecipient.form.triggerDetails)}</Typography>
                  </Grid>

                  <Grid item xs={12}>
                    <FormControlLabel
                      control={<Checkbox checked={acceptAnySupervisor} onClick={() => setAcceptAnySupervisor(!acceptAnySupervisor)} />}
                      label={strings(stringKeys.projectAlertRecipient.form.anySupervisor)}
                    />
                  </Grid>

                  {!acceptAnySupervisor && (
                    <Grid item xs={12}>
                      <MultiSelect
                        label={strings(stringKeys.projectAlertRecipient.form.supervisors)}
                        options={
                          props.formData.supervisors
                            .filter(s => !selectedSupervisors.some(ss => ss.id === s.id) && s.organizationId === props.alertRecipient.organizationId)
                            .map((s) => { return { label: s.name, value: s.id } })}
                        value={selectedSupervisors}
                        onChange={onSupervisorChange}
                      />
                    </Grid>
                  )}
                  <Grid item xs={12}>
                    <FormControlLabel
                      control={<Checkbox checked={acceptAnyHealthRisk} onClick={() => setAcceptAnyHealthRisk(!acceptAnyHealthRisk)} />}
                      label={strings(stringKeys.projectAlertRecipient.form.anyHealthRisk)}
                    />
                  </Grid>
                  {!acceptAnyHealthRisk && (
                    <Grid item xs={12}>
                      <MultiSelect
                        label={strings(stringKeys.projectAlertRecipient.form.healthRisks)}
                        options={
                          props.formData.healthRisks
                            .filter(hr => !selectedHealthRisks.some(shr => shr.id === hr.id))
                            .map((s) => { return { label: s.healthRiskName, value: s.id } })}
                        value={selectedHealthRisks}
                        onChange={onHealthRiskChange}
                      />
                    </Grid>
                  )}
                </Grid>
              </CardContent>
            </Card>
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
  alertRecipient: state.projectAlertRecipients.recipient,
  formData: state.projectAlertRecipients.formData,
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
