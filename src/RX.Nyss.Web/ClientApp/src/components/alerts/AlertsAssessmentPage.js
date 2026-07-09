import styles from "./AlertsAssessment.module.scss";
import React, { useEffect, useMemo, Fragment } from "react";
import { connect, useSelector } from "react-redux";
import { withLayout } from "../../utils/layout";
import * as alertsActions from "./logic/alertsActions";
import Layout from "../layout/Layout";
import { Loading } from "../common/loading/Loading";
import { useMount } from "../../utils/lifecycle";
import { Grid, Typography } from "@material-ui/core";
import { stringKeys, strings } from "../../strings";
import DisplayField from "../forms/DisplayField";
import { AlertsAssessmentReport } from "./components/AlertsAssessmentReport";
import { assessmentStatus } from "./logic/alertsConstants";
import { AlertsAssessmentActions } from "./components/AlertsAssessmentActions";
import AlertNotificationRecipients from "./components/AlertNotificationRecipients";
import { makeStyles } from "@material-ui/core/styles";
import { sortByReportStatus } from "../../utils/sortReportByStatus";
import { AlertStatusChip } from "../common/chip/AlertStatusChip";
import { trackPageView } from "../../utils/appInsightsHelper";
import { TabMenu } from "../layout/TabMenu";

const useStyles = makeStyles((theme) => ({
  infoBox: {
    padding: 10,
    backgroundColor: theme.palette.backgroundDark.main,
    border: "1px solid #E3E3E3",
    borderRadius: 5,
    margin: "20px 0 20px 0",
    width: "fit-content",
    maxWidth: 350,
  },
}));

const isLargerThanOne = (number) => number > 1;

const AlertsAssessmentPageComponent = ({
  alertId,
  projectId,
  data,
  listState,
  ...props
}) => {
  useMount(() => {
    props.openAssessment(projectId, alertId);

    // Track page view
    trackPageView("AlertsAssessmentPage");
  });

  const classes = useStyles();

  const useRtlDirection = useSelector(
    (state) => state.appData.direction === "rtl",
  );

  const handleReset = (alertId, reportId) => {
    props.resetReport(alertId, reportId);
  };

  useEffect(() => {
    if (!props.data) {
      return;
    }
  }, [props.data, props.match]);

  const hasAccessToActions = useMemo(
    () => !!data && data.reports.filter((r) => !r.isAnonymized).length > 0,
    [data],
  );

  if (props.isFetching || !data) {
    return <Loading />;
  }

  return (
    <Fragment>
      <Grid container alignItems="center">
        <Typography variant="h3" style={{ marginRight: 10 }}>
          {props.title}
        </Typography>
        <Typography
          variant="body1"
          style={{ alignSelf: "center", marginRight: 15 }}
        >{`#${alertId}`}</Typography>
        <AlertStatusChip status={data.assessmentStatus} />
      </Grid>
      <Typography variant="body1">
        {props.subTitle}
      </Typography>
      <TabMenu/>
      <div className={styles.form}>
        {data.assessmentStatus === assessmentStatus.closed && data.comments && (
          <DisplayField
            label={strings(stringKeys.alerts.assess.closeReason)}
            value={data.comments}
          />
        )}

        <Grid container className={classes.infoBox}>
          <Typography variant="body1">{data.caseDefinition}</Typography>
        </Grid>
        <Grid container className={classes.infoBox}>
          <Typography
            style={{ display: "flex", flexDirection: "row" }}
            variant="body1"
          >
            <>
              <Typography variant="body1" style={{ marginRight: 5 }}>
                {strings(stringKeys.alerts.assess.thresholdInfo.threshold)}:
              </Typography>
              <strong style={{ marginRight: 5 }}>
                {data.healthRiskCountThreshold}{" "}
                {isLargerThanOne(data.healthRiskCountThreshold)
                  ? strings(stringKeys.alerts.assess.thresholdInfo.reports)
                  : strings(stringKeys.alerts.assess.thresholdInfo.report)}
              </strong>
            </>
            {data.healthRiskDaysThreshold && (
              <>
                <Typography variant="body1" style={{ marginRight: 5 }}>
                  {strings(stringKeys.alerts.assess.thresholdInfo.in)}
                </Typography>
                <strong style={{ marginRight: 5 }}>
                  {data.healthRiskDaysThreshold}{" "}
                  {isLargerThanOne(data.healthRiskDaysThreshold)
                    ? strings(stringKeys.alerts.assess.thresholdInfo.days)
                    : strings(stringKeys.alerts.assess.thresholdInfo.day)}
                </strong>
              </>
            )}
            {data.healthRiskKilometersThreshold && (
              <>
                <Typography variant="body1" style={{ marginRight: 5 }}>
                  {strings(stringKeys.alerts.assess.thresholdInfo.within)}
                </Typography>
                <strong style={{ marginRight: 5 }}>
                  {data.healthRiskKilometersThreshold}{" "}
                  {strings(stringKeys.alerts.assess.thresholdInfo.kilometers)}
                </strong>
              </>
            )}
          </Typography>
        </Grid>

        {data.recipientsNotified && data.escalatedTo.length > 0 && (
          <>
            <DisplayField
              label={`${strings(stringKeys.alerts.assess.escalatedTo.title)}:`}
              value=""
            />
            <AlertNotificationRecipients recipients={data.escalatedTo} />
          </>
        )}

        <Typography variant="h4" style={{ marginTop: 20 }}>
          {strings(stringKeys.alerts.assess.reports)}
        </Typography>

        <Grid container spacing={2}>
          {data.reports.sort(sortByReportStatus).map((report) => (
            <Grid item xs={12} key={`report_${report.id}`}>
              <AlertsAssessmentReport
                report={report}
                alertId={alertId}
                status={data.assessmentStatus}
                acceptReport={props.acceptReport}
                dismissReport={props.dismissReport}
                resetReport={handleReset}
                projectIsClosed={props.projectIsClosed}
                escalatedAt={data.escalatedAt}
                rtl={useRtlDirection}
              />
            </Grid>
          ))}
        </Grid>

        <AlertsAssessmentActions
          alertId={alertId}
          projectId={projectId}
          alertAssessmentStatus={data.assessmentStatus}
          isNationalSocietyEidsrEnabled={data.isNationalSocietyEidsrEnabled}
          hasAccess={hasAccessToActions}
          goToList={props.goToList}
          closeAlert={props.closeAlert}
          isClosing={props.isClosing}
          escalateAlert={props.escalateAlert}
          isEscalating={props.isEscalating}
          dismissAlert={props.dismissAlert}
          isDismissing={props.isDismissing}
          fetchRecipients={props.fetchRecipients}
          isFetchingRecipients={props.isFetchingRecipients}
          validateEidsr={props.validateEidsr}
          validateEidsrResult={props.validateEidsrResult}
          isLoadingValidateEidsr={props.isLoadingValidateEidsr}
          isPendingAlertState={props.isPendingAlertState}
          notificationEmails={props.notificationEmails}
          notificationPhoneNumbers={props.notificationPhoneNumbers}
          listState={listState}
        />
      </div>
    </Fragment>
  );
};

