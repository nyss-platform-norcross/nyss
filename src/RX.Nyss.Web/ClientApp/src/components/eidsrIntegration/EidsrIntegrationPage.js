import React, { Fragment } from "react";
import { connect } from "react-redux";
import * as eidsrIntegrationActions from "./logic/eidsrIntegrationActions";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import { useMount } from "../../utils/lifecycle";
import { stringKeys, strings } from "../../strings";
import { accessMap } from "../../authentication/accessMap";
import FormActions from "../forms/formActions/FormActions";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import Form from "../forms/form/Form";
import { Grid, Typography } from "@material-ui/core";
import styles from "./EidsrIntegration.module.scss";
import { Loading } from "../common/loading/Loading";
import { EidsrIntegrationNotEnabled } from "./components/EidsrIntegrationNotEnabled";
import PasswordDisplayField from "../forms/PasswordDisplayField";
import { trackPageView } from "../../utils/appInsightsHelper";

const EidsrIntegrationPageComponent = (props) => {
  useMount(() => {
    props.getEidsrIntegration(props.nationalSocietyId);

    // Track page view
    trackPageView("EidsrIntegrationPage");
  });

  if (props.isFetching || !props.data) {
    return <Loading />;
  }

  if (!props.isEnabled) {
    return <EidsrIntegrationNotEnabled />;
  }

  return (
    <Fragment>
      <Form>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.eidsrIntegration.form.userName)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.username ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            {props.data.password ? (
              <PasswordDisplayField
                label={strings(stringKeys.login.password)}
                value={props.data.password}
              />
            ) : (
              <>
                <Typography variant="h6">
                  {strings(stringKeys.eidsrIntegration.form.password)}
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {props.data.password ??
                    strings(stringKeys.eidsrIntegration.form.dataNotSet)}
                </Typography>
              </>
            )}
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.eidsrIntegration.form.apiBaseUrl)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.apiBaseUrl ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.eidsrIntegration.form.trackerProgramId)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.trackerProgramId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <hr className={styles.divider} />
            <div className={styles.header}>
              {strings(stringKeys.eidsrIntegration.form.dataElements)}
            </div>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.eidsrIntegration.form.locationDataElementId)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.locationDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(
                stringKeys.eidsrIntegration.form.dateOfOnsetDataElementId,
              )}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.dateOfOnsetDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>

          <div hidden={true}>
            <Grid item xs={12}>
              <Typography variant="h6">
                {strings(
                  stringKeys.eidsrIntegration.form.phoneNumberDataElementId,
                )}
              </Typography>
              <Typography variant="body1" gutterBottom>
                {props.data.phoneNumberDataElementId ??
                  strings(stringKeys.eidsrIntegration.form.dataNotSet)}
              </Typography>
            </Grid>
          </div>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(
                stringKeys.eidsrIntegration.form.suspectedDiseaseDataElementId,
              )}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.suspectedDiseaseDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(
                stringKeys.eidsrIntegration.form.eventTypeDataElementId,
              )}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.eventTypeDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>
          
          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.eidsrIntegration.form.genderDataElementId)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.genderDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <hr className={styles.divider} />
            <div className={styles.header}>
              {strings("Data Elements for Reports")}
            </div>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h6">{strings("Report Location")}</Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.reportLocationDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h6">{strings("Report Geo Location")}</Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.reportGeoLocationDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h6">
              {strings("Report Health Risk")}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.reportHealthRiskDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h6">
              {strings("Report Suspected Disease")}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.reportSuspectedDiseaseDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h6">{strings("Report Status")}</Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.reportStatusDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h6">{strings("Report Gender")}</Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.reportGenderDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h6">{strings("Report AgeGroup")}</Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.reportAgeGroupDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h6">
              {strings("Report Case Count Female Age At Least 5")}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.reportCaseCountFemaleAgeAtLeastFiveDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h6">
              {strings("Report Case Count Male Age At Least 5")}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.reportCaseCountMaleAgeAtLeastFiveDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h6">
              {strings("Report Case Count Female Age Below 5")}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.reportCaseCountFemaleAgeBelowFiveDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h6">
              {strings("Report Case Count Male Age Below 5")}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.reportCaseCountMaleAgeBelowFiveDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h6">
              {strings("Report Date")}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.reportDateDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h6">
              {strings("Report Time")}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.reportTimeDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h6">
              {strings("Report Data Collector ID")}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.reportDataCollectorIdDataElementId ??
                strings(stringKeys.eidsrIntegration.form.dataNotSet)}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <hr className={styles.divider} />
            <div className={styles.header}>
              {strings(stringKeys.eidsrIntegration.form.districts)}
            </div>
          </Grid>

          <Grid item xs={12}>
            {props.data.districtsWithOrganizationUnits?.map((item, index) => (
              <Grid container spacing={2}>
                <Grid item xs={3}>
                  <Typography variant="caption">District</Typography>
                  <Typography>{item.districtName}</Typography>
                </Grid>
                <Grid item xs={9}>
                  <Typography variant="caption">Organisation Unit</Typography>
                  <Typography>
                    {item.organisationUnitName ??
                      strings(stringKeys.eidsrIntegration.form.dataNotSet)}
                  </Typography>
                </Grid>
              </Grid>
            ))}
            {props.data.districtsWithOrganizationUnits.length === 0 && (
              <p> {strings(stringKeys.eidsrIntegration.form.noDistricts)} </p>
            )}
          </Grid>
        </Grid>

        <FormActions>
          <TableActionsButton
            onClick={() =>
              props.goToEidsrIntegrationEdition(props.nationalSocietyId)
            }
            roles={accessMap.eidsrIntegration.edit}
            variant={"contained"}
          >
            {strings(stringKeys.common.buttons.edit)}
          </TableActionsButton>
        </FormActions>
      </Form>
    </Fragment>
  );
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,

  data: state.eidsrIntegration.data,
  isFetching: state.eidsrIntegration.isFetching,

  isEnabled:
    state.appData.siteMap.parameters.nationalSocietyEnableEidsrIntegration,
  callingUserRoles: state.appData.user.roles,
  nationalSocietyIsArchived:
    state.appData.siteMap.parameters.nationalSocietyIsArchived,
  nationalSocietyHasCoordinator:
    state.appData.siteMap.parameters.nationalSocietyHasCoordinator,
});

const mapDispatchToProps = {
  getEidsrIntegration: eidsrIntegrationActions.get.invoke,
  goToEidsrIntegrationEdition:
    eidsrIntegrationActions.goToEidsrIntegrationEdition,
};

export const EidsrIntegrationPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(EidsrIntegrationPageComponent),
);
