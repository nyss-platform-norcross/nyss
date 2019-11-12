import React, { useEffect, useState, Fragment } from 'react';
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
import SelectInput from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import SnackbarContent from '@material-ui/core/SnackbarContent';
import Button from "@material-ui/core/Button";
import { Loading } from '../common/loading/Loading';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';

const ProjectsEditPageComponent = (props) => {
  const [form, setForm] = useState(null);

  useMount(() => {
    props.openEdition(props.nationalSocietyId, props.projectId);
  });

  useEffect(() => {
    if (!props.data) {
      return;
    }

    const fields = {
      id: props.data.id,
      name: props.data.name,
      timeZone: props.data.timeZone
    };

    const validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)],
      timeZone: [validators.required, validators.minLength(1), validators.maxLength(50)]
    };

    setForm(createForm(fields, validation));
  }, [props.data, props.match]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    props.edit(props.nationalSocietyId, {
      id: values.id,
      name: values.name,
      timeZone: values.timeZone
    });
  };

  if (props.isFetching || !form) {
    return <Loading />;
  }

  return (
    <Fragment>
      <Typography variant="h2">{strings(stringKeys.project.form.editionTitle)}</Typography>

      {props.error &&
        <SnackbarContent
          message={props.error}
        />
      }

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.project.form.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.project.form.timeZone)}
              name="timeZone"
              field={form.fields.timeZone}
            />
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
