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
import SnackbarContent from '@material-ui/core/SnackbarContent';
import Button from "@material-ui/core/Button";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import AddIcon from '@material-ui/icons/Add';
import { MultiSelect } from '../forms/MultiSelect';
import { ProjectsHealthRiskItem } from './ProjectHealthRiskItem';
import { ProjectEmailNotificationItem } from './ProjectEmailNotificationItem';
import { ProjectSmsNotificationItem } from './ProjectSmsNotificationItem';
import { getSaveFormModel } from './logic/projectsService';
import SelectField from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";

const ProjectsCreatePageComponent = (props) => {
  const [healthRiskDataSource, setHealthRiskDataSource] = useState([]);
  const [selectedHealthRisks, setSelectedHealthRisks] = useState([]);
  const [emailNotifications, setEmailNotifications] = useState([]);
  const [smsNotifications, setSmsNotifications] = useState([]);
  const [healthRisksFieldTouched, setHealthRisksFieldTouched] = useState(false);

  useEffect(() => {
    setHealthRiskDataSource(props.healthRisks.map(hr => ({ label: hr.healthRiskName, value: hr.healthRiskId, data: hr })));
  }, [props.healthRisks])

  const [form] = useState(() => {
    const fields = {
      name: "",
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

    props.create(props.nationalSocietyId, getSaveFormModel(form.getValues(), selectedHealthRisks, emailNotifications, smsNotifications));
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

  const onEmailNotificationAdd = () => {
    setEmailNotifications([
      ...emailNotifications,
      {
        key: new Date().getTime(),
        id: null,
        email: ""
      }
    ])
  }

  const onEmailNotificationRemove = (key) =>
    setEmailNotifications(emailNotifications.filter(e => e.key !== key));

  const onSmsNotificationAdd = () => {
    setSmsNotifications([
      ...smsNotifications,
      {
        key: new Date().getTime(),
        id: null,
        phoneNumber: ""
      }
    ])
  }

  const onSmsNotificationRemove = (key) =>
    setSmsNotifications(smsNotifications.filter(e => e.key !== key));

  return (
    <Fragment>
      {props.error &&
        <SnackbarContent
          message={props.error}
        />
      }

      <Form onSubmit={handleSubmit} fullWidth style={{ maxWidth: 800 }}>
        <Grid container spacing={3}>
          <Grid item xs={9}>
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

          <Grid item xs={9}>
            <Typography variant="h3">{strings(stringKeys.project.form.emailNotificationsSetion)}</Typography>
            <Typography variant="subtitle1">{strings(stringKeys.project.form.emailNotificationDescription)}</Typography>

            {emailNotifications.map(emailNotification => (
              <ProjectEmailNotificationItem
                key={`projectEmailNotificationItem_${emailNotification.key}`}
                itemKey={emailNotification.key}
                form={form}
                emailNotification={emailNotification}
                onRemove={() => onEmailNotificationRemove(emailNotification.key)}
              />
            ))}
          </Grid>
          <Grid item xs={9}>
            <Button startIcon={<AddIcon />} onClick={onEmailNotificationAdd}>{strings(stringKeys.project.form.addEmail)}</Button>
          </Grid>

          <Grid item xs={9}>
            <Typography variant="h3">{strings(stringKeys.project.form.smsNotificationsSetion)}</Typography>
            <Typography variant="subtitle1">{strings(stringKeys.project.form.smsNotificationDescription)}</Typography>

            {smsNotifications.map(smsNotification => (
              <ProjectSmsNotificationItem
                key={`projectSmsNotificationItem_${smsNotification.key}`}
                itemKey={smsNotification.key}
                form={form}
                smsNotification={smsNotification}
                onRemove={() => onSmsNotificationRemove(smsNotification.key)}
              />
            ))}
          </Grid>
          <Grid item xs={9}>
            <Button startIcon={<AddIcon />} onClick={onSmsNotificationAdd}>{strings(stringKeys.project.form.addSms)}</Button>
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
