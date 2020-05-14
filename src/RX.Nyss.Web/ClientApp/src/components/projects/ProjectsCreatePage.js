import styles from './ProjectsCreatePage.module.scss';

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
import SelectField from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import { ValidationMessage } from '../forms/ValidationMessage';
import { Tooltip, Icon } from '@material-ui/core';
import CheckboxField from '../forms/CheckboxField';
import { ProjectsAlertRecipientItem } from './ProjectsAlertRecipientItem';

const ProjectsCreatePageComponent = (props) => {
  const [healthRiskDataSource, setHealthRiskDataSource] = useState([]);
  const [selectedHealthRisks, setSelectedHealthRisks] = useState([]);
  const [alertRecipients, setAlertRecipients] = useState([]);
  const [healthRisksFieldTouched, setHealthRisksFieldTouched] = useState(false);
  const [organizations, setOrganizations] = useState([]);

  useEffect(() => {
    setHealthRiskDataSource(props.healthRisks.map(hr => ({ label: hr.healthRiskName, value: hr.healthRiskId, data: hr })));
  }, [props.healthRisks])

  const [form] = useState(() => {
    const fields = {
      name: "",
      allowMultipleOrganizations: false,
      timeZoneId: ""
    };

    const validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)],
      timeZoneId: [validators.required, validators.minLength(1), validators.maxLength(50)]
    };

    return createForm(fields, validation);
  });

  useMount(() => {
    props.openCreation(props.nationalSocietyId);
  })

  const handleSubmit = (e) => {
    e.preventDefault();

    if (selectedHealthRisks.length === 0) {
      return;
    }

    if (!form.isValid()) {
      return;
    };

    props.create(props.nationalSocietyId, getSaveFormModel(form.getValues(), selectedHealthRisks, alertRecipients));
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
    const newRecipients = alertRecipients.slice();
    newRecipients.push({
      role: '',
      organization: '',
      email: '',
      phoneNumber: ''
    });
    setAlertRecipients(newRecipients);
  }

  const onRemoveRecipient = (recipient) => {
    setAlertRecipients(alertRecipients.filter(ar => ar !== recipient));
  }

  const onAddOrganization = (organization) => {
    setOrganizations([...new Set([...organizations, { title: organization }])]);
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
              autoFocus
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

          <Grid item xs={12} sm={9}>
            <CheckboxField
              label={strings(stringKeys.project.form.allowMultipleOrganizations)}
              name="allowMultipleOrganizations"
              field={form.fields.allowMultipleOrganizations}
            />
          </Grid>

          <Grid item xs={12}>
            <MultiSelect
              label={strings(stringKeys.project.form.healthRisks)}
              options={healthRiskDataSource}
              onChange={onHealthRiskChange}
              onBlur={e => setHealthRisksFieldTouched(true)}
              error={(healthRisksFieldTouched && selectedHealthRisks.length === 0) ? `${strings(stringKeys.validation.fieldRequired)}` : null}
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
              projectHealthRisk={{ id: null }}
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

            {alertRecipients.map((alertRecipient, alertRecipientNumber) => (
              <ProjectsAlertRecipientItem key={alertRecipientNumber}
                alertRecipient={alertRecipient}
                alertRecipientNumber={alertRecipientNumber}
                form={form}
                organizations={organizations}
                onAddOrganization={onAddOrganization}
                onRemoveRecipient={onRemoveRecipient} />
            ))}

            <Grid item xs={12} sm={9}>
              <Button startIcon={<AddIcon />} onClick={onAlertRecipientAdd}>{strings(stringKeys.project.form.addRecipient)}</Button>
            </Grid>

          </Grid>
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList(props.nationalSocietyId)}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.project.form.create)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

ProjectsCreatePageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  healthRisks: state.projects.formHealthRisks,
  timeZones: state.projects.formTimeZones,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isSaving: state.projects.formSaving,
  error: state.projects.formError
});

const mapDispatchToProps = {
  openCreation: projectsActions.openCreation.invoke,
  create: projectsActions.create.invoke,
  goToList: projectsActions.goToList
};

export const ProjectsCreatePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsCreatePageComponent)
);
