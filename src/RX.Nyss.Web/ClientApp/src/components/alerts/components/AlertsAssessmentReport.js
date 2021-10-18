import styles from "./AlertsAssessmentReport.module.scss";

import React, { Fragment, useEffect } from 'react';
import ExpandMoreIcon from '@material-ui/icons/ExpandMore';
import { stringKeys, strings } from "../../../strings";
import dayjs from "dayjs";
import SubmitButton from "../../forms/submitButton/SubmitButton";
import { assessmentStatus } from "../logic/alertsConstants";
import {
  Button,
  CircularProgress,
  Grid,
  Accordion,
  AccordionDetails,
  AccordionSummary,
  AccordionActions,
  Divider,
  Icon,
} from "@material-ui/core";
import { useSelector } from "react-redux";
import { Manager, TechnicalAdvisor } from "../../../authentication/roles";

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

export const AlertsAssessmentReport = ({ alertId, escalatedAt, report, acceptReport, dismissReport, resetReport, status, projectIsClosed }) => {
  const currentUserRoles = useSelector(state => state.appData.user.roles);
  const showActions = status !== assessmentStatus.closed && status !== assessmentStatus.dismissed && report.status === "Pending" && !report.isAnonymized;
  const showResetOption = status !== assessmentStatus.closed
    && status !== assessmentStatus.dismissed
    && (report.status === "Accepted" || report.status === "Rejected")
    && (escalatedAt ? dayjs(report.acceptedAt || report.rejectedAt).isAfter(dayjs(escalatedAt)) : true)
    && !report.isAnonymized;

  const fromOtherOrg = report.dataCollector == null;

  const showSupervisorDetails = currentUserRoles.some(r => r === Manager || r === TechnicalAdvisor);

  return (
    <Accordion disabled={fromOtherOrg}>
      <AccordionSummary expandIcon={!fromOtherOrg && <ExpandMoreIcon />}>
        {getReportIcon(report.status)}
        <span className={styles.time}>{dayjs(report.receivedAt).format('YYYY-MM-DD HH:mm')}</span>
        <div className={styles.senderContainer}>
          <span className={styles.senderLabel}>

            {strings(stringKeys.alerts.assess.report.sender)} {report.isAnonymized && strings(stringKeys.alerts.assess.report.linkedToSupervisor)}
          </span>
          <span className={styles.sender}>{report.dataCollector || report.organization}</span>
        </div>
      </AccordionSummary>
      <AccordionDetails className={styles.form}>
        <Grid container spacing={2}>
          <Grid item xs={6} xl={3}>
            <ReportFormLabel
              label={strings(stringKeys.alerts.assess.report.phoneNumber)}
              value={report.phoneNumber}
            />
            <ReportFormLabel
              label={strings(stringKeys.alerts.assess.report.village)}
              value={report.village}
            />
            <ReportFormLabel
              label={strings(stringKeys.alerts.assess.report.district)}
              value={report.district}
            />
            <ReportFormLabel
              label={strings(stringKeys.alerts.assess.report.region)}
              value={report.region}
            />
          </Grid>
          <Grid item xs={6} xl={3}>
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
            {showSupervisorDetails && (
              <ReportFormLabel
                label={strings(stringKeys.roles.Supervisor)}
                value={`${report.supervisorName} / ${report.supervisorPhoneNumber}`}
              />
            )}
          </Grid>
        </Grid>
      </AccordionDetails>
      {!projectIsClosed && (
        <Fragment>
          <Divider />
          <AccordionActions>
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
          </AccordionActions>
        </Fragment>
      )}
    </Accordion>
  );
}
