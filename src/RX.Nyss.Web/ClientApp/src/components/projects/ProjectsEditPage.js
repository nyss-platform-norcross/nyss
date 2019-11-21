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
import { ProjectNotificationItem } from './ProjectNotificationItem';
import { getSaveFormModel } from './logic/projectsService';
import { Loading } from '../common/loading/Loading';

const ProjectsEditPageComponent = (props) => {
  const [healthRiskDataSource, setHealthRiskDataSource] = useState([]);
  const [selectedHealthRisks, setSelectedHealthRisks] = useState([]);
  const [notificationEmails, setNotificationEmails] = useState([]);

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
      timeZone: props.data.timeZone
    };

    let validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)],
      timeZone: [validators.required, validators.minLength(1), validators.maxLength(50)]
    };

    setForm(createForm(fields, validation));
    setSelectedHealthRisks(props.data.projectHealthRisks);
    setNotificationEmails(props.data.alertRecipients.map(ar => ({ id: ar.id, email: ar.email, key: ar.id })));
  }, [props.data]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (selectedHealthRisks.length === 0){
      return;
    }

    if (!form.isValid()) {
      return;
    };

    props.edit(props.nationalSocietyId, props.projectId, getSaveFormModel(form.getValues(), selectedHealthRisks, notificationEmails));
  };

  const onHealthRiskChange = (value, eventData) => {
    if (eventData.action === "select-option") {
      setSelectedHealthRisks([...selectedHealthRisks, eventData.option.data]);
    } else if (eventData.action === "remove-value") {
      setSelectedHealthRisks(selectedHealthRisks.filter(hr => hr.healthRiskId !== eventData.removedValue.value));
    } else if (eventData.action === "clear") {
      setSelectedHealthRisks([]);
    }
  }

  const onNotificationEmailAdd = () => {
    setNotificationEmails([
      ...notificationEmails,
      {
        key: new Date().getTime(),
        id: null,
        email: ""
      }
    ])
  }

  const onNotificationEmailRemove = (key) =>
    setNotificationEmails(notificationEmails.filter(e => e.key !== key));

  if (props.isFetching || !form) {
    return <Loading />;
  }

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
            />
          </Grid>

          <Grid item xs={9}>
            <TextInputField
              label={strings(stringKeys.project.form.timeZone)}
              name="timeZone"
              field={form.fields.timeZone}
            />
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

          <Grid item xs={9}>
            <Typography variant="h3">{strings(stringKeys.project.form.notificationsSetion)}</Typography>
            <Typography variant="subtitle1">{strings(stringKeys.project.form.notificationDescription)}</Typography>

            {notificationEmails.map(notification => (
              <ProjectNotificationItem
                key={`projectNotificationItem_${notification.key}`}
                itemKey={notification.key}
                form={form}
                notification={notification}
                onRemove={() => onNotificationEmailRemove(notification.key)}
              />
            ))}
          </Grid>
          <Grid item xs={9}>
            <Button startIcon={<AddIcon />} onClick={onNotificationEmailAdd}>{strings(stringKeys.project.form.addEmail)}</Button>
          </Grid>
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList(props.nationalSocietyId)}>{strings(stringKeys.form.cancel)}</Button>
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
  goToList: projectsActions.goToList
};

export const ProjectsEditPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsEditPageComponent)
);
