import React, { useState, Fragment, useEffect } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm, useCustomErrors } from '../../utils/forms';
import * as smsGatewaysActions from './logic/smsGatewaysActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import SelectInput from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import Button from "@material-ui/core/Button";
import { smsGatewayTypes, smsEagle } from "./logic/smsGatewayTypes";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { ValidationMessage } from '../forms/ValidationMessage';
import CheckboxField from '../forms/CheckboxField';
import Icon from "@material-ui/core/Icon";
import { Typography } from '@material-ui/core';

const SmsGatewaysCreatePageComponent = (props) => {
  const [useIotHub, setUseIotHub] = useState(null);
  const [selectedIotDevice, setSelectedIotDevice] = useState("");
  const [pingIsRequired, setPingIsRequired] = useState(null);
  const [form] = useState(() => {
    const fields = {
      name: "",
      apiKey: "",
      gatewayType: smsEagle,
      emailAddress: "",
      useIotHub: false,
      iotHubDeviceName: ""
    };

    const validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)],
      apiKey: [validators.required, validators.minLength(1), validators.maxLength(100)],
      gatewayType: [validators.required],
      emailAddress: [validators.emailWhen(_ => _.gatewayType.toString() === smsEagle && _.useIotHub === false)],
      iotHubDeviceName: [validators.requiredWhen(x => x.useIotHub === true)]
    };


    const newForm = createForm(fields, validation)
    newForm.fields.useIotHub.subscribe(({ newValue }) => setUseIotHub(newValue));
    newForm.fields.iotHubDeviceName.subscribe(({ newValue }) => setSelectedIotDevice(newValue));
    return newForm;
  });

  useMount(() => {
    props.openCreation(props.nationalSocietyId);
  })

  const { listAvailableIotDevices } = props;
  useEffect(() => {
    if (useIotHub) {
      listAvailableIotDevices();
    }
  }, [useIotHub, listAvailableIotDevices]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };


    if (useIotHub && (!pingResult || !pingResult.isSuccess)) {
      setPingIsRequired(strings(stringKeys.smsGateway.form.pingIsRequired))
      return;
    }

    const values = form.getValues();
    props.create(props.nationalSocietyId, {
      name: values.name,
      apiKey: values.apiKey,
      gatewayType: values.gatewayType,
      emailAddress: values.emailAddress,
      iotHubDeviceName: values.useIotHub ? values.iotHubDeviceName : null
    });
  };

  const pingResult = props.pinging[selectedIotDevice] &&
    props.pinging[selectedIotDevice].result

  const handlePing = () => {
    setPingIsRequired(null);
    props.pingIotDevice(form.fields.iotHubDeviceName.value);
  }

  useCustomErrors(form, props.error);

  return (
    <Fragment>
      {props.error && !props.error.data && <ValidationMessage message={props.error.message} />}

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={2}>
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
              inputMode={"email"}
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
          <Grid item xs={12}>
            <CheckboxField
              label={strings(stringKeys.smsGateway.form.useIotHub)}
              name="useIotHub"
              field={form.fields.useIotHub}>
            </CheckboxField>
          </Grid>

          {form.fields.useIotHub.value && (
            <Fragment>
              <Grid item xs={12}>
                <SelectInput
                  label={strings(stringKeys.smsGateway.form.iotHubDeviceName)}
                  name="iotHubDeviceName"
                  field={form.fields.iotHubDeviceName}
                >
                  {(props.data && props.data.iotHubDeviceName ? [...props.availableIoTDevices, props.data.iotHubDeviceName] : props.availableIoTDevices).sort().map(deviceName => (
                    <MenuItem
                      key={`iotDevice${deviceName}`}
                      value={deviceName}>
                      {deviceName}
                    </MenuItem>
                  ))}
                </SelectInput>
              </Grid>
              <Grid item xs>
                <SubmitButton regular onClick={handlePing} isFetching={props.pinging[form.fields.iotHubDeviceName.value] && props.pinging[form.fields.iotHubDeviceName.value].pending}>
                  {strings(stringKeys.smsGateway.form.ping)}
                </SubmitButton>
                {pingResult && (
                  <Typography variant="body1" display="inline">
                    {pingResult.isSuccess ?
                      <Icon style={{ verticalAlign: "middle", color: '#004d13' }}>check</Icon> :
                      <Icon style={{ verticalAlign: "middle", color: '#C02C2C' }}>error</Icon>} {pingResult.message}
                  </Typography>
                )}
                {pingIsRequired && <Typography variant="body1" display="inline">{pingIsRequired}</Typography>}
              </Grid>
            </Fragment>
          )}
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
  error: state.smsGateways.formError,
  pinging: state.smsGateways.pinging,
  availableIoTDevices: state.smsGateways.availableIoTDevices
});

const mapDispatchToProps = {
  openCreation: smsGatewaysActions.openCreation.invoke,
  create: smsGatewaysActions.create.invoke,
  goToList: smsGatewaysActions.goToList,
  pingIotDevice: smsGatewaysActions.pingIotDevice.invoke,
  listAvailableIotDevices: smsGatewaysActions.listAvailableIotDevices.invoke
};

export const SmsGatewaysCreatePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(SmsGatewaysCreatePageComponent)
);
