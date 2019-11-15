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

const ProjectsCreatePageComponent = (props) => {
  const [healthRiskDataSource, setHealthRiskDataSource] = useState([]);
  const [selectedHealthRisks, setSelectedHealthRisks] = useState([]);
  const [notificationEmails, setNotificationEmails] = useState([]);

  useEffect(() => {
    setHealthRiskDataSource(props.healthRisks.map(hr => ({ label: hr.healthRiskName, value: hr.healthRiskId, data: hr })));
  }, [props.healthRisks])

  const [form] = useState(() => {
    const fields = {
      name: "",
      timeZone: ""
    };

    const validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)],
      timeZone: [validators.required, validators.minLength(1), validators.maxLength(50)]
    };

    return createForm(fields, validation);
  });

  useMount(() => {
    props.openCreation(props.nationalSocietyId);
  })

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    props.create(props.nationalSocietyId, getSaveFormModel(form.getValues(), selectedHealthRisks, notificationEmails));
  };

  const onHealthRiskChange = (value, eventData) => {
    if (eventData.action === "select-option") {
      setSelectedHealthRisks([...selectedHealthRisks, eventData.option.data]);
    } else if (eventData.action === "remove-value") {
      setSelectedHealthRisks(selectedHealthRisks.filter(hr => hr.id !== eventData.removedValue.value));
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

  return (
    <Fragment>
      <Typography variant="h2">{strings(stringKeys.project.form.creationTitle)}</Typography>

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
              onChange={onHealthRiskChange}
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
