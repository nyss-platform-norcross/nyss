import React, { useState, Fragment } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as nationalSocietyUsersActions from './logic/nationalSocietyUsersActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import Typography from '@material-ui/core/Typography';
import TextInputField from '../forms/TextInputField';
import SelectInput from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import SnackbarContent from '@material-ui/core/SnackbarContent';
import Button from "@material-ui/core/Button";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { userRoles, sexValues } from './logic/nationalSocietyUsersConstants';
import * as roles from '../../authentication/roles';
import { getBirthDecades } from '../dataCollectors/logic/dataCollectorsService';
import SelectField from '../forms/SelectField';

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
      sex: ""
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
      projectId: [validators.requiredWhen(f => f.role === roles.Supervisor)]
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
      projectId: values.projectId ? parseInt(values.projectId) : null,
      decadeOfBirth: values.decadeOfBirth ? parseInt(values.decadeOfBirth) : null
    });
  };

  return (
    <Fragment>
      <Typography variant="h2">{strings(stringKeys.nationalSocietyUser.form.creationTitle)}</Typography>

      {props.error &&
        <SnackbarContent
          message={props.error}
        />
      }

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
              {userRoles.map(role => (
                <MenuItem
                  key={`role${role}`}
                  value={role}>
                  {strings(`role.${role.toLowerCase()}`)}
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

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.nationalSocietyUser.form.organization)}
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

NationalSocietyUsersCreatePageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  projects: state.nationalSocietyUsers.formProjects,
  isSaving: state.nationalSocietyUsers.formSaving,
  error: state.nationalSocietyUsers.formError
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
