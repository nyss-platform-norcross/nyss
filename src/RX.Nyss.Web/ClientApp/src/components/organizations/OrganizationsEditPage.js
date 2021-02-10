import React, { useEffect, useState, Fragment } from 'react';
import { connect } from "react-redux";
import { withLayout } from '../../utils/layout';
import { validators, createForm, useCustomErrors } from '../../utils/forms';
import * as organizationsActions from './logic/organizationsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import Button from "@material-ui/core/Button";
import { Loading } from '../common/loading/Loading';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { ValidationMessage } from '../forms/ValidationMessage';

const OrganizationsEditPageComponent = (props) => {
  const [form, setForm] = useState(null);

  useMount(() => {
    props.openEdition(props.nationalSocietyId, props.organizationId);
  });

  useEffect(() => {
    if (!props.data) {
      return;
    }

    const fields = {
      id: props.data.id,
      name: props.data.name
    };

    const validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)]
    };

    setForm(createForm(fields, validation));
  }, [props.data, props.match]);

  useCustomErrors(form, props.error);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    props.edit(props.nationalSocietyId, {
      id: values.id,
      name: values.name,
      nationalSocietyId: parseInt(props.nationalSocietyId)
    });
  };

  if (props.isFetching || !form) {
    return <Loading />;
  }

  return (
    <Fragment>
      {props.error && !props.error.data && <ValidationMessage message={props.error.message} />}

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.organization.form.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList(props.nationalSocietyId)}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.organization.form.update)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

const mapStateToProps = (state, ownProps) => ({
  organizationId: ownProps.match.params.organizationId,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isFetching: state.organizations.formFetching,
  isSaving: state.organizations.formSaving,
  data: state.organizations.formData,
  error: state.organizations.formError
});

const mapDispatchToProps = {
  openEdition: organizationsActions.openEdition.invoke,
  edit: organizationsActions.edit.invoke,
  goToList: organizationsActions.goToList
};

export const OrganizationsEditPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(OrganizationsEditPageComponent)
);
