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
import { ProjectsDashboardReportChart } from './components/ProjectsDashboardReportChart';

const ProjectDashboardPageComponent = ({ openDashbaord, getDashboardData, projectId, isFetching, ...props }) => {
  useMount(() => {
    openDashbaord(projectId);
  });

  const handleFiltersChange = (filters) =>
    getDashboardData(projectId, filters);

  if (!props.filters) {
    return <Loading />;
  }

  return (
    <Grid container spacing={3}>
      <Grid item xs={12} style={{ position: "sticky", top: -12 }}>
        <ProjectsDashboardFilters
          healthRisks={props.healthRisks}
          nationalSocietyId={props.nationalSocietyId}
          onChange={handleFiltersChange}
          filters={props.filters}
        />
      </Grid>

      {!props.projectSummary
        ? <Loading />
        : (
          <Fragment>
            <Grid item xs={12}>
              <ProjectsDashboardNumbers projectSummary={props.projectSummary} />
            </Grid>
            <Grid item xs={12}>
              <ProjectsDashboardReportsMap data={props.reportsMapData} />
            </Grid>
            <Grid item xs={12}>
              <ProjectsDashboardReportChart data={props.reportsGroupedByDate} />
            </Grid>
          </Fragment>
        )}
    </Grid>
  );
}

ProjectDashboardPageComponent.propTypes = {
  openDashbaord: PropTypes.func
};

const mapStateToProps = state => ({
  projectId: state.appData.route.params.projectId,
  nationalSocietyId: state.appData.route.params.nationalSocietyId,
  healthRisks: state.projectDashboard.filtersData.healthRisks,
  projectSummary: state.projectDashboard.projectSummary,
  filters: state.projectDashboard.filters,
  reportsGroupedByDate: state.projectDashboard.reportsGroupedByDate,
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
