import styles from "./ProjectsDashboardPage.module.scss";
import React, { Fragment, useRef } from 'react';
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
import { ProjectsDashboardReportSexAgeChart } from './components/ProjectsDashboardReportSexAgeChart';
import { ProjectsDashboardReportSexAgeTable } from './components/ProjectsDashboardReportSexAgeTable';
import { ProjectsDashboardDataCollectionPointChart } from "./components/ProjectsDashboardDataCollectionPointChart";
import { strings, stringKeys } from "../../strings";
import SubmitButton from "../forms/submitButton/SubmitButton";

const ProjectDashboardPageComponent = ({ openDashbaord, getDashboardData, generatePdf, isGeneratingPdf, projectId, isFetching, ...props }) => {
  useMount(() => {
    openDashbaord(projectId);
  });

  const dashboardElement = useRef(null);

  const handleFiltersChange = (filters) =>
    getDashboardData(projectId, filters);

  if (!props.filters) {
    return <Loading />;
  }

  const handleGeneratePdf = async () => {
    generatePdf(dashboardElement.current);
  }

  return (
    <Grid container spacing={3} ref={dashboardElement}>
      <Grid item xs={12} className={styles.filtersGrid}>
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
              <ProjectsDashboardNumbers projectSummary={props.projectSummary} reportsType={props.filters.reportsType} />
            </Grid>
            <Grid item xs={12}>
              <ProjectsDashboardReportsMap
                projectId={projectId}
                data={props.reportsGroupedByLocation}
                detailsFetching={props.reportsGroupedByLocationDetailsFetching}
                details={props.reportsGroupedByLocationDetails}
                getReportHealthRisks={props.getReportHealthRisks}
              />
            </Grid>
            <Grid item xs={12}>
              <ProjectsDashboardReportChart data={props.reportsGroupedByDate} />
            </Grid>
            <Grid item xs={12}>
              <ProjectsDashboardReportSexAgeChart data={props.reportsGroupedByFeaturesAndDate} />
            </Grid>
            <Grid item sm={6} xs={12}>
              <ProjectsDashboardReportSexAgeTable data={props.reportsGroupedByFeatures} />
            </Grid>

            {props.filters.reportsType === "dataCollectionPoint" &&
              <Grid item xs={12}>
                <ProjectsDashboardDataCollectionPointChart data={props.dataCollectionPointsReportData} />
              </Grid>
            }
          </Fragment>
        )}

      <Grid item xs={12}>
        <SubmitButton
          isFetching={isGeneratingPdf}
          onClick={handleGeneratePdf}
        >
          {strings(stringKeys.project.dashboard.generatePdf)}
        </SubmitButton>
      </Grid>
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
  reportsGroupedByFeaturesAndDate: state.projectDashboard.reportsGroupedByFeaturesAndDate,
  reportsGroupedByFeatures: state.projectDashboard.reportsGroupedByFeatures,
  reportsGroupedByLocation: state.projectDashboard.reportsGroupedByLocation,
  reportsGroupedByLocationDetails: state.projectDashboard.reportsGroupedByLocationDetails,
  reportsGroupedByLocationDetailsFetching: state.projectDashboard.reportsGroupedByLocationDetailsFetching,
  dataCollectionPointsReportData: state.projectDashboard.dataCollectionPointsReportData,
  isGeneratingPdf: state.projectDashboard.isGeneratingPdf,
  isFetching: state.projectDashboard.isFetching
});

const mapDispatchToProps = {
  openDashbaord: projectDashboardActions.openDashbaord.invoke,
  getReportHealthRisks: projectDashboardActions.getReportHealthRisks.invoke,
  getDashboardData: projectDashboardActions.getDashboardData.invoke,
  generatePdf: projectDashboardActions.generatePdf.invoke
};

export const ProjectDashboardPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectDashboardPageComponent)
);
