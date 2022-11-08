import styles from "./AlertsAssessment.module.scss";
import React, { useEffect, useMemo, Fragment } from 'react';
import { connect, useSelector } from "react-redux";
import { withLayout } from '../../utils/layout';
import * as alertsActions from './logic/alertsActions';
import Layout from '../layout/Layout';
import { Loading } from '../common/loading/Loading';
import { useMount } from '../../utils/lifecycle';
import { Grid, Divider } from '@material-ui/core';
import { stringKeys, strings } from '../../strings';
import DisplayField from "../forms/DisplayField";
import { AlertsAssessmentReport } from "./components/AlertsAssessmentReport";
import { assessmentStatus } from "./logic/alertsConstants";
import { AlertsAssessmentActions } from "./components/AlertsAssessmentActions";
import AlertNotificationRecipients from "./components/AlertNotificationRecipients";

const getAssessmentStatusInformation = (status) => {
  switch (status) {
    case assessmentStatus.open:
    case assessmentStatus.toEscalate:
    case assessmentStatus.toDismiss:
      return stringKeys.alerts.assess.introduction;

    case assessmentStatus.closed:
      return stringKeys.alerts.assess.statusDescription.closed;
    case assessmentStatus.escalated:
      return stringKeys.alerts.assess.statusDescription.escalated;
    case assessmentStatus.dismissed:
      return stringKeys.alerts.assess.statusDescription.dismissed;
    case assessmentStatus.rejected:
      return stringKeys.alerts.assess.statusDescription.rejected;
    default:
      throw new Error("Wrong status")
  }
}

const AlertsAssessmentPageComponent = ({ alertId, projectId, data, ...props }) => {
  useMount(() => {
    props.openAssessment(projectId, alertId);
  });

  const useRtlDirection = useSelector(state => state.appData.direction === 'rtl');

  const handleReset = (alertId, reportId) => {
    props.resetReport(alertId, reportId);
  }

  useEffect(() => {
    if (!props.data) {
      return;
    }

  }, [props.data, props.match]);

  const hasAccessToActions = useMemo(() => !!data && data.reports.filter(r => !r.isAnonymized).length > 0,
    [data]);

  if (props.isFetching || !data) {
    return <Loading />;
  }

  return (
    <Fragment>
      <div className={styles.form}>
        <DisplayField
          label={strings(getAssessmentStatusInformation(data.assessmentStatus))}
          value={strings(stringKeys.alerts.constants.escalatedOutcomes[data.escalatedOutcome])}
        />

        {data.assessmentStatus === assessmentStatus.closed && data.comments && (
          <DisplayField
            label={strings(stringKeys.alerts.assess.closeReason)}
            value={data.comments}
          />
        )}

        <Divider />

        <DisplayField
          label={strings(stringKeys.alerts.assess.caseDefinition)}
          value={data.caseDefinition}
        />

        <Divider />

        {data.recipientsNotified && data.escalatedTo.length > 0 && <>
          <DisplayField
            label={`${strings(stringKeys.alerts.assess.escalatedTo.title)}:`}
            value=""
          />
          <AlertNotificationRecipients recipients={data.escalatedTo} />
        </>}

        <div className={styles.reportsTitle}>{strings(stringKeys.alerts.assess.reports)}</div>

        <Grid container spacing={2}>
          {data.reports.map(report => (
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
        />
      </div>
    </Fragment>
  );
}

AlertsAssessmentPageComponent.propTypes = {
};

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
  connect(mapStateToProps, mapDispatchToProps)(AlertsAssessmentPageComponent)
);
