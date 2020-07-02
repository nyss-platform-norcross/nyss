import React, { useState, Fragment } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm, useCustomErrors } from '../../utils/forms';
import * as projectOrganizationsActions from './logic/projectOrganizationsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import Button from "@material-ui/core/Button";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { ValidationMessage } from '../forms/ValidationMessage';
import SelectField from '../forms/SelectField';
import { MenuItem } from '@material-ui/core';

const ProjectOrganizationsCreatePageComponent = (props) => {
  const [form] = useState(() => {
    const fields = {
      organizationId: ""
    };

    const validation = {
      organizationId: [validators.required]
    };

    return createForm(fields, validation)
  });

  useMount(() => {
    props.openCreation(props.projectId);
  })

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    props.create(props.projectId, {
      organizationId: parseInt(values.organizationId)
    });
  };

  useCustomErrors(form, props.error);

  if (!props.data) {
    return null;
  }

  return (
    <Fragment>
      {props.error && !props.error.data && <ValidationMessage message={props.error.message} />}

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <SelectField
              label={strings(stringKeys.projectOrganization.form.organization)}
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
        </Grid>
        <FormActions>
          <Button onClick={() => props.goToList(props.projectId)}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.projectOrganization.form.create)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

ProjectOrganizationsCreatePageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  data: state.projectOrganizations.formData,
  isSaving: state.projectOrganizations.formSaving,
  error: state.projectOrganizations.formError
});

const mapDispatchToProps = {
  openCreation: projectOrganizationsActions.openCreation.invoke,
  create: projectOrganizationsActions.create.invoke,
  goToList: projectOrganizationsActions.goToList
};

export const ProjectOrganizationsCreatePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectOrganizationsCreatePageComponent)
);
