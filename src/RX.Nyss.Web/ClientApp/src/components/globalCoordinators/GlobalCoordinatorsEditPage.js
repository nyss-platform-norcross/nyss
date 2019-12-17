import React, { useEffect, useState, Fragment } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as globalCoordinatorsActions from './logic/globalCoordinatorsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import Button from "@material-ui/core/Button";
import { Loading } from '../common/loading/Loading';
import { useMount } from '../../utils/lifecycle';
import Grid from '@material-ui/core/Grid';
import { strings, stringKeys } from '../../strings';

const GlobalCoordinatorsEditPageComponent = (props) => {
  const [form, setForm] = useState(null);

  useMount(() => {
    props.openEdition(props.match);
  });

  useEffect(() => {
    if (!props.data) {
      return;
    }

    const fields = {
      id: props.data.id,
      name: props.data.name,
      phoneNumber: props.data.phoneNumber,
      additionalPhoneNumber: props.data.additionalPhoneNumber,
      organization: props.data.organization
    };

    const validation = {
      name: [validators.required, validators.maxLength(100)],
      phoneNumber: [validators.required, validators.maxLength(20), validators.phoneNumber],
      additionalPhoneNumber: [validators.maxLength(20), validators.phoneNumber],
      organization: [validators.maxLength(100)]
    };

    setForm(createForm(fields, validation));
  }, [props.data, props.match]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    props.edit(values);
  };

  if (props.isFetching || !form) {
    return <Loading />;
  }

  return (
    <Fragment>
      <Form onSubmit={handleSubmit}>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.globalCoordinator.form.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.globalCoordinator.form.phoneNumber)}
              name="phoneNumber"
              field={form.fields.phoneNumber}
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.globalCoordinator.form.additionalPhoneNumber)}
              name="additionalPhoneNumber"
              field={form.fields.additionalPhoneNumber}
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.globalCoordinator.form.organization)}
              name="organization"
              field={form.fields.organization}
            />
          </Grid>
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList()}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.globalCoordinator.form.update)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

GlobalCoordinatorsEditPageComponent.propTypes = {
};

const mapStateToProps = state => ({
  isFetching: state.globalCoordinators.formFetching,
  isSaving: state.globalCoordinators.formSaving,
  data: state.globalCoordinators.formData
});

const mapDispatchToProps = {
  openEdition: globalCoordinatorsActions.openEdition.invoke,
  goToList: globalCoordinatorsActions.goToList,
  edit: globalCoordinatorsActions.edit.invoke
};

export const GlobalCoordinatorsEditPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(GlobalCoordinatorsEditPageComponent)
);
