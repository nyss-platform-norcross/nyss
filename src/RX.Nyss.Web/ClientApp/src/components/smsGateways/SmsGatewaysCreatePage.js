import React, { useEffect, useState, Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as smsGatewaysActions from './logic/smsGatewaysActions';
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
import { SmsGatewayTypes } from "./logic/smsGatewayTypes";
import { useMount } from '../../utils/lifecycle';

const SmsGatewaysCreatePageComponent = (props) => {
  const [form] = useState(() => {
    const fields = {
      name: "",
      apiKey: "",
      gatewayType: ""
    };

    const validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)],
      apiKey: [validators.required, validators.minLength(1), validators.maxLength(100)],
      gatewayType: [validators.required]
    };

    return createForm(fields, validation);
  });

  useMount(() => {
    props.openCreation(props.match.path, props.match.params);
  })

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    props.create(props.match.params.nationalSocietyId, {
      name: values.name,
      apiKey: values.apiKey,
      gatewayType: parseInt(values.gatewayType)
    });
  };

  return (
    <Fragment>
      <Typography variant="h2">Add SMS Gateway</Typography>

      {props.error &&
        <SnackbarContent
          message={props.error}
        />
      }

      <Form onSubmit={handleSubmit}>
      <TextInputField
          label="Name"
          name="name"
          field={form.fields.name}
        />

        <TextInputField
          label="API key"
          name="apiKey"
          field={form.fields.apiKey}
        />

        <SelectInput
          label="Gateway type"
          name="gatewayType"
          field={form.fields.gatewayType}
        >
          {Object.keys(SmsGatewayTypes).map(key => (
            <MenuItem
              key={`gatewayType${key}`}
              value={key.toString()}>
              {SmsGatewayTypes[key]}
            </MenuItem>
          ))}
        </SelectInput>

        <FormActions>
          <Button onClick={() => props.goToList(props.match.params.nationalSocietyId)}>
            Cancel
          </Button>

          <SubmitButton isFetching={props.isSaving}>
            Save SMS Gateway
          </SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

SmsGatewaysCreatePageComponent.propTypes = {
};

const mapStateToProps = state => ({
  isSaving: state.smsGateways.formSaving,
  error: state.smsGateways.formError  
});

const mapDispatchToProps = {
  openCreation: smsGatewaysActions.openCreation.invoke,
  create: smsGatewaysActions.create.invoke,
  goToList: smsGatewaysActions.goToList  
};

export const SmsGatewaysCreatePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(SmsGatewaysCreatePageComponent)
);
