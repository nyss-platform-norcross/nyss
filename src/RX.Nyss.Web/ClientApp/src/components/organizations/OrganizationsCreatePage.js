import React, { useState, Fragment } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm, useCustomErrors } from '../../utils/forms';
import * as organizationsActions from './logic/organizationsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import Button from "@material-ui/core/Button";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { ValidationMessage } from '../forms/ValidationMessage';

const OrganizationsCreatePageComponent = (props) => {
  const [form] = useState(() => {
    const fields = {
      name: ""
    };

    const validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)]
    };

    return createForm(fields, validation)
  });

  useCustomErrors(form, props.error);

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
      name: values.name,
      nationalSocietyId: parseInt(props.nationalSocietyId)
    });
  };

  return (
    <Fragment>
      {props.error && !props.error.data && <ValidationMessage message={props.error.message} />}

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={3}>
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
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.organization.form.create)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

OrganizationsCreatePageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isSaving: state.organizations.formSaving,
  error: state.organizations.formError
});

const mapDispatchToProps = {
  openCreation: organizationsActions.openCreation.invoke,
  create: organizationsActions.create.invoke,
  goToList: organizationsActions.goToList
};

export const OrganizationsCreatePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(OrganizationsCreatePageComponent)
);
