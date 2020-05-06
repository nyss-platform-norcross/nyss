import React, { useEffect, useState, Fragment } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as organizationsActions from './logic/organizationsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import SelectInput from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import Button from "@material-ui/core/Button";
import { Loading } from '../common/loading/Loading';
import { organizationTypes, smsEagle } from "./logic/organizationTypes";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { ValidationMessage } from '../forms/ValidationMessage';
import CheckboxField from '../forms/CheckboxField';
import Icon from "@material-ui/core/Icon";
import { Typography } from '@material-ui/core';

const OrganizationsEditPageComponent = (props) => {
  const [form, setForm] = useState(null);
  const [useIotHub, setUseIotHub] = useState(null);
  const [selectedIotDevice, setSelectedIotDevice] = useState(null);
  const [pingIsRequired, setPingIsRequired] = useState(null);

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

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    props.edit(props.nationalSocietyId, {
      id: values.id,
      name: values.name
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

OrganizationsEditPageComponent.propTypes = {
};

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

export const OrganizationsEditPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(OrganizationsEditPageComponent)
);
