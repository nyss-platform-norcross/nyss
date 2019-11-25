import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as projectDashboardActions from './logic/projectDashboardActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import Grid from '@material-ui/core/Grid';
import { Loading } from '../common/loading/Loading';
import { useMount } from '../../utils/lifecycle';
import { ProjectsDashboardFilters } from "./components/ProjectsDashboardFilters";
import { ProjectsDashboardNumbers } from './components/ProjectsDashboardNumbers';
import { ProjectsDashboardReportsMap } from './components/ProjectsDashboardReportsMap';

const ProjectDashboardPageComponent = ({ openDashbaord, reportsMapData, getDashboardData, healthRisks, projectId, isFetching, match, name, projectSummary, ...props }) => {
  useMount(() => {
    openDashbaord(projectId);
  });

  const handleFiltersChange = (filters) =>
    getDashboardData(projectId, filters);

  return (
    <Grid container spacing={3}>
      <Grid item xs={12}>
        <ProjectsDashboardFilters
          healthRisks={healthRisks}
          nationalSocietyId={props.nationalSocietyId}
          onChange={handleFiltersChange}
        />
      </Grid>

      {isFetching && <Loading />}

      {!isFetching && projectSummary && <Grid item xs={12}>
        <ProjectsDashboardNumbers
          projectSummary={projectSummary}
        />
      </Grid>}

      {!isFetching && <Grid item xs={12}>
        <ProjectsDashboardReportsMap
          data={reportsMapData}
        />
      </Grid>}
    </Grid>
  );
}

ProjectDashboardPageComponent.propTypes = {
  openDashbaord: PropTypes.func,
  name: PropTypes.string
};

const mapStateToProps = state => ({
  projectId: state.appData.route.params.projectId,
  nationalSocietyId: state.appData.route.params.nationalSocietyId,
  name: state.projectDashboard.name,
  healthRisks: state.projectDashboard.filtersData.healthRisks,
  projectSummary: state.projectDashboard.projectSummary,
  isFetching: state.projectDashboard.isFetching
});

const mapDispatchToProps = {
  openDashbaord: projectDashboardActions.openDashbaord.invoke,
  getDashboardData: projectDashboardActions.getDashboardData.invoke
};

export const ProjectDashboardPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectDashboardPageComponent)
);
