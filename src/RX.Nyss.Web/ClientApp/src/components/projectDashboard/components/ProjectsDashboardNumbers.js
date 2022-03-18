import styles from "./ProjectsDashboardNumbers.module.scss";

import React from 'react';
import { Grid, Card, CardContent, CardHeader } from '@material-ui/core';
import { Loading } from '../../common/loading/Loading';
import { stringKeys, strings } from '../../../strings';

export const ProjectsDashboardNumbers = ({ isFetching, projectSummary, reportsType }) => {
  if (isFetching || !projectSummary) {
    return <Loading />;
  }

  const renderNumber = (label, value) => (
    <Grid container spacing={2}>
      <Grid item className={styles.numberName}>{label}</Grid>
      <Grid item className={styles.numberValue}>{value}</Grid>
    </Grid>
  );

  return (
    <Grid container spacing={2} data-printable={true}>
      <Grid item sm={6} md={3} xs={12} className={styles.numberBox}>
        <Card className={styles.card}>
          <CardHeader title={strings(stringKeys.dashboard.numbers.reportCountTitle)} />
          <CardContent>
            {renderNumber(strings(stringKeys.dashboard.numbers.totalReportCount), projectSummary.totalReportCount)}
          </CardContent>
        </Card>
      </Grid>

      <Grid item sm={6} md={3} xs={12} className={styles.numberBox}>
        <Card className={styles.card}>
          <CardHeader title={strings(stringKeys.dashboard.dataCollectors)} />
          <CardContent>
            {renderNumber(strings(stringKeys.dashboard.activeDataCollectorCount), projectSummary.activeDataCollectorCount)}
          </CardContent>
        </Card>
      </Grid>

      {reportsType === "dataCollectionPoint" && (
        <Grid item sm={6} md={3} xs={12} className={styles.numberBox}>
          <Card className={styles.card}>
            <CardHeader title={strings(stringKeys.dashboard.dataCollectionPoints)} />
            <CardContent>
              {renderNumber(strings(stringKeys.dashboard.referredToHospitalCount), projectSummary.dataCollectionPointSummary.referredToHospitalCount)}
              {renderNumber(strings(stringKeys.dashboard.fromOtherVillagesCount), projectSummary.dataCollectionPointSummary.fromOtherVillagesCount)}
              {renderNumber(strings(stringKeys.dashboard.deathCount), projectSummary.dataCollectionPointSummary.deathCount)}
            </CardContent>
          </Card>
        </Grid>
      )}

      {reportsType !== "dataCollectionPoint" && (
        <Grid item sm={6} md={3} xs={12} className={styles.numberBox}>
          <Card className={styles.card}>
            <CardHeader title={strings(stringKeys.dashboard.alertsSummary)} />
            <CardContent>
              {renderNumber(strings(stringKeys.dashboard.numbers.openAlerts), projectSummary.alertsSummary.open)}
              {renderNumber(strings(stringKeys.dashboard.numbers.escalatedAlerts), projectSummary.alertsSummary.escalated)}
              {renderNumber(strings(stringKeys.dashboard.numbers.closedAlerts), projectSummary.alertsSummary.closed)}
              {renderNumber(strings(stringKeys.dashboard.numbers.dismissedAlerts), projectSummary.alertsSummary.dismissed)}
            </CardContent>
          </Card>
        </Grid>
      )}

      <Grid item sm={6} md={3} xs={12} className={styles.numberBox}>
        <Card className={styles.card}>
          <CardHeader title={strings(stringKeys.dashboard.geographicalCoverageSummary)} />
          <CardContent>
            {renderNumber(strings(stringKeys.dashboard.numbers.numberOfVillages), projectSummary.numberOfVillages)}
            {renderNumber(strings(stringKeys.dashboard.numbers.numberOfDistricts), projectSummary.numberOfDistricts)}
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );
}
