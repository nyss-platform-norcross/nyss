import React, {Fragment, useEffect, useState} from 'react';
import { connect, useSelector } from "react-redux";
import * as eidsrIntegrationActions from './logic/eidsrIntegrationActions';
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import FormActions from "../forms/formActions/FormActions";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import Form from "../forms/form/Form";
import {Loading} from "../common/loading/Loading";
import {createForm, validators} from "../../utils/forms";
import {ValidationMessage} from "../forms/ValidationMessage";
import {Button, CircularProgress, Grid, MenuItem, Tooltip, Typography} from "@material-ui/core";
import TextInputField from "../forms/TextInputField";
import styles from "./EidsrIntegration.module.scss";
import {EidsrIntegrationNotEnabled} from "./components/EidsrIntegrationNotEnabled";
import PasswordInputField from "../forms/PasswordInputField";
import {EidsrIntegrationEditPageDistrictsComponent} from "./components/EidsrIntegratonEditPageDistricts";
import CheckCircleIcon from "@material-ui/icons/CheckCircle";
import WarningIcon from "@material-ui/icons/Warning";
import LiveHelpIcon from '@material-ui/icons/LiveHelp';

const EidsrIntegrationEditPageComponent = (props) => {
  const [form, setForm] = useState(null);
  const [integrationEditingDisabled, setIntegrationEditingDisabled] = useState(true);

  useMount(() => {
    props.getEidsrIntegration(props.nationalSocietyId);
  });

  useEffect(() => {
    if (!props.data) {
      return;
    }

    const fields = {
      username: props.data.username ?? "",
      password: props.data.password ?? "",
      apiBaseUrl: props.data.apiBaseUrl ?? "",
      trackerProgramId: props.data.trackerProgramId ?? "",
      locationDataElementId: props.data.locationDataElementId ?? "",
      dateOfOnsetDataElementId: props.data.dateOfOnsetDataElementId ?? "",
      phoneNumberDataElementId: props.data.phoneNumberDataElementId ?? "",
      suspectedDiseaseDataElementId: props.data.suspectedDiseaseDataElementId ?? "",
      eventTypeDataElementId: props.data.eventTypeDataElementId ?? "",
      genderDataElementId: props.data.genderDataElementId ?? "",
      districtsWithOrganizationUnits: props.data.districtsWithOrganizationUnits ?? [{ districtId:'1', districtName:'Jabłonowo Pomorskie', organisationUnitId:'ou481291', organisationUnitName:'Jabłonowo'},{ districtId:'2', districtName:'Oslo', organisationUnitId:'', organisationUnitName:''}],
    };
    const val = {
      req: [() => "Districts without org unit", (value) => false],
    }

    const validation = {
      username: [validators.required],
      password: [validators.required],
      apiBaseUrl: [validators.required],
      trackerProgramId: [validators.required],
      locationDataElementId: [validators.required],
      dateOfOnsetDataElementId: [validators.required],
      phoneNumberDataElementId: [validators.required],
      suspectedDiseaseDataElementId: [validators.required],
      eventTypeDataElementId: [validators.required],
      genderDataElementId: [validators.required],
      districtsWithOrganizationUnits: [val.req],
    };

    setForm(createForm(fields, validation));
  }, [props.data]);

  useEffect(() => {
    enableIntegrationEditing();
  }, [props.program]);

  const enableIntegrationEditing = () => {
    if (form != null && props?.program?.name != null && props?.program?.name !== "") {
      setIntegrationEditingDisabled(false);
      let formApiProperties = getFormApiProperties();
      props.getOrganisationUnits(formApiProperties, formApiProperties.ProgramId)
    }else{
      setIntegrationEditingDisabled(true);
    }
  }

  const testConnection = () => {
    let formApiProperties = getFormApiProperties();
    props.getProgram(formApiProperties, formApiProperties.ProgramId)
  }

  const updateDistrictsField = (newValue) => {
    if(form == null)
      return;

    form.fields.districtsWithOrganizationUnits.value = newValue;
  }

  const getFormApiProperties = () => {
    const values = form.getValues();

    return {
      Url: values?.apiBaseUrl,
      UserName: values?.username,
      Password: values?.password,
      ProgramId: values?.trackerProgramId,
    };
  }

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    }

    const values = form.getValues();

    props.editEidsrIntegration(props.nationalSocietyId, {
      username: values.username,
      password: values.password,
      apiBaseUrl: values.apiBaseUrl,
      trackerProgramId: values.trackerProgramId,
      locationDataElementId: values.locationDataElementId,
      dateOfOnsetDataElementId: values.dateOfOnsetDataElementId,
      phoneNumberDataElementId: values.phoneNumberDataElementId,
      suspectedDiseaseDataElementId: values.suspectedDiseaseDataElementId,
      eventTypeDataElementId: values.eventTypeDataElementId,
      genderDataElementId: values.genderDataElementId,
      districtsWithOrganizationUnits: values.districtsWithOrganizationUnits,
    });

  };

  if (props.isFetching || !form) {
    return <Loading />;
  }

  if(!props.isEnabled){
    return <EidsrIntegrationNotEnabled/>;
  }

  return (
    <Fragment>
      {props.formError && !props.formError.data && <ValidationMessage message={props.formError.message} />}

        <Form fullWidth onSubmit={handleSubmit}>
          <Grid container spacing={2} direction="column">




            <Grid item xs={4}>
              <TextInputField
                label={strings(stringKeys.eidsrIntegration.form.userName)}
                name="username"
                field={form.fields.username}
                autoFocus
                subscribeOnChange = {(e, val) => setIntegrationEditingDisabled(true)}
              />
            </Grid>
            <Grid item xs={4}>
              <PasswordInputField
                label={strings(stringKeys.login.password)}
                name="password"
                field={form.fields.password}
                subscribeOnChange = {(e, val) => setIntegrationEditingDisabled(true)}
              />
            </Grid>
            <Grid item xs={4}>
              <TextInputField
                label={strings(stringKeys.eidsrIntegration.form.apiBaseUrl)}
                name="apiBaseUrl"
                field={form.fields.apiBaseUrl}
                subscribeOnChange = {(e, val) => setIntegrationEditingDisabled(true)}
              />
            </Grid>
            <Grid item xs={4}>
              <TextInputField
                label={strings(stringKeys.eidsrIntegration.form.trackerProgramId)}
                name="trackerProgramId"
                field={form.fields.trackerProgramId}
                subscribeOnChange = {(e, val) => setIntegrationEditingDisabled(true)}
              />
            </Grid>

            <Grid item container xs={4}>
              <Grid item xs={5}>
                <Button color="primary" variant="contained" size={"small"} onClick={() => testConnection()}>
                  Test connection
                </Button>
              </Grid>
              <Grid item xs={7}>
                { props.programIsFetching &&
                  <CircularProgress color="inherit" size={20} />
                }
                { (!props.programIsFetching && integrationEditingDisabled) &&
                  <Typography> Connection not tested <WarningIcon fontSize="small" color={"error"}/></Typography>
                }
                { (!props.programIsFetching && !integrationEditingDisabled) &&
                  <Typography>
                    {props?.program?.name} <CheckCircleIcon style={{color: 'green', fontSize: 'inherit'}}/>
                  </Typography>
                }

              </Grid>
            </Grid>


            <Grid item xs={6}>
              <hr className={styles.divider} />
              <div className={styles.header}>
                {strings(stringKeys.eidsrIntegration.form.dataElements)}
                { integrationEditingDisabled &&
                  <Tooltip title="Test connection to continue editing">
                    <LiveHelpIcon fontSize="small" style={{color: '#bbb'}}/>
                  </Tooltip>
                }
              </div>
            </Grid>

            <Grid item xs={4}>
              <TextInputField
                disabled = {integrationEditingDisabled}
                label={strings(stringKeys.eidsrIntegration.form.locationDataElementId)}
                name="locationDataElementId"
                field={form.fields.locationDataElementId}
              />
            </Grid>
            <Grid item xs={4}>
              <TextInputField
                disabled = {integrationEditingDisabled}
                label={strings(stringKeys.eidsrIntegration.form.dateOfOnsetDataElementId)}
                name="dateOfOnsetDataElementId"
                field={form.fields.dateOfOnsetDataElementId}
              />
            </Grid>
            <Grid item xs={4}>
              <TextInputField
                disabled = {integrationEditingDisabled}
                label={strings(stringKeys.eidsrIntegration.form.phoneNumberDataElementId)}
                name="phoneNumberDataElementId"
                field={form.fields.phoneNumberDataElementId}
              />
            </Grid>
            <Grid item xs={4}>
              <TextInputField
                disabled = {integrationEditingDisabled}
                label={strings(stringKeys.eidsrIntegration.form.suspectedDiseaseDataElementId)}
                name="suspectedDiseaseDataElementId"
                field={form.fields.suspectedDiseaseDataElementId}
              />
            </Grid>
            <Grid item xs={4}>
              <TextInputField
                disabled = {integrationEditingDisabled}
                label={strings(stringKeys.eidsrIntegration.form.eventTypeDataElementId)}
                name="eventTypeDataElementId"
                field={form.fields.eventTypeDataElementId}
              />
            </Grid>
            <Grid item xs={4}>
              <TextInputField
                disabled = {integrationEditingDisabled}
                label={strings(stringKeys.eidsrIntegration.form.genderDataElementId)}
                name="genderDataElementId"
                field={form.fields.genderDataElementId}
              />
            </Grid>




            <Grid item xs={6}>
              <hr className={styles.divider} />
              <div className={styles.header}>
                {strings(stringKeys.eidsrIntegration.form.districts)}
                { integrationEditingDisabled &&
                  <Tooltip title="Test connection to continue editing">
                    <LiveHelpIcon fontSize="small" style={{color: '#bbb'}}/>
                  </Tooltip>
                }
              </div>
            </Grid>

            <Grid item md={6} xs={10}>
              <div>
                <EidsrIntegrationEditPageDistrictsComponent
                  districtsWithOrganizationUnits={form.fields.districtsWithOrganizationUnits.value}
                  integrationEditingDisabled={integrationEditingDisabled}
                  organisationUnits={props.organisationUnits}
                  organisationUnitsIsFetching={props.organisationUnitsIsFetching}
                  onChange={(newValue)=>{updateDistrictsField(newValue)}}
                />
              </div>

              <div className={styles.districtInput}>
                <TextInputField
                  disabled = {integrationEditingDisabled}
                  label={strings(stringKeys.eidsrIntegration.form.districtsWithOrganizationUnits)}
                  name="districtsWithOrganizationUnits"
                  field={form.fields.districtsWithOrganizationUnits}
                />
              </div>
            </Grid>

          </Grid>

          <FormActions>
            <CancelButton onClick={() => props.goToEidsrIntegration(props.nationalSocietyId)}>{ strings(stringKeys.form.cancel) }</CancelButton>
            <SubmitButton isFetching={props.formSaving} disabled={integrationEditingDisabled}> { strings(stringKeys.common.buttons.update) } </SubmitButton>
          </FormActions>

        </Form>

    </Fragment>
  );
}

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,

  data: state.eidsrIntegration.data,
  isFetching: state.eidsrIntegration.isFetching,
  formError: state.eidsrIntegration.formError,
  formSaving: state.eidsrIntegration.formSaving,

  isEnabled: state.appData.siteMap.parameters.nationalSocietyEnableEidsrIntegration,
  callingUserRoles: state.appData.user.roles,
  nationalSocietyIsArchived: state.appData.siteMap.parameters.nationalSocietyIsArchived,
  nationalSocietyHasCoordinator: state.appData.siteMap.parameters.nationalSocietyHasCoordinator,

  organisationUnits: state.eidsrIntegration.organisationUnits,
  organisationUnitsIsFetching: state.eidsrIntegration.organisationUnitsIsFetching,

  program: state.eidsrIntegration.program,
  programIsFetching: state.eidsrIntegration.programIsFetching,
});

const mapDispatchToProps = {
  getEidsrIntegration: eidsrIntegrationActions.get.invoke,
  goToEidsrIntegration: eidsrIntegrationActions.goToEidsrIntegration,
  editEidsrIntegration: eidsrIntegrationActions.edit.invoke,

  getOrganisationUnits: eidsrIntegrationActions.getOrganisationUnits.invoke,
  getProgram: eidsrIntegrationActions.getProgram.invoke,
};

export const EidsrIntegrationEditPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(EidsrIntegrationEditPageComponent)
);
