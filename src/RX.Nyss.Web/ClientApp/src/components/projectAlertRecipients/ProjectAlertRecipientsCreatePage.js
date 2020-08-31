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
import { MenuItem, Checkbox, FormControlLabel, Typography, Card, CardContent } from '@material-ui/core';
import { MultiSelect } from '../forms/MultiSelect';

const ProjectAlertRecipientsCreatePageComponent = (props) => {
  const [freeTextOrganizations, setFreeTextOrganizations] = useState([]);
  const [supervisorsDataSource, setSupervisorsDataSource] = useState([]);
  const [selectedSupervisors, setSelectedSupervisors] = useState([]);
  const [selectedHealthRisks, setSelectedHealthRisks] = useState([]);
  const [selectedOrganizationId, setSelectedOrganizationId] = useState(null);
  const [acceptAnySupervisor, setAcceptAnySupervisor] = useState(true);
  const [acceptAnyHealthRisk, setAcceptAnyHealthRisk] = useState(true);
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

    const newForm = createForm(fields, validation);
    newForm.fields.organizationId.subscribe(({ newValue }) => setSelectedOrganizationId(newValue));
    return newForm;
  });

  useMount(() => {
    props.openCreation(props.projectId);
  });

  useEffect(() => {
    const uniqueOrganizations = [...new Set(props.listData.map(ar => ar.organization))];
    setFreeTextOrganizations(uniqueOrganizations.map(o => ({ title: o })));
  }, [props.listData]);

  useEffect(() => {
    setSelectedSupervisors([]);
    setSupervisorsDataSource((props.formData && props.formData.supervisors.filter(s => s.organizationId === parseInt(selectedOrganizationId))) || []);
    if (props.formData && !userRoles.some(r => r === Administrator)) {
      setSelectedOrganizationId(props.formData.projectOrganizations[0].id)
    }
  }, [props.formData, selectedOrganizationId, userRoles])

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
      organizationId: parseInt(values.organizationId),
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

  if (!props.formData) {
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
                    <FormControlLabel
                      control={<Checkbox checked={acceptAnySupervisor} onClick={e => setAcceptAnySupervisor(!acceptAnySupervisor)} />}
                      label={strings(stringKeys.projectAlertRecipient.form.anySupervisor)}
                    />
                  </Grid>

                  {!acceptAnySupervisor && (
                    <Grid item xs={12}>
                      <MultiSelect
                        label={strings(stringKeys.projectAlertRecipient.form.supervisors)}
                        options={supervisorsDataSource.map((s) => { return { label: s.name, value: s.id } })}
                        value={selectedSupervisors}
                        onChange={onSupervisorChange}
                      />
                    </Grid>
                  )}
                  <Grid item xs={12}>
                    <FormControlLabel
                      control={<Checkbox checked={acceptAnyHealthRisk} onClick={e => setAcceptAnyHealthRisk(!acceptAnyHealthRisk)} />}
                      label={strings(stringKeys.projectAlertRecipient.form.anyHealthRisk)}
                    />
                  </Grid>
                  {!acceptAnyHealthRisk && (
                    <Grid item xs={12}>
                      <MultiSelect
                        label={strings(stringKeys.projectAlertRecipient.form.healthRisks)}
                        options={props.formData.healthRisks.map((s) => { return { label: s.healthRiskName, value: s.id } })}
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
