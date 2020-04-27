import styles from './ProjectsEditPage.module.scss';

import React, { useState, Fragment, useEffect } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as projectsActions from './logic/projectsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import Typography from '@material-ui/core/Typography';
import TextInputField from '../forms/TextInputField';
import Button from "@material-ui/core/Button";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import AddIcon from '@material-ui/icons/Add';
import { MultiSelect } from '../forms/MultiSelect';
import { ProjectsHealthRiskItem } from './ProjectHealthRiskItem';
import { getSaveFormModel } from './logic/projectsService';
import { Loading } from '../common/loading/Loading';
import SelectField from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import { ValidationMessage } from '../forms/ValidationMessage';
import { Tooltip, Icon, IconButton } from '@material-ui/core';

const ProjectsEditPageComponent = (props) => {
  const [healthRiskDataSource, setHealthRiskDataSource] = useState([]);
  const [selectedHealthRisks, setSelectedHealthRisks] = useState([]);
  const [alertRecipients, setAlertRecipients] = useState([]);

  useMount(() => {
    props.openEdition(props.nationalSocietyId, props.projectId);
  })

  useEffect(() => {
    setHealthRiskDataSource(props.healthRisks.map(hr => ({ label: hr.healthRiskName, value: hr.healthRiskId, data: hr })));
  }, [props.healthRisks])

  const [form, setForm] = useState(null);

  useEffect(() => {
    if (!props.data) {
      return;
    }

    let fields = {
      name: props.data.name,
      timeZoneId: props.data.timeZoneId
    };

    let validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)],
      timeZoneId: [validators.required, validators.minLength(1), validators.maxLength(50)]
    };

    const newForm = createForm(fields, validation);
    console.log(props.data);
    const newAlertRecipients = props.data.alertNotificationRecipients.map((ar, i) => i);

    newAlertRecipients.forEach(ar => {
      newForm.addField(`alertRecipientRole${ar}`, '', [validators.required, validators.maxLength(100)]);
      newForm.addField(`alertRecipientOrganization${ar}`, '', [validators.required, validators.maxLength(100)]);
      newForm.addField(`alertRecipientEmail${ar}`, '', [validators.emailWhen(_ => _[`alertRecipientPhone${ar}`] === ''), validators.maxLength(100)]);
      newForm.addField(`alertRecipientPhone${ar}`, '', [validators.requiredWhen(_ => _[`alertRecipientEmail${ar}`] === ''), validators.maxLength(20)]);
    })

    setForm(newForm);

    setSelectedHealthRisks(props.data.projectHealthRisks);

    return () => setForm(null);
  }, [props.data]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (selectedHealthRisks.length === 0) {
      return;
    }

    if (!form.isValid()) {
      return;
    };

    props.edit(props.nationalSocietyId, props.projectId, getSaveFormModel(form.getValues(), selectedHealthRisks, alertRecipients));
  };

  const onHealthRiskChange = (value, eventData) => {
    if (eventData.action === "select-option") {
      setSelectedHealthRisks([...selectedHealthRisks, eventData.option.data]);
    } else if (eventData.action === "remove-value" || eventData.action === "pop-value") {
      setSelectedHealthRisks(selectedHealthRisks.filter(hr => hr.healthRiskId !== eventData.removedValue.value));
    } else if (eventData.action === "clear") {
      setSelectedHealthRisks([]);
    }
  }

  const onAlertRecipientAdd = () => {
    const alertRecipientNumber = alertRecipients.length + 1;
    const newRecipients = alertRecipients.slice();
    newRecipients.push(alertRecipientNumber);
    setAlertRecipients(newRecipients);

    form.addField(`alertRecipientRole${alertRecipientNumber}`, '', [validators.required, validators.maxLength(100)]);
    form.addField(`alertRecipientOrganization${alertRecipientNumber}`, '', [validators.required, validators.maxLength(100)]);
    form.addField(`alertRecipientEmail${alertRecipientNumber}`, '', [validators.emailWhen(_ => _[`alertRecipientPhone${alertRecipientNumber}`] === ''), validators.maxLength(100)]);
    form.addField(`alertRecipientPhone${alertRecipientNumber}`, '', [validators.requiredWhen(_ => _[`alertRecipientEmail${alertRecipientNumber}`] === ''), validators.maxLength(20)]);
  }

  const onRemoveRecipient = (recipient) => {
    setAlertRecipients(alertRecipients.filter(ar => ar !== recipient));

    form.removeField(`alertRecipientRole${recipient}`);
    form.removeField(`alertRecipientOrganization${recipient}`);
    form.removeField(`alertRecipientEmail${recipient}`);
    form.removeField(`alertRecipientPhone${recipient}`);
  }

  if (props.isFetching || !form) {
    return <Loading />;
  }

  return (
    <Fragment>
      {props.error && <ValidationMessage message={props.error} />}

      <Form onSubmit={handleSubmit} fullWidth style={{ maxWidth: 800 }}>
        <Grid container spacing={3}>
          <Grid item xs={12} sm={9}>
            <TextInputField
              label={strings(stringKeys.project.form.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>

          <Grid item xs={12}>
            <SelectField
              label={strings(stringKeys.project.form.timeZone)}
              field={form.fields.timeZoneId}
              name="timeZoneId"
            >
              {props.timeZones.map(timeZone => (
                <MenuItem key={timeZone.id} value={timeZone.id}>
                  {timeZone.displayName}
                </MenuItem>
              ))}
            </SelectField>
          </Grid>

          <Grid item xs={12}>
            <MultiSelect
              label={strings(stringKeys.project.form.healthRisks)}
              options={healthRiskDataSource}
              defaultValue={healthRiskDataSource.filter(hr => (selectedHealthRisks.some(shr => shr.healthRiskId === hr.value)))}
              onChange={onHealthRiskChange}
              error={selectedHealthRisks.length === 0 ? `${strings(stringKeys.validation.fieldRequired)}` : null}
            />
          </Grid>

          {selectedHealthRisks.length > 0 &&
            <Grid item xs={12}>
              <Typography variant="h3">{strings(stringKeys.project.form.healthRisksSetion)}</Typography>
            </Grid>
          }

          {selectedHealthRisks.map(selectedHealthRisk => (
            <ProjectsHealthRiskItem
              key={`projectsHealthRiskItem_${selectedHealthRisk.healthRiskId}`}
              form={form}
              projectHealthRisk={{ id: selectedHealthRisk.id }}
              healthRisk={selectedHealthRisk}
            />
          ))}

          <Grid item xs={12}>
            <Typography variant="h3">
              <div className={styles.alertNotificationsHeader}>
                {strings(stringKeys.project.form.alertNotificationsSection)}

                <Tooltip title={strings(stringKeys.project.form.alertNotificationsSupervisorsExplanation)} className={styles.helpIcon}>
                  <Icon>help_outline</Icon>
                </Tooltip>
              </div>
            </Typography>
            <Typography variant="subtitle1">{strings(stringKeys.project.form.alertNotificationsDescription)}</Typography>

            {alertRecipients.map(ar => (
              <Grid container spacing={3} key={ar}>

                <Grid item lg={3}>
                  <TextInputField
                    label={strings(stringKeys.project.form.alertNotificationsRole)}
                    name="role"
                    field={form.fields[`alertRecipientRole${ar}`]}
                  />
                </Grid>
                <Grid item lg={2}>
                  <TextInputField
                    label={strings(stringKeys.project.form.alertNotificationsOrganization)}
                    name="organization"
                    field={form.fields[`alertRecipientOrganization${ar}`]}
                  />
                </Grid>
                <Grid item lg={3}>
                  <TextInputField
                    label={strings(stringKeys.project.form.alertNotificationsEmail)}
                    name="email"
                    field={form.fields[`alertRecipientEmail${ar}`]}
                  />
                </Grid>
                <Grid item lg={3}>
                  <TextInputField
                    label={strings(stringKeys.project.form.alertNotificationsPhoneNumber)}
                    name="phoneNumber"
                    field={form.fields[`alertRecipientPhone${ar}`]}
                  />
                </Grid>
                <Grid item lg={1} className={styles.removeButtonContainer}>
                  <IconButton onClick={() => onRemoveRecipient(ar)}>
                    <Icon>delete</Icon>
                  </IconButton>
                </Grid>
              </Grid>
            ))}

            <Grid item xs={12} sm={9}>
              <Button startIcon={<AddIcon />} onClick={onAlertRecipientAdd}>{strings(stringKeys.project.form.addRecipient)}</Button>
            </Grid>

          </Grid>
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToOverview(props.nationalSocietyId, props.projectId)}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.project.form.update)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

ProjectsEditPageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  healthRisks: state.projects.formHealthRisks,
  timeZones: state.projects.formTimeZones,
  projectId: ownProps.match.params.projectId,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isFetching: state.projects.formFetching,
  isSaving: state.projects.formSaving,
  data: state.projects.formData,
  error: state.projects.formError
});

const mapDispatchToProps = {
  openEdition: projectsActions.openEdition.invoke,
  edit: projectsActions.edit.invoke,
  goToOverview: projectsActions.goToOverview
};

export const ProjectsEditPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsEditPageComponent)
);
