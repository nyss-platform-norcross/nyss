import React, { useEffect, useState, Fragment } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as smsGatewaysActions from './logic/smsGatewaysActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import SelectInput from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import Button from "@material-ui/core/Button";
import { Loading } from '../common/loading/Loading';
import { smsGatewayTypes, smsEagle } from "./logic/smsGatewayTypes";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { ValidationMessage } from '../forms/ValidationMessage';
import CheckboxField from '../forms/CheckboxField';
import Icon from "@material-ui/core/Icon";
import { Typography } from '@material-ui/core';

const SmsGatewaysEditPageComponent = (props) => {
  const [form, setForm] = useState(null);
  const [useIotHub, setUseIotHub] = useState(null);
  const [selectedIotDevice, setSelectedIotDevice] = useState(null);
  const [pingIsRequired, setPingIsRequired] = useState(null);

  useMount(() => {
    props.openEdition(props.nationalSocietyId, props.smsGatewayId);
  });

  useEffect(() => {
    if (props.availableIoTDevices === undefined || props.availableIoTDevices.length === 0) {
      props.listAvailableIotDevices();
    }
  }, [useIotHub]);

  useEffect(() => {
    if (!props.data) {
      return;
    }

    setSelectedIotDevice(props.data && props.data.iotHubDeviceName);
    setUseIotHub(props.data && props.data.iotHubDeviceName != null);

    const fields = {
      id: props.data.id,
      name: props.data.name,
      apiKey: props.data.apiKey,
      gatewayType: props.data.gatewayType.toString(),
      emailAddress: props.data.emailAddress,
      useIotHub: props.data.iotHubDeviceName != null,
      iotHubDeviceName: props.data.iotHubDeviceName
    };

    const validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)],
      apiKey: [validators.required, validators.minLength(1), validators.maxLength(100)],
      gatewayType: [validators.required],
      emailAddress: [validators.emailWhen(_ => _.gatewayType.toString() === smsEagle)],
      iotHubDeviceName: [validators.requiredWhen(x => x.useIotHub === true)]
    };

    const newForm = createForm(fields, validation)
    newForm.fields.useIotHub.subscribe(({ newValue }) => setUseIotHub(newValue));
    newForm.fields.iotHubDeviceName.subscribe(({ newValue }) => setSelectedIotDevice(newValue));
    setForm(newForm);
  }, [props.data, props.match]);

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
    props.edit(props.nationalSocietyId, {
      id: values.id,
      name: values.name,
      apiKey: values.apiKey,
      gatewayType: values.gatewayType,
      emailAddress: values.emailAddress,
      iotHubDeviceName: values.useIotHub ? values.iotHubDeviceName : null
    });
  };

  if (props.isFetching || !form) {
    return <Loading />;
  }

  const pingResult = props.pinging[selectedIotDevice] &&
    props.pinging[selectedIotDevice].result

  const handlePing = () => {
    setPingIsRequired(null);
    props.pingIotDevice(form.fields.iotHubDeviceName.value);
  }

  return (
    <Fragment>
      {props.error && <ValidationMessage message={props.error} />}

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
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.smsGateway.form.update)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

SmsGatewaysEditPageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  smsGatewayId: ownProps.match.params.smsGatewayId,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isFetching: state.smsGateways.formFetching,
  isSaving: state.smsGateways.formSaving,
  data: state.smsGateways.formData,
  error: state.smsGateways.formError,
  pinging: state.smsGateways.pinging,
  availableIoTDevices: state.smsGateways.availableIoTDevices
});

const mapDispatchToProps = {
  openEdition: smsGatewaysActions.openEdition.invoke,
  edit: smsGatewaysActions.edit.invoke,
  goToList: smsGatewaysActions.goToList,
  pingIotDevice: smsGatewaysActions.pingIotDevice.invoke,
  listAvailableIotDevices: smsGatewaysActions.listAvailableIotDevices.invoke
};

export const SmsGatewaysEditPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(SmsGatewaysEditPageComponent)
);
