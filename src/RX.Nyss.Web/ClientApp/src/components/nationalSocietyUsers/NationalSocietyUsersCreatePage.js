import React, { useState, Fragment, useMemo } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as nationalSocietyUsersActions from './logic/nationalSocietyUsersActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import SelectInput from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import Button from "@material-ui/core/Button";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { userRoles, globalCoordinatorUserRoles, coordinatorUserRoles, headManagerRoles, allUserRoles sexValues } from './logic/nationalSocietyUsersConstants';
import * as roles from '../../authentication/roles';
import { getBirthDecades } from '../dataCollectors/logic/dataCollectorsService';
import SelectField from '../forms/SelectField';
import { ValidationMessage } from '../forms/ValidationMessage';
import { MultiSelect } from '../forms/MultiSelect';

const NationalSocietyUsersCreatePageComponent = (props) => {
  const [birthDecades] = useState(getBirthDecades());
  const [role, setRole] = useState(null);
  const [alertRecipientsDataSource, setAlertRecipientsDataSource] = useState([]);
  const [selectedAlertRecipients, setSelectedAlertRecipients] = useState([]);
  const [alertRecipientsFieldTouched, setAlertRecipientsFieldTouched] = useState(false);
  const [alertRecipientsFieldError, setAlertRecipientsFieldError] = useState(null);

  const form = useMemo(() => {
    const fields = {
      nationalSocietyId: parseInt(props.nationalSocietyId),
      role: roles.Manager,
      name: "",
      email: "",
      phoneNumber: "",
      additionalPhoneNumber: "",
      organization: "",
      decadeOfBirth: "",
      projectId: "",
      sex: "",
      organizationId: ""
    };

    const validation = {
      role: [validators.required],
      name: [validators.required, validators.maxLength(100)],
      email: [validators.required, validators.email, validators.maxLength(100)],
      phoneNumber: [validators.required, validators.maxLength(20), validators.phoneNumber],
      additionalPhoneNumber: [validators.maxLength(20), validators.phoneNumber],
      organization: [validators.requiredWhen(f => f.role === roles.DataConsumer), validators.maxLength(100)],
      decadeOfBirth: [validators.requiredWhen(f => f.role === roles.Supervisor)],
      sex: [validators.requiredWhen(f => f.role === roles.Supervisor)],
      projectId: [validators.requiredWhen(f => f.role === roles.Supervisor)],
      organizationId: [validators.requiredWhen(f => canChangeOrganization())]
    };

    setRole(roles.Manager);
    const newForm = createForm(fields, validation);
    newForm.fields.role.subscribe(({ newValue }) => setRole(newValue));

    return newForm;
  }, [props.data, props.nationalSocietyId, canChangeOrganization]);

  useMount(() => {
    props.openCreation(props.nationalSocietyId);
  });

  const onProjectChange = (projectId) => {
    const project = props.projects.filter(p => p.id === parseInt(projectId))[0];
    const newAlertRecipientsDataSource = [{ label: 'All recipients', value: 0, data: { id: 0 }}, ...project.alertRecipients.map(ar => ({ label: `${ar.organization} - ${ar.role}`, value: ar.id, data: ar }))];
    setAlertRecipientsDataSource(newAlertRecipientsDataSource);
  }

  const onAlertRecipientsChange = (value, eventData) => {
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
      setAlertRecipientsFieldError('Field is required');
    } else if (allRecipientsChosen && newAlertRecipients.length > 1) {
      setAlertRecipientsFieldError('"All recipients" cannot be selected in addition to other recipients');
    } else {
      setAlertRecipientsFieldError(null);
    }
  }

  const setSupervisorAlertRecipients = (role) => {
    if (role !== roles.Supervisor) {
      return null;
    }

    return selectedAlertRecipients[0].id === 0 ? alertRecipientsDataSource.map(ards => ards.data.id).filter(id => id !== 0) : selectedAlertRecipients.map(sar => sar.id);
  }

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    if (role === roles.Supervisor) {
      if (alertRecipientsFieldError !== null) {
        return;
      }
      if (!alertRecipientsFieldTouched) {
        setAlertRecipientsFieldError("Value is reqired");
        return;
      }
    }

    const values = form.getValues();

    props.create(props.nationalSocietyId, {
      ...values,
      organizationId: values.organizationId ? parseInt(values.organizationId) : null,
      projectId: values.projectId ? parseInt(values.projectId) : null,
      decadeOfBirth: values.decadeOfBirth ? parseInt(values.decadeOfBirth) : null,
      setAsHeadManager: props.callingUserRoles.some(r => r === roles.Coordinator) ? true : null,
      supervisorAlertRecipients: setSupervisorAlertRecipients(values.role)
    });
  };

  const canChangeOrganization = () =>
    props.data
    && props.callingUserRoles.some(r => r === roles.Administrator || r === roles.Coordinator || ((r === roles.Manager || r === roles.TechnicalAdvisor) && props.data.isHeadManager))
    && role !== roles.DataConsumer
    && !props.data.hasCoordinator;

  const isCoordinator = () =>
    props.callingUserRoles.some(r => r === roles.Coordinator);

  const getAvailableUserRoles = () => {
    if (props.callingUserRoles.some(r => r === roles.GlobalCoordinator)) {
      return globalCoordinatorUserRoles.filter(r => !props.data.hasCoordinator || r !== roles.Coordinator);
    }

    if (props.callingUserRoles.some(r => r === roles.Administrator)) {
      return allUserRoles;
    }

    if (isCoordinator()) {
      return coordinatorUserRoles;
    }

    if (props.data.isHeadManager) {
      return headManagerRoles.filter(r => !props.data.hasCoordinator || r !== roles.Coordinator);
    }

    return userRoles;
  }

  if (!props.data) {
    return null;
  }

  const availableUserRoles = getAvailableUserRoles();

  return (
    <Fragment>
      {props.error && <ValidationMessage message={props.error} />}

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.nationalSocietyUser.form.email)}
              name="email"
              field={form.fields.email}
              autoFocus
            />
          </Grid>

          <Grid item xs={12}>
            <SelectInput
              label={strings(stringKeys.nationalSocietyUser.form.role)}
              name="role"
              field={form.fields.role}
            >
              {availableUserRoles.map(role => (
                <MenuItem
                  key={`role${role}`}
                  value={role}>
                  {strings(`role.${((isCoordinator() && role === roles.Manager) ? "headManager" : role).toLowerCase()}`)}
                </MenuItem>
              ))}
            </SelectInput>
          </Grid>

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


          {canChangeOrganization() && (
            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.nationalSocietyUser.form.organization)}
                field={form.fields.organizationId}
                name="organizationId"
              >
                {props.data.organizations.map(organization => (
                  <MenuItem key={`organization_${organization.id}`} value={organization.id.toString()}>
                    {organization.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.nationalSocietyUser.form.customOrganization)}
              name="organization"
              field={form.fields.organization}
            />
          </Grid>

          {role === roles.Supervisor && (
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

          {role === roles.Supervisor && (
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

          {role === roles.Supervisor && (
            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.nationalSocietyUser.form.project)}
                field={form.fields.projectId}
                name="projectId"
                onChange={e => onProjectChange(e.target.value)}
              >
                {props.projects.map(project => (
                  <MenuItem key={`project_${project.id}`} value={project.id.toString()}>
                    {project.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}

          {role === roles.Supervisor && (
            <Grid item xs={12}>
              <MultiSelect
                label={strings(stringKeys.nationalSocietyUser.form.alertRecipients)}
                options={alertRecipientsDataSource}
                onChange={onAlertRecipientsChange}
                onBlur={e => setAlertRecipientsFieldTouched(true)}
                error={alertRecipientsFieldError}
              />
            </Grid>
          )}
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList(props.nationalSocietyId)}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.nationalSocietyUser.form.create)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  data: state.nationalSocietyUsers.formAdditionalData,
  isSaving: state.nationalSocietyUsers.formSaving,
  error: state.nationalSocietyUsers.formError,
  callingUserRoles: state.appData.user.roles
});

const mapDispatchToProps = {
  openCreation: nationalSocietyUsersActions.openCreation.invoke,
  create: nationalSocietyUsersActions.create.invoke,
  goToList: nationalSocietyUsersActions.goToList
};

export const NationalSocietyUsersCreatePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyUsersCreatePageComponent)
);
