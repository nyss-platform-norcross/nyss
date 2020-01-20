import React, { useEffect, useState, Fragment } from 'react';
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

const NationalSocietyUsersEditPageComponent = (props) => {
  const [birthDecades] = useState(getBirthDecades());
  const [form, setForm] = useState(null);
  const [role, setRole] = useState(null);

  useMount(() => {
    props.openEdition(props.nationalSocietyUserId);
  });

  useEffect(() => {
    if (!props.data) {
      return;
    }

    const fields = {
      id: props.data.id,
      role: props.data.role,
      name: props.data.name,
      phoneNumber: props.data.phoneNumber,
      additionalPhoneNumber: props.data.additionalPhoneNumber,
      organization: props.data.organization,
      decadeOfBirth: props.data.decadeOfBirth ? props.data.decadeOfBirth.toString() : "",
      projectId: props.data.projectId ? props.data.projectId.toString() : "",
      sex: props.data.sex ? props.data.sex : ""
    };

    const validation = {
      name: [validators.required, validators.maxLength(100)],
      phoneNumber: [validators.required, validators.maxLength(20), validators.phoneNumber],
      additionalPhoneNumber: [validators.maxLength(20), validators.phoneNumber],
      organization: [validators.requiredWhen(f => f.role === roles.DataConsumer), validators.maxLength(100)],
      decadeOfBirth: [validators.requiredWhen(f => f.role === roles.Supervisor)],
      sex: [validators.requiredWhen(f => f.role === roles.Supervisor)],
      projectId: [validators.requiredWhen(f => f.role === roles.Supervisor)]
    };

    setRole(props.data.role);

    setForm(createForm(fields, validation));
  }, [props.data, props.match]);

  if (!props.data) {
    return null;
  }

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();

    props.edit(props.nationalSocietyId, {
      ...values,
      projectId: values.projectId ? parseInt(values.projectId) : null,
      decadeOfBirth: values.decadeOfBirth ? parseInt(values.decadeOfBirth) : null
    });
  };

  if (props.isFetching || !form) {
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
                {props.data.editSupervisorFormData.availableProjects.map(project => (
                  <MenuItem key={`project_${project.id}`} value={project.id.toString()}>
                    { project.isClosed
                      ? stringsFormat(stringKeys.nationalSocietyUser.form.projectIsClosed, {projectName: project.name})
                      : project.name }                     
                  </MenuItem>
                ))}
              </SelectField>
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
  isFetching: state.nationalSocietyUsers.formFetching,
  isSaving: state.nationalSocietyUsers.formSaving,
  data: state.nationalSocietyUsers.formData,
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
