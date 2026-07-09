import styles from "./ProjectsDashboardNumbers.module.scss";

import React from "react";
import { Grid, Card, CardContent, CardHeader, Typography, useMediaQuery } from "@material-ui/core";
import { Loading } from "../../common/loading/Loading";
import { stringKeys, strings } from "../../../strings";

export const ProjectsDashboardNumbers = ({
  isFetching,
  projectSummary,
  reportsType,
  isMapExpanded
}) => {
  const isSmallScreen = useMediaQuery(theme => theme.breakpoints.down("sm"));

  if (isFetching || !projectSummary) {
    return <Loading />;
  }

  const cardSizes = {
    xs: isSmallScreen || isMapExpanded ? 12 : 6,
  }

  const renderNumber = (label, value, minimalSpacing) => (
    <Grid container spacing={2} justifyContent="center" alignItems="center">
      <Grid item xs={minimalSpacing ? 2 : 3}>
        {label}
      </Grid>
      <Grid item xs={2} style={{ fontWeight: "bold", fontSize: "1.125rem" }} container justifyContent={minimalSpacing ? "flex-start" : "flex-end"}>
        {value}
      </Grid>
    </Grid>
  );

  return (
    <Grid container spacing={2}>
      <Grid item xs={cardSizes.xs} className={styles.numberBox}>
        <Card className={styles.card} data-printable={true}>
          <CardHeader
            style={{ paddingBottom: 0 }}
            title={<Typography variant="h5" align="center">{strings(stringKeys.dashboard.numbers.reportCountTitle)}</Typography>}
          />
          <CardContent>
            {renderNumber(
              strings(stringKeys.dashboard.numbers.totalReportCount),
              projectSummary.totalReportCount,
              true
            )}
          </CardContent>
        </Card>
      </Grid>

      <Grid item xs={cardSizes.xs} className={styles.numberBox}>
        <Card className={styles.card} data-printable={true}>
          <CardHeader
            style={{ paddingBottom: 0 }}
            title={<Typography variant="h5" align="center">{strings(stringKeys.dashboard.dataCollectors)}</Typography>} />
          <CardContent>
            {renderNumber(
              strings(stringKeys.dashboard.activeDataCollectorCount),
              projectSummary.activeDataCollectorCount,
              true
            )}
          </CardContent>
        </Card>
      </Grid>

      {reportsType === "dataCollectionPoint" && (
      <Grid item xs={cardSizes.xs} className={styles.numberBox}>
        <Card className={styles.card} data-printable={true}>
            <CardHeader
              style={{ paddingBottom: 0 }}
              title={<Typography variant="h5" align="center">{strings(stringKeys.dashboard.dataCollectionPoints)}</Typography>}
            />
            <CardContent>
              {renderNumber(
                strings(stringKeys.dashboard.referredToHospitalCount),
                projectSummary.dataCollectionPointSummary
                  .referredToHospitalCount,
              )}
            </CardContent>
          </Card>
        </Grid>
      )}

      {reportsType !== "dataCollectionPoint" && (
      <Grid item xs={cardSizes.xs} className={styles.numberBox}>
        <Card className={styles.card} data-printable={true}>
            <CardHeader style={{ paddingBottom: 0 }} title={<Typography variant="h5" align="center">{strings(stringKeys.dashboard.alertsSummary)}</Typography>} />
            <CardContent>
              {renderNumber(
                strings(stringKeys.dashboard.numbers.openAlerts),
                projectSummary.alertsSummary.open,
              )}
              {renderNumber(
                strings(stringKeys.dashboard.numbers.escalatedAlerts),
                projectSummary.alertsSummary.escalated,
              )}
              {renderNumber(
                strings(stringKeys.dashboard.numbers.closedAlerts),
                projectSummary.alertsSummary.closed,
              )}
              {renderNumber(
                strings(stringKeys.dashboard.numbers.dismissedAlerts),
                projectSummary.alertsSummary.dismissed,
              )}
            </CardContent>
          </Card>
        </Grid>
      )}

      <Grid item xs={cardSizes.xs} className={styles.numberBox}>
        <Card className={styles.card} data-printable={true}>
          <CardHeader
            style={{ paddingBottom: 0 }}
            title={<Typography variant="h5" align="center">{strings(stringKeys.dashboard.geographicalCoverageSummary)}</Typography>}
          />
          <CardContent>
            {renderNumber(
              strings(stringKeys.dashboard.numbers.numberOfVillages),
              projectSummary.numberOfVillages,
            )}
            {renderNumber(
              strings(stringKeys.dashboard.numbers.numberOfDistricts),
              projectSummary.numberOfDistricts,
            )}
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );
};
