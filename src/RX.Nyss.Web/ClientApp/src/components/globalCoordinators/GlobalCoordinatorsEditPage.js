import React, { useEffect, useState, Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as globalCoordinatorsActions from './logic/globalCoordinatorsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import Typography from '@material-ui/core/Typography';
import TextInputField from '../forms/TextInputField';
import Button from "@material-ui/core/Button";
import { Loading } from '../common/loading/Loading';

const GlobalCoordinatorsEditPageComponent = (props) => {
  const [form, setForm] = useState(null);

  useEffect(() => {
    props.openEdition(props.match);
  }, []);

  useEffect(() => {
    if (!props.data || props.data.id.toString() !== props.match.params.globalCoordinatorId) {
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
      name: [validators.required, validators.minLength(5)],
      phoneNumber: [validators.required, validators.minLength(8)],
      additionalPhoneNumber: [validators.minLength(8)],
      organization: [validators.minLength(3)]
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
      <Typography variant="h2">Edit Global Coordinator</Typography>

      <Form onSubmit={handleSubmit}>
        <TextInputField
          label="Name"
          name="name"
          field={form.fields.name}
        />

        <TextInputField
          label="Phone number"
          name="phoneNumber"
          field={form.fields.phoneNumber}
        />

        <TextInputField
          label="Additional phone number (optional)"
          name="additionalPhoneNumber"
          field={form.fields.additionalPhoneNumber}
        />

        <TextInputField
          label="Organization"
          name="organization"
          field={form.fields.organization}
        />

        <FormActions>
          <Button onClick={() => props.goToList()}>
            Cancel
          </Button>

          <SubmitButton isFetching={props.isSaving}>
            Save Global Coordinator
          </SubmitButton>
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
