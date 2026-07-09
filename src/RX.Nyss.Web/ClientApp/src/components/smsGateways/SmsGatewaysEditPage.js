import styles from "./SmsGatewaysCreateOrEditPage.module.scss";
import React, { useEffect, useState, Fragment } from "react";
import { connect } from "react-redux";
import { withLayout } from "../../utils/layout";
import { validators, createForm, useCustomErrors } from "../../utils/forms";
import * as smsGatewaysActions from "./logic/smsGatewaysActions";
import Layout from "../layout/Layout";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import PasswordInputField from "../forms/PasswordInputField";
import TextInputField from "../forms/TextInputField";
import SelectInput from "../forms/SelectField";
import { Loading } from "../common/loading/Loading";
import {
  smsGatewayTypes,
  smsEagle,
  smsGateway,
  mtnSmsGateway,
  telerivet,
} from "./logic/smsGatewayTypes";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { ValidationMessage } from "../forms/ValidationMessage";
import CheckboxField from "../forms/CheckboxField";
import {
  Typography,
  MenuItem,
  Button,
  Grid,
  Icon,
  InputAdornment,
  Snackbar,
  IconButton,
} from "@material-ui/core";
import RefreshIcon from "@material-ui/icons/Refresh";
import FileCopyIcon from "@material-ui/icons/FileCopy";
import { v4 as uuidv4 } from "uuid";
import { trackPageView } from "../../utils/appInsightsHelper";

