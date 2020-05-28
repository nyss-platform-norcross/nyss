import React, { useState, Fragment, useMemo, useCallback } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as nationalSocietyUsersActions from './logic/nationalSocietyUsersActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import Button from "@material-ui/core/Button";
import { Loading } from '../common/loading/Loading';
import { useMount } from '../../utils/lifecycle';
import SelectField from '../forms/SelectField';
import Grid from '@material-ui/core/Grid';
import * as roles from '../../authentication/roles';
import { stringKeys, strings, stringsFormat } from '../../strings';
import { getBirthDecades } from '../dataCollectors/logic/dataCollectorsService';
import MenuItem from "@material-ui/core/MenuItem";
import { sexValues } from './logic/nationalSocietyUsersConstants';
import { ValidationMessage } from '../forms/ValidationMessage';
import { MultiSelect } from '../forms/MultiSelect';

const NationalSocietyUsersEditPageComponent = (props) => {
  const [birthDecades] = useState(getBirthDecades());
  const [selectedRole, setRole] = useState(null);
  const [alertRecipientsDataSource, setAlertRecipientsDataSource] = useState([]);
  const [selectedAlertRecipients, setSelectedAlertRecipients] = useState([]);
  const [alertRecipientsFieldError, setAlertRecipientsFieldError] = useState(null);

  useMount(() => {
    props.openEdition(props.nationalSocietyUserId);
  });

  const hasAnyRole = useCallback((...roles) =>
    props.callingUserRoles.some(userRole => roles.some(role => role === userRole)),
    [props.callingUserRoles]
  );

  const form = useMemo(() => {
    if (!props.data) {
      return null;
    }

    const fields = {
      id: props.data.id,
      nationalSocietyId: parseInt(props.nationalSocietyId),
      role: props.data.role,
      name: props.data.name,
      phoneNumber: props.data.phoneNumber,
      additionalPhoneNumber: props.data.additionalPhoneNumber,
      organization: props.data.organization,
      decadeOfBirth: props.data.decadeOfBirth ? props.data.decadeOfBirth.toString() : "",
      projectId: props.data.projectId ? props.data.projectId.toString() : "",
      organizationId: props.data.organizationId ? props.data.organizationId.toString() : "",
      sex: props.data.sex ? props.data.sex : ""
    };

    const validation = {
      name: [validators.required, validators.maxLength(100)],
      phoneNumber: [validators.required, validators.maxLength(20), validators.phoneNumber],
      additionalPhoneNumber: [validators.maxLength(20), validators.phoneNumber],
      organization: [validators.requiredWhen(f => f.role === roles.DataConsumer), validators.maxLength(100)],
      decadeOfBirth: [validators.requiredWhen(f => f.role === roles.Supervisor)],
      sex: [validators.requiredWhen(f => f.role === roles.Supervisor)],
      projectId: [validators.requiredWhen(f => f.role === roles.Supervisor)],
      organizationId: [validators.requiredWhen(f => f.role === roles.Coordinator || f.role === roles.GlobalCoordinator)]
    };

    setRole(props.data.role);

    if (props.data.role === roles.Supervisor && props.data.projectId !== null) {
      const currentAlertRecipients = props.data.currentProject.alertRecipients;
      const allAlertRecipientsForProject = props.data.editSupervisorFormData.availableProjects.filter(p => p.id === props.data.projectId)[0].alertRecipients;
      const allRecipients = { label: strings(stringKeys.nationalSocietyUser.form.alertRecipientsAll), value: 0, data: { id: 0 }};

      setAlertRecipientsDataSource([allRecipients, ...allAlertRecipientsForProject.map(ar => ({ label: `${ar.organization} - ${ar.role}`, value: ar.id, data: ar }))]);

      setSelectedAlertRecipients(currentAlertRecipients.length === allAlertRecipientsForProject.length ? [allRecipients.data] : currentAlertRecipients);
    }

    return createForm(fields, validation);
  }, [props.data, props.nationalSocietyId]);

  const onProjectChange = useCallback(projectId => {
    const project = props.data.editSupervisorFormData.availableProjects.filter(p => p.id === parseInt(projectId))[0];
    const newAlertRecipientsDataSource = [{ label: strings(stringKeys.nationalSocietyUser.form.alertRecipientsAll), value: 0, data: { id: 0 } }, ...project.alertRecipients.map(ar => ({ label: `${ar.organization} - ${ar.role}`, value: ar.id, data: ar }))];
    setAlertRecipientsDataSource(newAlertRecipientsDataSource);
    setSelectedAlertRecipients([]);
  }, [props.data]);

  const onAlertRecipientsChange = useCallback((_, eventData) => {
    let newAlertRecipients = [];
    if (eventData.action === "select-option") {
      newAlertRecipients = [...selectedAlertRecipients, eventData.option.data];
      setSelectedAlertRecipients([...selectedAlertRecipients, eventData.option.data]);
    } else if (eventData.action === "remove-value" || eventData.action === "pop-value") {
      newAlertRecipients = selectedAlertRecipients.filter(sar => sar.id !== eventData.removedValue.value);
      setSelectedAlertRecipients(selectedAlertRecipients.filter(sar => sar.id !== eventData.removedValue.value));
    } else if (eventData.action === "clear") {
      setSelectedAlertRecipients([]);
    }

    const allRecipientsChosen = newAlertRecipients.filter(ar => ar.id === 0).length > 0;
    if (newAlertRecipients.length === 0) {
      setAlertRecipientsFieldError(strings(stringKeys.validation.fieldRequired));
    } else if (allRecipientsChosen && newAlertRecipients.length > 1) {
      setAlertRecipientsFieldError(strings(stringKeys.nationalSocietyUser.form.alertRecipientsAllNotAllowed));
    } else {
      setAlertRecipientsFieldError(null);
    }
  }, [selectedAlertRecipients]);

  const canChangeOrganization = useMemo(() =>
    hasAnyRole(roles.Administrator) && selectedRole !== roles.DataConsumer,
    [hasAnyRole, selectedRole]);

  const setSupervisorAlertRecipients = useCallback(role => {
    if (role !== roles.Supervisor) {
      return null;
  }

    return selectedAlertRecipients[0].id === 0 ? alertRecipientsDataSource.map(ards => ards.data.id).filter(id => id !== 0) : selectedAlertRecipients.map(sar => sar.id);
  }, [alertRecipientsDataSource, selectedAlertRecipients]);

  const handleSubmit = useCallback(e => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    if (selectedRole === roles.Supervisor) {
      if (alertRecipientsFieldError !== null) {
        return;
      }
      if (selectedAlertRecipients.length === 0) {
        setAlertRecipientsFieldError(strings(stringKeys.validation.fieldRequired));
        return;
      }
    }

    const values = form.getValues();

    props.edit(props.nationalSocietyId, {
      ...values,
      organizationId: values.organizationId ? parseInt(values.organizationId) : null,
      projectId: values.projectId ? parseInt(values.projectId) : null,
      decadeOfBirth: values.decadeOfBirth ? parseInt(values.decadeOfBirth) : null,
      supervisorAlertRecipients: setSupervisorAlertRecipients(values.role)
    });
  }, [form, props, selectedRole, selectedAlertRecipients, setSupervisorAlertRecipients, alertRecipientsFieldError]);

  if (!props.data || !form || props.isFetching ) {
    return <Loading />;
  }

  return (
    <Fragment>
      {props.error && <ValidationMessage message={props.error} />}

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={3}>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.nationalSocietyUser.form.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.nationalSocietyUser.form.phoneNumber)}
              name="phoneNumber"
              field={form.fields.phoneNumber}
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.nationalSocietyUser.form.additionalPhoneNumber)}
              name="additionalPhoneNumber"
              field={form.fields.additionalPhoneNumber}
            />
          </Grid>

          {canChangeOrganization && (
            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.nationalSocietyUser.form.organization)}
                field={form.fields.organizationId}
                name="organizationId"
              >
                {props.organizations.map(organization => (
                  <MenuItem key={`organization_${organization.id}`} value={organization.id.toString()}>
                    {organization.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}

          {selectedRole !== roles.Coordinator && (
            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.nationalSocietyUser.form.customOrganization)}
                name="organization"
                field={form.fields.organization}
              />
            </Grid>
          )}

          {selectedRole === roles.Supervisor && (
            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.nationalSocietyUser.form.decadeOfBirth)}
                field={form.fields.decadeOfBirth}
                name="decadeOfBirth"
              >
                {birthDecades.map(decade => (
                  <MenuItem key={`birthDecade_${decade}`} value={decade}>
                    {decade}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}

          {selectedRole === roles.Supervisor && (
            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.nationalSocietyUser.form.sex)}
                field={form.fields.sex}
                name="sex"
              >
                {sexValues.map(type => (
                  <MenuItem key={`sex${type}`} value={type}>
                    {strings(stringKeys.dataCollector.constants.sex[type.toLowerCase()])}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}

          {selectedRole === roles.Supervisor && (
            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.nationalSocietyUser.form.project)}
                field={form.fields.projectId}
                name="projectId"
                onChange={e => onProjectChange(e.target.value)}
              >
                {props.data.editSupervisorFormData.availableProjects.map(project => (
                  <MenuItem key={`project_${project.id}`} value={project.id.toString()}>
                    {project.isClosed
                      ? stringsFormat(stringKeys.nationalSocietyUser.form.projectIsClosed, { projectName: project.name })
                      : project.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}

          {selectedRole === roles.Supervisor && (
            <Grid item xs={12}>
              <MultiSelect
                label={strings(stringKeys.nationalSocietyUser.form.alertRecipients)}
                options={alertRecipientsDataSource}
                value={alertRecipientsDataSource.filter(ar => (selectedAlertRecipients.some(sar => sar.id === ar.value)))}
                onChange={onAlertRecipientsChange}
                error={alertRecipientsFieldError}
              />
            </Grid>
          )}
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList(props.nationalSocietyId)}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.nationalSocietyUser.form.update)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

NationalSocietyUsersEditPageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyUserId: ownProps.match.params.nationalSocietyUserId,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  organizations: state.nationalSocietyUsers.formOrganizations,
  isFetching: state.nationalSocietyUsers.formFetching,
  isSaving: state.nationalSocietyUsers.formSaving,
  data: state.nationalSocietyUsers.formData,
  callingUserRoles: state.appData.user.roles,
  error: state.nationalSocietyUsers.formError
});

const mapDispatchToProps = {
  openEdition: nationalSocietyUsersActions.openEdition.invoke,
  edit: nationalSocietyUsersActions.edit.invoke,
  goToList: nationalSocietyUsersActions.goToList
};

export const NationalSocietyUsersEditPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyUsersEditPageComponent)
);
