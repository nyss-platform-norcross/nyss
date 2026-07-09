import styles from "./ProjectsDashboardPage.module.scss";
import React, { Fragment, useRef, useState } from "react";
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import * as projectDashboardActions from "./logic/projectDashboardActions";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import { Grid, useMediaQuery } from "@material-ui/core";
import { Loading } from "../common/loading/Loading";
import { useMount } from "../../utils/lifecycle";
import { ProjectsDashboardFilters } from "./components/ProjectsDashboardFilters";
import { ProjectsDashboardNumbers } from "./components/ProjectsDashboardNumbers";
import { DashboardReportsMap } from "../dashboardCharts/DashboardReportsMap";
import { DashboardReportChart } from "../dashboardCharts/DashboardReportChart";
import { DashboardReportSexAgeChart } from "../dashboardCharts/DashboardReportSexAgeChart";
import { DashboardReportSexAgeTable } from "../dashboardTables/DashboardReportSexAgeTable";
import { DashboardDataCollectionPointChart } from "../dashboardCharts/DashboardDataCollectionPointChart";
import { strings, stringKeys } from "../../strings";
import { DashboardReportVillageChart } from "../dashboardCharts/DashboardReportVillageChart";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import { trackEvent, trackPageView } from "../../utils/appInsightsHelper";
import { MapAndDashboardNumbers } from "../dashboard/MapAndDashboardNumbers";
import { DashboardReportSexAgePyramidChart } from "../dashboardCharts/DashboardReportsSexAgePyramidChart";
import { DashboardKeptReportByHealthRiskChart } from "../dashboardCharts/DashboardKeptReportByHealthRiskChart";

const ProjectDashboardPageComponent = ({
  openDashboard,
  getDashboardData,
  generatePdf,
  isGeneratingPdf,
  projectId,
  isFetching,
  userRoles,
  ...props
}) => {
  useMount(() => {
    openDashboard(props.match.params.projectId);

    // Track page view
    trackPageView("ProjectDashboardPage");
  });

  const useRtlDirection = useSelector(
    (state) => state.appData.direction === "rtl",
  );

  const dashboardElement = useRef(null);
  const [isFilterExpanded, setIsFilterExpanded] = useState(false);
  const [isMapExpanded, setIsMapExpanded] = useState(false);
  const isSmallScreen = useMediaQuery((theme) => theme.breakpoints.down("sm"));

  const handleFiltersChange = (filters) => getDashboardData(projectId, filters);

  if (!props.filters) {
    return <Loading />;
  }

  const handleGeneratePdf = () => {
    trackEvent("exportProjectDashboardPdf", { exportFileType: "Pdf" });

    const initialState = isFilterExpanded;
    setIsFilterExpanded(true);
    const timer = setTimeout(() => {
      generatePdf(dashboardElement.current);
      setIsFilterExpanded(initialState);
    }, 200);
    return () => clearTimeout(timer);
  };

  return (
    <Grid container spacing={2} ref={dashboardElement}>
      <Grid
        item
        xs={12}
        className={!isSmallScreen ? styles.filtersGrid : undefined}
      >
        <ProjectsDashboardFilters
          healthRisks={props.healthRisks}
          organizations={props.organizations}
          locations={props.locations}
          onChange={handleFiltersChange}
          filters={props.filters}
          isFetching={isFetching}
          isGeneratingPdf={isGeneratingPdf}
          isFilterExpanded={isFilterExpanded}
          setIsFilterExpanded={setIsFilterExpanded}
          userRoles={userRoles}
          rtl={useRtlDirection}
        />
      </Grid>

      {!props.projectSummary ? (
        <Loading />
      ) : (
        <Fragment>
          <MapAndDashboardNumbers
            DashboardNumbers={
              <ProjectsDashboardNumbers
                projectSummary={props.projectSummary}
                reportsType={props.filters.reportsType}
                isMapExpanded={isMapExpanded}
              />
            }
            DashboardReportsMap={
              <DashboardReportsMap
                data={props.reportsGroupedByLocation}
              />
            }
            isMapExpanded={isMapExpanded}
            setIsMapExpanded={setIsMapExpanded}
          />
          <Grid item xs={12}>
            <DashboardReportChart
              data={props.reportsGroupedByHealthRiskAndDate}
              groupingType={props.filters.groupingType}
            />
          </Grid>
          <Grid item xs={12}>
            <DashboardReportVillageChart
              data={props.reportsGroupedByVillageAndDate}
              groupingType={props.filters.groupingType}
            />
          </Grid>
          <Grid item xs={12}>
            <DashboardReportSexAgeChart
              data={props.reportsGroupedByFeaturesAndDate}
              groupingType={props.filters.groupingType}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <DashboardReportSexAgeTable data={props.reportsGroupedByFeatures} />
          </Grid>
          <Grid item xs={12}>
            <DashboardReportSexAgePyramidChart
              data={props.reportsGroupedByFeaturesAndDate}
            />
          </Grid>
          <Grid item xs={12}>
            <DashboardKeptReportByHealthRiskChart
              data={props.keptReportsInEscalatedAlertsHistogramData}
            />
          </Grid>

          {props.filters.reportsType === "dataCollectionPoint" && (
            <Grid item xs={12}>
              <DashboardDataCollectionPointChart
                data={props.dataCollectionPointsReportData}
              />
            </Grid>
          )}
        </Fragment>
      )}

      <Grid item xs={12}>
        <SubmitButton isFetching={isGeneratingPdf} onClick={handleGeneratePdf}>
          {strings(stringKeys.dashboard.generatePdf)}
        </SubmitButton>
      </Grid>
    </Grid>
  );
};