AlertsAssessmentPageComponent.propTypes = {};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  alertId: ownProps.match.params.alertId,
  isFetching: state.alerts.formFetching,
  isSaving: state.alerts.formSaving,
  isEscalating: state.alerts.formEscalating,
  isClosing: state.alerts.formClosing,
  isDismissing: state.alerts.formDismissing,
  isFetchingRecipients: state.alerts.isFetchingRecipients,
  data: state.alerts.formData,
  notificationEmails: state.alerts.notificationEmails,
  notificationPhoneNumbers: state.alerts.notificationPhoneNumbers,
  projectIsClosed: state.appData.siteMap.parameters.projectIsClosed,
  isPendingAlertState: state.alerts.isPendingAlertState,
  validateEidsrResult: state.alerts.formData?.validateEidsrResult,
  isLoadingValidateEidsr: state.alerts.isLoadingValidateEidsr,
  title: state.appData.siteMap.parameters.title,
  subTitle: state.appData.siteMap.parameters.subTitle,
  listState: ownProps.location?.state?.alertsListState,
});

const mapDispatchToProps = {
  goToList: alertsActions.goToList,
  openAssessment: alertsActions.openAssessment.invoke,
  acceptReport: alertsActions.acceptReport.invoke,
  dismissReport: alertsActions.dismissReport.invoke,
  resetReport: alertsActions.resetReport.invoke,
  escalateAlert: alertsActions.escalateAlert.invoke,
  closeAlert: alertsActions.closeAlert.invoke,
  dismissAlert: alertsActions.dismissAlert.invoke,
  fetchRecipients: alertsActions.fetchRecipients.invoke,
  validateEidsr: alertsActions.validateEidsr.invoke,
};

export const AlertsAssessmentPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(AlertsAssessmentPageComponent),
);
