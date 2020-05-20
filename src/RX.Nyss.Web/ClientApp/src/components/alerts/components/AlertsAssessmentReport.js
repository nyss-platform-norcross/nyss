import styles from "./AlertsAssessmentReport.module.scss";

import React, { Fragment } from 'react';
import ExpansionPanel from '@material-ui/core/ExpansionPanel';
import ExpansionPanelDetails from '@material-ui/core/ExpansionPanelDetails';
import ExpansionPanelSummary from '@material-ui/core/ExpansionPanelSummary';
import ExpansionPanelActions from '@material-ui/core/ExpansionPanelActions';
import ExpandMoreIcon from '@material-ui/icons/ExpandMore';
import Divider from '@material-ui/core/Divider';
import { stringKeys, strings } from "../../../strings";
import dayjs from "dayjs";
import Icon from "@material-ui/core/Icon";
import SubmitButton from "../../forms/submitButton/SubmitButton";
import { assessmentStatus } from "../logic/alertsConstants";
import { Button, CircularProgress } from "@material-ui/core";

const ReportFormLabel = ({ label, value }) => (
  <div className={styles.container}>
    <div className={styles.key}>{label}</div>
    <div className={styles.value}>{value}</div>
  </div>
);

const getReportIcon = (status) => {
  switch (status) {
    case "Pending": return <Icon className={styles.indicator}>hourglass_empty</Icon>;
    case "Accepted": return <Icon className={`${styles.indicator} ${styles.accepted}`}>check</Icon>;
    case "Rejected": return <Icon className={`${styles.indicator} ${styles.rejected}`}>clear</Icon>;
    case "Closed": return <Icon className={`${styles.indicator}`}>block</Icon>;
    default: return <Icon className={styles.indicator}>warning</Icon>;
  }
}

export const AlertsAssessmentReport = ({ alertId, report, acceptReport, dismissReport, resetReport, status, projectIsClosed }) => {
  const showActions = status !== assessmentStatus.closed && report.status === "Pending";
  const showResetOption = status !== assessmentStatus.closed
    && status !== assessmentStatus.dismissed
    && (report.status === "Accepted" || report.status === "Rejected");

  const fromOtherOrg = report.dataCollector == null;
  return (
    <ExpansionPanel disabled={fromOtherOrg}>
      <ExpansionPanelSummary expandIcon={!fromOtherOrg && <ExpandMoreIcon />}>
        {getReportIcon(report.status)}
        <span className={styles.time}>{dayjs(report.receivedAt).format('YYYY-MM-DD HH:mm')}</span>
        <div className={styles.senderContainer}>
          <span className={styles.senderLabel}>{strings(stringKeys.alerts.assess.report.sender)}</span>
          <span className={styles.sender}>{report.dataCollector || report.organization}</span>
        </div>
      </ExpansionPanelSummary>
      <ExpansionPanelDetails className={styles.form}>
        <ReportFormLabel
          label={strings(stringKeys.alerts.assess.report.phoneNumber)}
          value={report.phoneNumber}
        />
        <ReportFormLabel
          label={strings(stringKeys.alerts.assess.report.village)}
          value={report.village}
        />
        {report.sex && (
          <ReportFormLabel
            label={strings(stringKeys.alerts.assess.report.sex)}
            value={strings(stringKeys.alerts.constants.sex[report.sex])}
          />
        )}
        {report.age && (
          <ReportFormLabel
            label={strings(stringKeys.alerts.assess.report.age)}
            value={strings(stringKeys.alerts.constants.age[report.age])}
          />
        )}
        <ReportFormLabel
          label={strings(stringKeys.alerts.assess.report.id)}
          value={report.id}
        />
      </ExpansionPanelDetails>
      {!projectIsClosed && (
        <Fragment>
          <Divider />
          <ExpansionPanelActions>
            {showActions && (
              <Fragment>
                <SubmitButton onClick={() => dismissReport(alertId, report.id)} isFetching={report.isDismissing} regular>
                  {strings(stringKeys.alerts.assess.report.dismiss)}
                </SubmitButton>

                <SubmitButton onClick={() => acceptReport(alertId, report.id)} isFetching={report.isAccepting}>
                  {strings(stringKeys.alerts.assess.report.accept)}
                </SubmitButton>
              </Fragment>
            )}

            {!showActions && (
              <div className={styles.reportStatus}>{strings(stringKeys.alerts.constants.reportStatus[report.status])}</div>
            )}

            {showResetOption && (
              <Fragment>
                <Button variant="text" onClick={() => resetReport(alertId, report.id)} disabled={report.isResetting}>
                  {report.isResetting && <CircularProgress size={16} className={styles.progressIcon} />}
                  {strings(stringKeys.alerts.assess.report.reset)}
                </Button>
              </Fragment>
            )}
          </ExpansionPanelActions>
        </Fragment>
      )}
    </ExpansionPanel>
  );
}