ProjectDashboardPageComponent.propTypes = {
  openDashboard: PropTypes.func,
};

const mapStateToProps = (state) => ({
  projectId: state.appData.route.params.projectId,
  nationalSocietyId: state.appData.route.params.nationalSocietyId,
  healthRisks: state.projectDashboard.filtersData.healthRisks,
  organizations: state.projectDashboard.filtersData.organizations,
  locations: state.projectDashboard.filtersData.locations,
  projectSummary: state.projectDashboard.projectSummary,
  filters: state.projectDashboard.filters,
  reportsGroupedByHealthRiskAndDate:
    state.projectDashboard.reportsGroupedByHealthRiskAndDate,
  reportsGroupedByFeaturesAndDate:
    state.projectDashboard.reportsGroupedByFeaturesAndDate,
  reportsGroupedByVillageAndDate:
    state.projectDashboard.reportsGroupedByVillageAndDate,
  reportsGroupedByFeatures: state.projectDashboard.reportsGroupedByFeatures,
  reportsGroupedByLocation: state.projectDashboard.reportsGroupedByLocation,
  reportsGroupedByLocationDetails:
    state.projectDashboard.reportsGroupedByLocationDetails,
  reportsGroupedByLocationDetailsFetching:
    state.projectDashboard.reportsGroupedByLocationDetailsFetching,
  dataCollectionPointsReportData:
    state.projectDashboard.dataCollectionPointsReportData,
  keptReportsInEscalatedAlertsHistogramData:
    state.projectDashboard.keptReportsInEscalatedAlertsHistogramData,
  isGeneratingPdf: state.projectDashboard.isGeneratingPdf,
  isFetching: state.projectDashboard.isFetching,
  userRoles: state.appData.user.roles,
});

const mapDispatchToProps = {
  openDashboard: projectDashboardActions.openDashboard.invoke,
  getReportHealthRisks: projectDashboardActions.getReportHealthRisks.invoke,
  getDashboardData: projectDashboardActions.getDashboardData.invoke,
  generatePdf: projectDashboardActions.generatePdf.invoke,
};

export const ProjectDashboardPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectDashboardPageComponent),
);
