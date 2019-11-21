import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as projectDashboardActions from './logic/projectDashboardActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import Grid from '@material-ui/core/Grid';
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import Typography from '@material-ui/core/Typography';
import Divider from '@material-ui/core/Divider';
import { Loading } from '../common/loading/Loading';
import { stringKeys, strings } from '../../strings';
import dayjs from "dayjs"
import { useMount } from '../../utils/lifecycle';

const ProjectDashboardPageComponent = ({ openDashbaord, projectId, isFetching, match, name, projectSummary }) => {
  useMount(() => {
    openDashbaord(projectId);
  });

  if (isFetching || !projectSummary) {
    return <Loading />;
  }

  return (
    <Fragment>
      <Card>
        <CardContent>
          <Typography variant="h6">
            {dayjs(projectSummary.startDate).format("YYYY-MM-DD")}
          </Typography>
          <Typography variant="body1" color="textSecondary" gutterBottom>
            {strings(stringKeys.project.dashboard.startDate)}
          </Typography>
          <Typography variant="h1">
            <Divider />
          </Typography>
          <Grid container spacing={2}>
            <Grid item container xs={4} direction="column" spacing={2}>
              <Grid item>
                <Typography variant="h5">
                  {strings(stringKeys.project.dashboard.dataCollectors)}
                </Typography>
              </Grid>
              <Grid item>
                <Typography variant="body1">
                  {projectSummary.activeDataCollectorCount}
                </Typography>
                <Typography variant="body2" color="textSecondary" gutterBottom>
                  {strings(stringKeys.project.dashboard.activeDataCollectorCount)}
                </Typography>
                <Typography variant="body1">
                  {projectSummary.inactiveDataCollectorCount}
                </Typography>
                <Typography variant="body2" color="textSecondary" gutterBottom>
                  {strings(stringKeys.project.dashboard.inactiveDataCollectorCount)}
                </Typography>
                <Typography variant="body1">
                  {projectSummary.inTrainingDataCollectorCount}
                </Typography>
                <Typography variant="body2" color="textSecondary" gutterBottom>
                  {strings(stringKeys.project.dashboard.inTrainingDataCollectorCount)}
                </Typography>
              </Grid>
            </Grid>
            <Grid item container xs={4} direction="column" spacing={2}>
              <Grid item>
                <Typography variant="h5">
                  {strings(stringKeys.project.dashboard.healthRisks)}
                </Typography>
              </Grid>
              {projectSummary.healthRisks.map(healthRisk => (
                <Grid item key={`projectSummaryHealthRisk_${healthRisk.id}`}>
                  <Typography variant="h6" gutterBottom>
                    {healthRisk.name}
                  </Typography>
                  <Typography variant="body1">
                    {healthRisk.totalReportCount}
                  </Typography>
                  <Typography variant="body2" color="textSecondary" gutterBottom>
                    {strings(stringKeys.project.dashboard.totalReportCount)}
                  </Typography>
                  <Typography variant="body1">
                    {healthRisk.escalatedAlertCount}
                  </Typography>
                  <Typography variant="body2" color="textSecondary" gutterBottom>
                    {strings(stringKeys.project.dashboard.escalatedAlertCount)}
                  </Typography>
                  <Typography variant="body1">
                    {healthRisk.dismissedAlertCount}
                  </Typography>
                  <Typography variant="body2" color="textSecondary" gutterBottom>
                    {strings(stringKeys.project.dashboard.dismissedAlertCount)}
                  </Typography>
                </Grid>
              ))}
            </Grid>
            <Grid item container xs={4} direction="column" spacing={2}>
              <Grid item>
                <Typography variant="h5">
                  {strings(stringKeys.project.dashboard.supervisors)}
                </Typography>
              </Grid>
              {projectSummary.supervisors.map(supervisor => (
                <Grid item key={`projectSummarySupervisor_${supervisor.id}`}>
                  <Typography variant="h6" gutterBottom>
                    {supervisor.name}
                  </Typography>
                  <Typography variant="body1">
                    {supervisor.emailAddress}
                  </Typography>
                  <Typography variant="body2" color="textSecondary" gutterBottom>
                    {strings(stringKeys.project.dashboard.supervisorEmailAddress)}
                  </Typography>
                  <Typography variant="body1">
                    {supervisor.phoneNumber}
                  </Typography>
                  <Typography variant="body2" color="textSecondary" gutterBottom>
                    {strings(stringKeys.project.dashboard.supervisorPhoneNumber)}
                  </Typography>
                  {supervisor.additionalPhoneNumber &&
                    <Fragment>
                      <Typography variant="body1">
                        {supervisor.additionalPhoneNumber}
                      </Typography>
                      <Typography variant="body2" color="textSecondary" gutterBottom>
                        {strings(stringKeys.project.dashboard.supervisorAdditionalPhoneNumber)}
                      </Typography>
                    </Fragment>
                  }
                </Grid>
              ))}
            </Grid>
          </Grid>
        </CardContent>
      </Card>
    </Fragment>
  );
}

ProjectDashboardPageComponent.propTypes = {
  openDashbaord: PropTypes.func,
  name: PropTypes.string
};

const mapStateToProps = state => ({
  projectId: state.appData.route.params.projectId,
  name: state.projectDashboard.name,
  projectSummary: state.projectDashboard.projectSummary,
  isFetching: state.projectDashboard.isFetching
});

const mapDispatchToProps = {
  openDashbaord: projectDashboardActions.openDashbaord.invoke
};

export const ProjectDashboardPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectDashboardPageComponent)
);
