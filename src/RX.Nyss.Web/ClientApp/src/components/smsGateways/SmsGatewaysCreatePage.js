import React, { useState, Fragment } from 'react';
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
import { smsGatewayTypes } from "./logic/smsGatewayTypes";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';

const SmsGatewaysCreatePageComponent = (props) => {
  const [form] = useState(() => {
    const fields = {
      name: "",
      apiKey: "",
      gatewayType: "SmsEagle",
      "emailAddress": ""
    };

    const validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)],
      apiKey: [validators.required, validators.minLength(1), validators.maxLength(100)],
      gatewayType: [validators.required],
      emailAddress: [validators.required, validators.email]
    };

    return createForm(fields, validation);
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
      name: values.name,
      apiKey: values.apiKey,
      gatewayType: values.gatewayType,
      emailAddress: values.emailAddress
    });
  };

  return (
    <Fragment>
      {props.error &&
        <SnackbarContent
          message={props.error}
        />
      }

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.smsGateway.form.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.smsGateway.form.apiKey)}
              name="apiKey"
              field={form.fields.apiKey}
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.smsGateway.form.emailAddress)}
              name="emailAddress"
              field={form.fields.emailAddress}
            />
          </Grid>

          <Grid item xs={12}>
            <SelectInput
              label={strings(stringKeys.smsGateway.form.gatewayType)}
              name="gatewayType"
              field={form.fields.gatewayType}
            >
              {smsGatewayTypes.map(type => (
                <MenuItem
                  key={`gatewayType${type}`}
                  value={type}>
                  {strings(`smsGateway.type.${type.toLowerCase()}`)}
                </MenuItem>
              ))}
            </SelectInput>
          </Grid>
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList(props.nationalSocietyId)}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.smsGateway.form.create)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

SmsGatewaysCreatePageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
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
