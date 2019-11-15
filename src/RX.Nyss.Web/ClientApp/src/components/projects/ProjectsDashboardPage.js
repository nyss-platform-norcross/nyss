import React, { useEffect, Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as projectsActions from './logic/projectsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import Grid from '@material-ui/core/Grid';
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import Typography from '@material-ui/core/Typography';
import { Loading } from '../common/loading/Loading';
import { stringKeys, strings } from '../../strings';
import dayjs from "dayjs"
import { useMount } from '../../utils/lifecycle';

const ProjectsDashboardPageComponent = ({ openDashbaord, projectId, isFetching, match, name }) => {
  useMount(() => {
    openDashbaord(projectId);
  });

  if (isFetching) {
    return <Loading />;
  }

  return (
    <Fragment>
      <Typography variant="h2">{strings(stringKeys.project.dashboard.title)} - {name}</Typography>

      <Card key={{ id: "projectdashboard1" }.id}>
        <CardContent>
          <Typography variant="h3" gutterBottom>
            {{}.name}
          </Typography>
          <Typography variant="h6" color="textSecondary" gutterBottom>
            {{}.state}
          </Typography>
          <Grid container spacing={3}>
            <Grid item container xs={6} direction="column" spacing={2}>
              <Grid item>
                <Typography variant="h6">
                  {{}.totalReportCount}
                </Typography>
                <Typography variant="body1" color="textSecondary" gutterBottom>
                  {strings(stringKeys.project.list.totalReportCount)}
                </Typography>
                <Typography variant="h6">
                  {{}.activeDataCollectorCount}
                </Typography>
                <Typography variant="body1" color="textSecondary" gutterBottom>
                  {strings(stringKeys.project.list.activeDataCollectorCount)}
                </Typography>
                <Typography variant="h6">
                  {dayjs({}.startDate).format("YYYY-MM-DD")}
                </Typography>
                <Typography variant="body1" color="textSecondary" gutterBottom>
                  {strings(stringKeys.project.list.startDate)}
                </Typography>
              </Grid>
            </Grid>
            <Grid item container xs={6} direction="column" spacing={3}>
              <Grid item>
                <Typography variant="h6">
                  {{}.escalatedAlertCount}
                </Typography>
                <Typography variant="body1" color="textSecondary" gutterBottom>
                  {strings(stringKeys.project.list.escalatedAlertCount)}
                </Typography>
                <Typography variant="h6">
                  {{}.supervisorCount}
                </Typography>
                <Typography variant="body1" color="textSecondary" gutterBottom>
                  {strings(stringKeys.project.list.supervisorCount)}
                </Typography>
                <Typography variant="h6">
                  {{}.endDate ? dayjs({}.endDate).format("YYYY-MM-DD") : strings(stringKeys.project.list.ongoing)}
                </Typography>
                <Typography variant="body1" color="textSecondary" gutterBottom>
                  {strings(stringKeys.project.list.endDate)}
                </Typography>
              </Grid>
            </Grid>
          </Grid>
        </CardContent>
      </Card>
    </Fragment>
  );
}

ProjectsDashboardPageComponent.propTypes = {
  openDashbaord: PropTypes.func,
  name: PropTypes.string
};

const mapStateToProps = state => ({
  name: state.projects.dashboard.name,
  projectId: state.appData.route.params.projectId,
  isFetching: state.projects.dashboard.isFetching
});

const mapDispatchToProps = {
  openDashbaord: projectsActions.openDashbaord.invoke
};

export const ProjectsDashboardPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsDashboardPageComponent)
);
