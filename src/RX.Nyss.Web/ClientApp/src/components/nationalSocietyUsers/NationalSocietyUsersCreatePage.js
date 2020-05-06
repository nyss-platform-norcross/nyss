import React, { useState, Fragment } from 'react';
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
import { userRoles, globalCoordinatorUserRoles, coordinatorUserRoles, sexValues } from './logic/nationalSocietyUsersConstants';
import * as roles from '../../authentication/roles';
import { getBirthDecades } from '../dataCollectors/logic/dataCollectorsService';
import SelectField from '../forms/SelectField';
import { ValidationMessage } from '../forms/ValidationMessage';

const NationalSocietyUsersCreatePageComponent = (props) => {
  const [birthDecades] = useState(getBirthDecades());
  const [role, setRole] = useState(null);

  const [form] = useState(() => {
    const fields = {
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
      organizationId: [validators.requiredWhen(f => f.role === roles.Coordinator || f.role === roles.GlobalCoordinator)]
    };

    setRole(roles.Manager);
    const newForm = createForm(fields, validation);
    newForm.fields.role.subscribe(({ newValue }) => setRole(newValue));

    return newForm;
  });

  useMount(() => {
    props.openCreation(props.nationalSocietyId);
  })

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();

    props.create(props.nationalSocietyId, {
      ...values,
      organizationId: values.organizationId ? parseInt(values.organizationId) : null,
      projectId: values.projectId ? parseInt(values.projectId) : null,
      decadeOfBirth: values.decadeOfBirth ? parseInt(values.decadeOfBirth) : null,
      setAsHeadManager: props.callingUserRoles.some(r => r === roles.Coordinator) ? true : null
    });
  };

  const isCoordinator = () =>
    props.callingUserRoles.some(r => r === roles.Coordinator);

  const getAvailableUserRoles = () => {
    if (props.callingUserRoles.some(r => r === roles.GlobalCoordinator || r === roles.Administrator)){
      return globalCoordinatorUserRoles;
    }

    if (isCoordinator()) {
      return coordinatorUserRoles;
    }

    return userRoles;
  }

  const availableUserRoles = getAvailableUserRoles();

  if (!props.organizations) {
    return null;
  }

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


          {(props.callingUserRoles.some(r => r === roles.Administrator || r === roles.GlobalCoordinator || r === roles.Coordinator) && role !== roles.DataConsumer) && (
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
              >
                {props.projects.map(project => (
                  <MenuItem key={`project_${project.id}`} value={project.id.toString()}>
                    {project.name}
                  </MenuItem>
                ))}
              </SelectField>
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
  projects: state.nationalSocietyUsers.formProjects,
  organizations: state.nationalSocietyUsers.formOrganizations,
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