const SmsGatewaysEditPageComponent = (props) => {
  const [form, setForm] = useState(null);
  const [useIotHub, setUseIotHub] = useState(null);
  const [selectedIotDevice, setSelectedIotDevice] = useState(null);
  const [pingIsRequired, setPingIsRequired] = useState(null);
  const [useDualModem, setUseDualModem] = useState(null);
  const [snackbarOpen, setSnackbarOpen] = useState(false);
  const [selectedGatewayType, setSelectedGatewayType] = useState(null);

  useMount(() => {
    props.openEdition(props.nationalSocietyId, props.smsGatewayId);

    // Track page view
    trackPageView("SmsGatewaysEditPage");
  });

  const { listAvailableIotDevices } = props;
  useEffect(() => {
    if (useIotHub) {
      listAvailableIotDevices();
    }
  }, [useIotHub, listAvailableIotDevices]);

  useEffect(() => {
    if (!props.data) {
      return;
    }

    setSelectedIotDevice(props.data && props.data.iotHubDeviceName);
    setUseIotHub(props.data && !!props.data.iotHubDeviceName);
    setUseDualModem(props.data && !!props.data.modemOneName);
    setSelectedGatewayType(props.data && props.data.gatewayType.toString());

    const fields = {
      id: props.data.id,
      name: props.data.name,
      apiKey: props.data.apiKey,
      gatewayType: props.data.gatewayType.toString(),
      telerivetApiKey: props.data.telerivetApiKey || "",
      telerivetProjectId: props.data.telerivetProjectId || "",
      gatewayApiKey: props.data.gatewayApiKey || "",
      gatewayApiKeyName: props.data.gatewayApiKeyName || "",
      gatewayExtraKey: props.data.gatewayExtraKey || "",
      gatewayExtraKeyName: props.data.gatewayExtraKeyName || "",
      gatewayUrl: props.data.gatewayUrl || "",
      gatewayAuthUrl: props.data.gatewayAuthUrl || "",
      gatewaySenderId: props.data.gatewaySenderId || "",
      emailAddress: props.data.emailAddress || "",
      useIotHub: !!props.data.iotHubDeviceName,
      iotHubDeviceName: props.data.iotHubDeviceName || "",
      useDualModem: !!props.data.modemOneName,
      modemOneName: props.data.modemOneName || "",
      modemTwoName: props.data.modemTwoName || "",
    };

    const validation = {
      name: [
        validators.required,
        validators.minLength(1),
        validators.maxLength(100),
      ],
      apiKey: [
        validators.required,
        validators.minLength(1),
        validators.maxLength(100),
      ],
      gatewayType: [validators.required],
      telerivetApiKey: [
        validators.requiredWhen(
          (tr) => tr.gatewayType.toString() === telerivet,
        ),
      ],
      telerivetProjectId: [
        validators.requiredWhen(
          (tp) => tp.gatewayType.toString() === telerivet,
        ),
      ],
      gatewayApiKey: [
        validators.requiredWhen(
          (tr) => tr.gatewayType.toString() === smsGateway || tr.gatewayType.toString() === mtnSmsGateway,
        ),
      ],
      gatewayApiKeyName: [
        validators.requiredWhen(
          (tp) => tp.gatewayType.toString() === smsGateway || tp.gatewayType.toString() === mtnSmsGateway,
        ),
      ],
      gatewayExtraKey: [
        validators.requiredWhen(
          (tr) => tr.gatewayType.toString() === mtnSmsGateway,
        ),
      ],
      gatewayExtraKeyName: [
        validators.requiredWhen(
          (tp) => tp.gatewayType.toString() === mtnSmsGateway,
        ),
      ],
      gatewayUrl: [
        validators.requiredWhen(
          (tr) => tr.gatewayType.toString() === smsGateway || tr.gatewayType.toString() === mtnSmsGateway,
        ),
      ],
      gatewayAuthUrl: [
        validators.requiredWhen(
          (tr) => tr.gatewayType.toString() === mtnSmsGateway,
        ),
      ],
      gatewaySenderId: [
        validators.requiredWhen(
          (tp) => tp.gatewayType.toString() === smsGateway || tp.gatewayType.toString() === mtnSmsGateway,
        ),
      ],
      emailAddress: [
        validators.emailWhen(
          (_) => _.gatewayType.toString() === smsEagle && _.useIotHub === false,
        ),
      ],
      iotHubDeviceName: [validators.requiredWhen((x) => x.useIotHub === true)],
      modemOneName: [
        validators.requiredWhen((x) => x.useDualModem === true),
        validators.maxLength(100),
      ],
      modemTwoName: [
        validators.requiredWhen((x) => x.useDualModem === true),
        validators.maxLength(100),
      ],
    };

    const newForm = createForm(fields, validation);
    newForm.fields.useIotHub.subscribe(({ newValue }) =>
      setUseIotHub(newValue),
    );
    newForm.fields.iotHubDeviceName.subscribe(({ newValue }) =>
      setSelectedIotDevice(newValue),
    );
    newForm.fields.useDualModem.subscribe(({ newValue }) =>
      setUseDualModem(newValue),
    );
    newForm.fields.gatewayType.subscribe(({ newValue }) =>
      setSelectedGatewayType(newValue),
    );
    setForm(newForm);
  }, [props.data, props.match]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    }

    if (useIotHub && (!pingResult || !pingResult.isSuccess)) {
      setPingIsRequired(strings(stringKeys.smsGateway.form.pingIsRequired));
      return;
    }

    const values = form.getValues();
    props.edit({
      id: values.id,
      nationalSocietyId: parseInt(props.nationalSocietyId),
      name: values.name,
      apiKey: values.apiKey,
      gatewayType: values.gatewayType,
      emailAddress: values.gatewayType.toString() === smsEagle
        ? values.emailAddress
        : null,
      telerivetApiKey:
        values.gatewayType.toString() === telerivet
          ? values.telerivetApiKey
          : null,
      telerivetProjectId:
        values.gatewayType.toString() === telerivet
          ? values.telerivetProjectId
          : null,
      gatewayApiKey:
        values.gatewayType.toString() === smsGateway || values.gatewayType.toString() === mtnSmsGateway
          ? values.gatewayApiKey
          : null,
      gatewayApiKeyName:
        values.gatewayType.toString() === smsGateway || values.gatewayType.toString() === mtnSmsGateway
          ? values.gatewayApiKeyName
          : null,
      gatewayExtraKey:
        values.gatewayType.toString() === smsGateway || values.gatewayType.toString() === mtnSmsGateway
          ? values.gatewayExtraKey
          : null,
      gatewayExtraKeyName:
        values.gatewayType.toString() === smsGateway || values.gatewayType.toString() === mtnSmsGateway
          ? values.gatewayExtraKeyName
          : null,
      gatewayUrl:
        values.gatewayType.toString() === smsGateway || values.gatewayType.toString() === mtnSmsGateway
          ? values.gatewayUrl : null,
      gatewayAuthUrl:
        values.gatewayType.toString() === mtnSmsGateway
          ? values.gatewayAuthUrl : null,
      gatewaySenderId:
        values.gatewayType.toString() === smsGateway || values.gatewayType.toString() === mtnSmsGateway
          ? values.gatewaySenderId
          : null,
      iotHubDeviceName: values.useIotHub ? values.iotHubDeviceName : null,
      modemOneName: values.useDualModem ? values.modemOneName : null,
      modemTwoName: values.useDualModem ? values.modemTwoName : null,
    });
  };

  useCustomErrors(form, props.error);

  if (props.isFetching || !form) {
    return <Loading />;
  }

  const gatewayType = selectedGatewayType || form.fields.gatewayType.value;
  const isSmsEagle = gatewayType === smsEagle;
  const isTelerivet = gatewayType === telerivet;
  const isSmsGatewayType =
    gatewayType === smsGateway || gatewayType === mtnSmsGateway;
  const isMtn = gatewayType === mtnSmsGateway;

  const pingResult =
    props.pinging[selectedIotDevice] && props.pinging[selectedIotDevice].result;

  const handlePing = () => {
    setPingIsRequired(null);
    props.pingIotDevice(form.fields.iotHubDeviceName.value);
  };

  const generateUuid = () => {
    form.fields.apiKey.update(uuidv4().replace(/-/g, ""));
  };

  const copyApiKey = () => {
    navigator.clipboard.writeText(form.fields.apiKey.value);
    setSnackbarOpen(true);
  };

  const handleSnackbarClose = () => {
    setSnackbarOpen(false);
  };

  return (
    <Fragment>
      {props.error && !props.error.data && (
        <ValidationMessage message={props.error.message} />
      )}

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.common.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.smsGateway.form.apiKey)}
              name="apiKey"
              field={form.fields.apiKey}
              disabled
              endAdornment={
                <InputAdornment position="end">
                  <IconButton
                    onClick={generateUuid}
                    className={styles.endAdornmentButton}
                  >
                    <RefreshIcon />
                  </IconButton>
                  <IconButton
                    onClick={copyApiKey}
                    className={styles.endAdornmentButton}
                  >
                    <FileCopyIcon />
                  </IconButton>
                </InputAdornment>
              }
            />
          </Grid>

          <Grid item xs={12}>
            <SelectInput
              label={strings(stringKeys.smsGateway.form.gatewayType)}
              name="gatewayType"
              field={form.fields.gatewayType}
            >
              {smsGatewayTypes.map((type) => (
                <MenuItem key={`gatewayType${type}`} value={type}>
                  {strings(`smsGateway.type.${type.toLowerCase()}`)}
                </MenuItem>
              ))}
            </SelectInput>
          </Grid>
          {isSmsEagle && (
            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.smsGateway.form.emailAddress)}
                name="emailAddress"
                field={form.fields.emailAddress}
                inputMode={"email"}
              />
            </Grid>
          )}

          {isTelerivet && (
            <Fragment>
              <Grid item xs={12}>
                <PasswordInputField
                  label={strings(stringKeys.smsGateway.form.telerivetApiKey)}
                  name="telerivetApiKey"
                  field={form.fields.telerivetApiKey}
                />
              </Grid>

              <Grid item xs={12}>
                <TextInputField
                  label={strings(
                    stringKeys.smsGateway.form.telerivetProjectId,
                  )}
                  name="telerivetProjectId"
                  field={form.fields.telerivetProjectId}
                />
              </Grid>
            </Fragment>
          )}

          {isSmsGatewayType && (
            <Fragment>
              <Grid item xs={12}>
                <TextInputField
                  label={strings(
                    stringKeys.smsGateway.form.gatewayApiKeyName,
                  )}
                  name="gatewayApiKeyName"
                  field={form.fields.gatewayApiKeyName}
                />
                <PasswordInputField
                  label={strings(stringKeys.smsGateway.form.gatewayApiKey)}
                  name="gatewayApiKey"
                  field={form.fields.gatewayApiKey}
                />
              </Grid>
              <Grid item xs={12}>
                <TextInputField
                  label={strings(
                    stringKeys.smsGateway.form.gatewayExtraKeyName,
                  )}
                  name="gatewayExtraKeyName"
                  field={form.fields.gatewayExtraKeyName}
                />
                <PasswordInputField
                  label={strings(stringKeys.smsGateway.form.gatewayExtraKey)}
                  name="gatewayExtraKey"
                  field={form.fields.gatewayExtraKey}
                />
              </Grid>
              <Grid item xs={12}>
                <TextInputField
                  label={strings(stringKeys.smsGateway.form.gatewayUrl)}
                  name="gatewayUrl"
                  field={form.fields.gatewayUrl}
                />
              </Grid>
              {isMtn && (
                <Grid item xs={12}>
                  <TextInputField
                    label={strings(stringKeys.smsGateway.form.gatewayAuthUrl)}
                    name="gatewayAuthUrl"
                    field={form.fields.gatewayAuthUrl}
                  />
                </Grid>
              )}
              <Grid item xs={12}>
                <TextInputField
                  label={strings(stringKeys.smsGateway.form.gatewaySenderId)}
                  name="gatewaySenderId"
                  field={form.fields.gatewaySenderId}
                />
              </Grid>
            </Fragment>
          )}

          <Grid item xs={12}>
            <CheckboxField
              label={strings(stringKeys.smsGateway.form.useIotHub)}
              name="useIotHub"
              field={form.fields.useIotHub}
              color="primary"
            ></CheckboxField>
          </Grid>

          {form.fields.useIotHub.value && (
            <Fragment>
              <Grid item xs={12}>
                <SelectInput
                  label={strings(stringKeys.smsGateway.form.iotHubDeviceName)}
                  name="iotHubDeviceName"
                  field={form.fields.iotHubDeviceName}
                >
                  {(props.data && props.data.iotHubDeviceName
                    ? [
                        ...props.availableIoTDevices,
                        props.data.iotHubDeviceName,
                      ]
                    : props.availableIoTDevices
                  )
                    .sort()
                    .map((deviceName) => (
                      <MenuItem
                        key={`iotDevice${deviceName}`}
                        value={deviceName}
                      >
                        {deviceName}
                      </MenuItem>
                    ))}
                </SelectInput>
              </Grid>
              <Grid item xs>
                <SubmitButton
                  regular
                  onClick={handlePing}
                  isFetching={
                    props.pinging[form.fields.iotHubDeviceName.value] &&
                    props.pinging[form.fields.iotHubDeviceName.value].pending
                  }
                >
                  {strings(stringKeys.smsGateway.form.ping)}
                </SubmitButton>
                {pingResult && (
                  <Typography variant="body1" display="inline">
                    {pingResult.isSuccess ? (
                      <Icon
                        style={{ verticalAlign: "middle", color: "#004d13" }}
                      >
                        check
                      </Icon>
                    ) : (
                      <Icon
                        style={{ verticalAlign: "middle", color: "#C02C2C" }}
                      >
                        error
                      </Icon>
                    )}{" "}
                    {pingResult.message}
                  </Typography>
                )}
                {pingIsRequired && (
                  <Typography variant="body1" display="inline">
                    {pingIsRequired}
                  </Typography>
                )}
              </Grid>
            </Fragment>
          )}

          <Grid item xs={12}>
            <CheckboxField
              label={strings(stringKeys.smsGateway.form.useDualModem)}
              name="useDualModem"
              field={form.fields.useDualModem}
              color="primary"
            ></CheckboxField>
          </Grid>

          {useDualModem && (
            <Fragment>
              <Grid item xs={12}>
                <TextInputField
                  label={strings(stringKeys.smsGateway.form.modemOneName)}
                  name="modemOneName"
                  field={form.fields.modemOneName}
                />
              </Grid>
              <Grid item xs={12}>
                <TextInputField
                  label={strings(stringKeys.smsGateway.form.modemTwoName)}
                  name="modemTwoName"
                  field={form.fields.modemTwoName}
                />
              </Grid>
            </Fragment>
          )}
        </Grid>

        <FormActions>
          <CancelButton onClick={() => props.goToList(props.nationalSocietyId)}>
            {strings(stringKeys.form.cancel)}
          </CancelButton>
          <SubmitButton isFetching={props.isSaving}>
            {strings(stringKeys.common.buttons.update)}
          </SubmitButton>
        </FormActions>
      </Form>

      <Snackbar
        open={snackbarOpen}
        autoHideDuration={2000}
        message={strings(stringKeys.smsGateway.apiKeyCopied)}
        onClose={handleSnackbarClose}
      />
    </Fragment>
  );
};

SmsGatewaysEditPageComponent.propTypes = {};

const mapStateToProps = (state, ownProps) => ({
  smsGatewayId: ownProps.match.params.smsGatewayId,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isFetching: state.smsGateways.formFetching,
  isSaving: state.smsGateways.formSaving,
  data: state.smsGateways.formData,
  error: state.smsGateways.formError,
  pinging: state.smsGateways.pinging,
  availableIoTDevices: state.smsGateways.availableIoTDevices,
});

const mapDispatchToProps = {
  openEdition: smsGatewaysActions.openEdition.invoke,
  edit: smsGatewaysActions.edit.invoke,
  goToList: smsGatewaysActions.goToList,
  pingIotDevice: smsGatewaysActions.pingIotDevice.invoke,
  listAvailableIotDevices: smsGatewaysActions.listAvailableIotDevices.invoke,
};

export const SmsGatewaysEditPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(SmsGatewaysEditPageComponent),
);
