import React, { Fragment, useEffect, useState } from "react";
import { connect, useSelector } from "react-redux";
import * as eidsrIntegrationActions from "./logic/eidsrIntegrationActions";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import FormActions from "../forms/formActions/FormActions";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import Form from "../forms/form/Form";
import { Loading } from "../common/loading/Loading";
import { createForm, validators } from "../../utils/forms";
import { ValidationMessage } from "../forms/ValidationMessage";
import {
  Button,
  CircularProgress,
  Grid,
  MenuItem,
  Tooltip,
  Typography,
} from "@material-ui/core";
import TextInputField from "../forms/TextInputField";
import styles from "./EidsrIntegration.module.scss";
import { EidsrIntegrationNotEnabled } from "./components/EidsrIntegrationNotEnabled";
import PasswordInputField from "../forms/PasswordInputField";
import { EidsrIntegrationEditPageDistrictsComponent } from "./components/EidsrIntegratonEditPageDistricts";
import CheckCircleIcon from "@material-ui/icons/CheckCircle";
import WarningIcon from "@material-ui/icons/Warning";
import LiveHelpIcon from "@material-ui/icons/LiveHelp";
import { districtValidator } from "./components/districtValidator";
import { trackPageView } from "../../utils/appInsightsHelper";

const EidsrIntegrationEditPageComponent = (props) => {
  const [form, setForm] = useState(null);
  const [integrationEditingDisabled, setIntegrationEditingDisabled] =
    useState(true);

  useMount(() => {
    props.getEidsrIntegration(props.nationalSocietyId);

    // Track page view
    trackPageView("EidsrIntegrationEditPage");
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
      suspectedDiseaseDataElementId:
        props.data.suspectedDiseaseDataElementId ?? "",
      eventTypeDataElementId: props.data.eventTypeDataElementId ?? "",
      genderDataElementId: props.data.genderDataElementId ?? "",
      districtsWithOrganizationUnits:
        props.data.districtsWithOrganizationUnits ?? [],

      reportLocationDataElementId: props.data.reportLocationDataElementId ?? "",
      reportGeoLocationDataElementId: props.data.reportGeoLocationDataElementId ?? "",
      reportHealthRiskDataElementId: props.data.reportHealthRiskDataElementId ?? "",
      reportSuspectedDiseaseDataElementId: props.data.reportSuspectedDiseaseDataElementId ?? "",
      reportStatusDataElementId: props.data.reportStatusDataElementId ?? "",
      reportGenderDataElementId: props.data.reportGenderDataElementId ?? "",
      reportAgeGroupDataElementId: props.data.reportAgeGroupDataElementId ?? "",
      reportCaseCountFemaleAgeAtLeastFiveDataElementId: props.data.reportCaseCountFemaleAgeAtLeastFiveDataElementId ?? "",
      reportCaseCountMaleAgeAtLeastFiveDataElementId: props.data.reportCaseCountMaleAgeAtLeastFiveDataElementId ?? "",
      reportCaseCountFemaleAgeBelowFiveDataElementId: props.data.reportCaseCountFemaleAgeBelowFiveDataElementId ?? "",
      reportCaseCountMaleAgeBelowFiveDataElementId: props.data.reportCaseCountMaleAgeBelowFiveDataElementId ?? "",
      reportDateDataElementId: props.data.reportDateDataElementId ?? "",
      reportTimeDataElementId: props.data.reportTimeDataElementId ?? "",
      reportDataCollectorIdDataElementId: props.data.reportDataCollectorIdDataElementId ?? ""
    };

    const validation = {
      username: [validators.required],
      password: [validators.required],
      apiBaseUrl: [validators.required],
      trackerProgramId: [validators.required],
      districtsWithOrganizationUnits: [districtValidator.allOrganisationUnits],
    };

    setForm(createForm(fields, validation));
  }, [props.data]);

  useEffect(() => {
    enableIntegrationEditing();
  }, [props.program]);

  const enableIntegrationEditing = () => {
    if (
      form != null &&
      props?.program?.name != null &&
      props?.program?.name !== ""
    ) {
      setIntegrationEditingDisabled(false);
      let formApiProperties = getFormApiProperties();
      props.getOrganisationUnits(
        formApiProperties,
        formApiProperties.ProgramId,
      );
    } else {
      setIntegrationEditingDisabled(true);
    }
  };

  const testConnection = () => {
    let formApiProperties = getFormApiProperties();
    props.getProgram(formApiProperties, formApiProperties.ProgramId);
  };

  const updateDistrictsField = (newValue) => {
    if (form == null) return;

    form.fields.districtsWithOrganizationUnits.value = newValue;
  };

  const getFormApiProperties = () => {
    const values = form.getValues();

    return {
      Url: values?.apiBaseUrl,
      UserName: values?.username,
      Password: values?.password,
      ProgramId: values?.trackerProgramId,
    };
  };

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

      reportLocationDataElementId: values.reportLocationDataElementId,
      reportGeoLocationDataElementId: values.reportGeoLocationDataElementId,
      reportHealthRiskDataElementId: values.reportHealthRiskDataElementId,
      reportSuspectedDiseaseDataElementId:
        values.reportSuspectedDiseaseDataElementId,
      reportStatusDataElementId: values.reportStatusDataElementId,
      reportGenderDataElementId: values.reportGenderDataElementId,
      reportAgeGroupDataElementId: values.reportAgeGroupDataElementId,
      reportCaseCountFemaleAgeAtLeastFiveDataElementId: values.reportCaseCountFemaleAgeAtLeastFiveDataElementId,
      reportCaseCountMaleAgeAtLeastFiveDataElementId: values.reportCaseCountMaleAgeAtLeastFiveDataElementId,
      reportCaseCountFemaleAgeBelowFiveDataElementId: values.reportCaseCountFemaleAgeBelowFiveDataElementId,
      reportCaseCountMaleAgeBelowFiveDataElementId: values.reportCaseCountMaleAgeBelowFiveDataElementId,
      reportDateDataElementId: values.reportDateDataElementId,
      reportTimeDataElementId: values.reportTimeDataElementId,
      reportDataCollectorIdDataElementId: values.reportDataCollectorIdDataElementId
    });
  };

  if (props.isFetching || !form) {
    return <Loading />;
  }

  if (!props.isEnabled) {
    return <EidsrIntegrationNotEnabled />;
  }

  return (
    <Fragment>
      {props.formError && !props.formError.data && (
        <ValidationMessage message={props.formError.message} />
      )}

      <Form fullWidth onSubmit={handleSubmit}>
        <Grid container spacing={2} direction="column">
          <Grid item xs={4}>
            <TextInputField
              label={strings(stringKeys.eidsrIntegration.form.userName)}
              name="username"
              field={form.fields.username}
              autoFocus
              subscribeOnChange={(e, val) =>
                setIntegrationEditingDisabled(true)
              }
            />
          </Grid>
          <Grid item xs={4}>
            <PasswordInputField
              label={strings(stringKeys.login.password)}
              name="password"
              field={form.fields.password}
              subscribeOnChange={(e, val) =>
                setIntegrationEditingDisabled(true)
              }
            />
          </Grid>
          <Grid item xs={4}>
            <TextInputField
              label={strings(stringKeys.eidsrIntegration.form.apiBaseUrl)}
              name="apiBaseUrl"
              field={form.fields.apiBaseUrl}
              subscribeOnChange={(e, val) =>
                setIntegrationEditingDisabled(true)
              }
            />
          </Grid>
          <Grid item xs={4}>
            <TextInputField
              label={strings(stringKeys.eidsrIntegration.form.trackerProgramId)}
              name="trackerProgramId"
              field={form.fields.trackerProgramId}
              subscribeOnChange={(e, val) =>
                setIntegrationEditingDisabled(true)
              }
            />
          </Grid>

          <Grid item container xs={4} spacing={2}>
            <Grid item xs={5}>
              <Button
                color="primary"
                variant="contained"
                size={"small"}
                onClick={() => testConnection()}
              >
                {strings(stringKeys.eidsrIntegration.form.testConn)}
              </Button>
            </Grid>
            <Grid item xs={7}>
              {props.programIsFetching && (
                <CircularProgress color="inherit" size={20} />
              )}
              {!props.programIsFetching && integrationEditingDisabled && (
                <Typography>
                  {" "}
                  {strings(stringKeys.eidsrIntegration.form.connNotTested)}{" "}
                  <WarningIcon fontSize="small" color={"error"} />
                </Typography>
              )}
              {!props.programIsFetching && !integrationEditingDisabled && (
                <Typography>
                  {props?.program?.name}{" "}
                  <CheckCircleIcon
                    style={{ color: "green", fontSize: "inherit" }}
                  />
                </Typography>
              )}
            </Grid>
          </Grid>
          <Grid item xs={6}>
            <hr className={styles.divider} />
            <div className={styles.header}>
              {strings(stringKeys.eidsrIntegration.form.dataElements)}
              {integrationEditingDisabled && (
                <Tooltip
                  title={strings(
                    stringKeys.eidsrIntegration.form.testConnToContinue,
                  )}
                >
                  <LiveHelpIcon fontSize="small" style={{ color: "#bbb" }} />
                </Tooltip>
              )}
            </div>
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings(
                stringKeys.eidsrIntegration.form.locationDataElementId,
              )}
              name="locationDataElementId"
              field={form.fields.locationDataElementId}
            />
          </Grid>
          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings(
                stringKeys.eidsrIntegration.form.dateOfOnsetDataElementId,
              )}
              name="dateOfOnsetDataElementId"
              field={form.fields.dateOfOnsetDataElementId}
            />
          </Grid>
          <div hidden={true}>
            <Grid item xs={4}>
              <TextInputField
                disabled={integrationEditingDisabled}
                label={strings(
                  stringKeys.eidsrIntegration.form.phoneNumberDataElementId,
                )}
                name="phoneNumberDataElementId"
                field={form.fields.phoneNumberDataElementId}
              />
            </Grid>
          </div>
          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings(
                stringKeys.eidsrIntegration.form.suspectedDiseaseDataElementId,
              )}
              name="suspectedDiseaseDataElementId"
              field={form.fields.suspectedDiseaseDataElementId}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings(
                stringKeys.eidsrIntegration.form.eventTypeDataElementId,
              )}
              name="eventTypeDataElementId"
              field={form.fields.eventTypeDataElementId}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings(
                stringKeys.eidsrIntegration.form.genderDataElementId,
              )}
              name="genderDataElementId"
              field={form.fields.genderDataElementId}
            />
          </Grid>

          <Grid item xs={6}>
            <hr className={styles.divider} />
            <div className={styles.header}>
              {strings("Data Elements for Reports")}
              {integrationEditingDisabled && (
                <Tooltip
                  title={strings(
                    stringKeys.eidsrIntegration.form.testConnToContinue,
                  )}
                >
                  <LiveHelpIcon fontSize="small" style={{ color: "#bbb" }} />
                </Tooltip>
              )}
            </div>
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings("Report Location")}
              name="reportLocationDataElementId"
              field={form.fields.reportLocationDataElementId}
            />
          </Grid>
          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings("Report Geo Location")}
              name="reportGeoLocationDataElementId"
              field={form.fields.reportGeoLocationDataElementId}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings("Report Health Risk")}
              name="reportHealthRiskDataElementId"
              field={form.fields.reportHealthRiskDataElementId}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings("Report Suspected Disease")}
              name="reportSuspectedDiseaseDataElementId"
              field={form.fields.reportSuspectedDiseaseDataElementId}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings("Report Status")}
              name="reportStatusDataElementId"
              field={form.fields.reportStatusDataElementId}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings("Report Gender")}
              name="reportGenderDataElementId"
              field={form.fields.reportGenderDataElementId}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings("Report Age Group")}
              name="reportAgeGroupDataElementId"
              field={form.fields.reportAgeGroupDataElementId}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings("Report Case Count Female Age At Least 5")}
              name="reportCaseCountFemaleAgeAtLeastFiveDataElementId"
              field={form.fields.reportCaseCountFemaleAgeAtLeastFiveDataElementId}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings("Report Case Count Male Age At Least 5")}
              name="reportCaseCountMaleAgeAtLeastFiveDataElementId"
              field={form.fields.reportCaseCountMaleAgeAtLeastFiveDataElementId}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings("Report Case Count Female Age Below 5")}
              name="reportCaseCountFemaleAgeBelowFiveDataElementId"
              field={form.fields.reportCaseCountFemaleAgeBelowFiveDataElementId}
            />
          </Grid>
          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings("Report Case Count Male Age Below 5")}
              name="reportCaseCountMaleAgeBelowFiveDataElementId"
              field={form.fields.reportCaseCountMaleAgeBelowFiveDataElementId}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings("Report Date")}
              name="reportDateDataElementId"
              field={form.fields.reportDateDataElementId}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings("Report Time")}
              name="reportTimeDataElementId"
              field={form.fields.reportTimeDataElementId}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              disabled={integrationEditingDisabled}
              label={strings("Report Data Collector ID")}
              name="reportDataCollectorIdDataElementId"
              field={form.fields.reportDataCollectorIdDataElementId}
            />
          </Grid>

          <Grid item xs={6}>
            <hr className={styles.divider} />
            <div className={styles.header}>
              {strings(stringKeys.eidsrIntegration.form.districts)}
              {integrationEditingDisabled && (
                <Tooltip
                  title={strings(
                    stringKeys.eidsrIntegration.form.testConnToContinue,
                  )}
                >
                  <LiveHelpIcon fontSize="small" style={{ color: "#bbb" }} />
                </Tooltip>
              )}
            </div>
          </Grid>

          <Grid item md={6} xs={10}>
            <div>
              <EidsrIntegrationEditPageDistrictsComponent
                districtsWithOrganizationUnits={
                  form.fields.districtsWithOrganizationUnits.value
                }
                integrationEditingDisabled={integrationEditingDisabled}
                organisationUnits={props.organisationUnits}
                organisationUnitsIsFetching={props.organisationUnitsIsFetching}
                onChange={(newValue) => {
                  updateDistrictsField(newValue);
                }}
              />
            </div>
            <div className={styles.districtInput}>
              <TextInputField
                disabled={integrationEditingDisabled}
                label={strings(
                  stringKeys.eidsrIntegration.form
                    .districtsWithOrganizationUnits,
                )}
                name="districtsWithOrganizationUnits"
                field={form.fields.districtsWithOrganizationUnits}
              />
            </div>
          </Grid>
        </Grid>

        <FormActions>
          <CancelButton
            onClick={() => props.goToEidsrIntegration(props.nationalSocietyId)}
          >
            {strings(stringKeys.form.cancel)}
          </CancelButton>
          <SubmitButton
            isFetching={props.formSaving}
            disabled={integrationEditingDisabled}
          >
            {" "}
            {strings(stringKeys.common.buttons.update)}{" "}
          </SubmitButton>
          {integrationEditingDisabled && (
            <Tooltip
              title={strings(
                stringKeys.eidsrIntegration.form.testConnToContinue,
              )}
            >
              <LiveHelpIcon fontSize="small" style={{ color: "#bbb" }} />
            </Tooltip>
          )}
        </FormActions>
      </Form>
    </Fragment>
  );
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,

  data: state.eidsrIntegration.data,
  isFetching: state.eidsrIntegration.isFetching,
  formError: state.eidsrIntegration.formError,
  formSaving: state.eidsrIntegration.formSaving,

  isEnabled:
    state.appData.siteMap.parameters.nationalSocietyEnableEidsrIntegration,
  callingUserRoles: state.appData.user.roles,
  nationalSocietyIsArchived:
    state.appData.siteMap.parameters.nationalSocietyIsArchived,
  nationalSocietyHasCoordinator:
    state.appData.siteMap.parameters.nationalSocietyHasCoordinator,

  organisationUnits: state.eidsrIntegration.organisationUnits,
  organisationUnitsIsFetching:
    state.eidsrIntegration.organisationUnitsIsFetching,

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
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(EidsrIntegrationEditPageComponent),
);
