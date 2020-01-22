import styles from "./NationalSocietyDashboardNumbers.module.scss";

import React from 'react';
import Grid from '@material-ui/core/Grid';
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import CardHeader from '@material-ui/core/CardHeader';
import { Loading } from '../../common/loading/Loading';
import { stringKeys, strings } from '../../../strings';

export const NationalSocietyDashboardNumbers = ({ isFetching, summary, reportsType }) => {
  if (isFetching || !summary) {
    return <Loading />;
  }

  const renderNumber = (label, value) => (
    <Grid container spacing={3}>
      <Grid item className={styles.numberName}>{label}</Grid>
      <Grid item className={styles.numberValue}>{value}</Grid>
    </Grid>
  );

  return (
    <Grid container spacing={3}>
      <Grid item sm={6} md={4} xs={12} className={styles.numberBox}>
        <Card className={styles.card}>
          <CardHeader title={strings(stringKeys.nationalSociety.dashboard.numbers.totalReportCountTitle)} />
          <CardContent>
            {renderNumber(strings(stringKeys.nationalSociety.dashboard.numbers.totalReportCount), summary.reportCount)}
          </CardContent>
        </Card>
      </Grid>

      <Grid item sm={6} md={4} xs={12} className={styles.numberBox}>
        <Card className={styles.card}>
          <CardHeader title={strings(stringKeys.nationalSociety.dashboard.dataCollectors)} />
          <CardContent>
            {renderNumber(strings(stringKeys.nationalSociety.dashboard.activeDataCollectorCount), summary.activeDataCollectorCount)}
            {renderNumber(strings(stringKeys.nationalSociety.dashboard.inactiveDataCollectorCount), summary.inactiveDataCollectorCount)}
          </CardContent>
        </Card>
      </Grid>

      {reportsType === "dataCollectionPoint" && (
        <Grid item sm={6} md={4} xs={12} className={styles.numberBox}>
          <Card className={styles.card}>
            <CardHeader title={strings(stringKeys.nationalSociety.dashboard.dataCollectionPoints)} />
            <CardContent>
              {renderNumber(strings(stringKeys.nationalSociety.dashboard.referredToHospitalCount), summary.dataCollectionPointSummary.referredToHospitalCount)}
              {renderNumber(strings(stringKeys.nationalSociety.dashboard.fromOtherVillagesCount), summary.dataCollectionPointSummary.fromOtherVillagesCount)}
              {renderNumber(strings(stringKeys.nationalSociety.dashboard.deathCount), summary.dataCollectionPointSummary.deathCount)}
            </CardContent>
          </Card>
        </Grid>
      )}
    </Grid>
  );
}
