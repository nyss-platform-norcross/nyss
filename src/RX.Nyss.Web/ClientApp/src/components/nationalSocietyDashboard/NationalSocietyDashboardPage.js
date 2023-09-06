import styles from "./NationalSocietyDashboardPage.module.scss";
import React, { Fragment, useRef } from 'react';
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import * as nationalSocietyDashboardActions from './logic/nationalSocietyDashboardActions';
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import { Grid } from '@material-ui/core';
import { Loading } from '../common/loading/Loading';
import { useMount } from '../../utils/lifecycle';
import { NationalSocietyDashboardFilters } from "./components/NationalSocietyDashboardFilters";
import { NationalSocietyDashboardNumbers } from './components/NationalSocietyDashboardNumbers';
import { DashboardReportsMap } from '../dashboardCharts/DashboardReportsMap';
import { DashboardReportVillageChart } from '../dashboardCharts/DashboardReportVillageChart';
import { DashboardReportChart } from '../dashboardCharts/DashboardReportChart';
import { DashboardReportSexAgeChart } from '../dashboardCharts/DashboardReportSexAgeChart';
import { DashboardReportSexAgeTable } from "../dashboardTables/DashboardReportSexAgeTable";

const NationalSocietyDashboardPageComponent = ({ nationalSocietyId, openDashboard, getDashboardData, isGeneratingPdf, isFetching, userRoles, ...props }) => {
  useMount(() => {
    openDashboard(props.match.params.nationalSocietyId);
  });

  const useRtlDirection = useSelector(state => state.appData.direction === 'rtl');

  const dashboardElement = useRef(null);

  const handleFiltersChange = (filters) =>
    getDashboardData(nationalSocietyId, filters);

  if (!props.filters) {
    return <Loading />;
  }

  return (
    <Grid container spacing={2} ref={dashboardElement}>
      <Grid item xs={12} className={styles.filtersGrid}>
        <NationalSocietyDashboardFilters
          healthRisks={props.healthRisks}
          organizations={props.organizations}
          locations={props.locations}
          onChange={handleFiltersChange}
          filters={props.filters}
          isFetching={isFetching}
          userRoles={userRoles}
          rtl={useRtlDirection}
        />
      </Grid>

      {!props.summary
        ? <Loading />
        : (
          <Fragment>
            <Grid item xs={12}>
              <NationalSocietyDashboardNumbers
                summary={props.summary}
                reportsType={props.filters.reportsType} />
            </Grid>
            <Grid item xs={12}>
              <DashboardReportsMap
                data={props.reportsGroupedByLocation}
                detailsFetching={props.reportsGroupedByLocationDetailsFetching}
                details={props.reportsGroupedByLocationDetails}
                getReportHealthRisks={(lat, long) => props.getReportHealthRisks(nationalSocietyId, lat, long)}
              />
            </Grid>

            <Grid item xs={12}>
              <DashboardReportChart data={props.reportsGroupedByHealthRiskAndDate}/>
            </Grid>

            <Grid item xs={12}>
              <DashboardReportVillageChart data={props.reportsGroupedByVillageAndDate} />
            </Grid>

            <Grid item xs={12}>
              <DashboardReportSexAgeChart data={props.reportsGroupedByFeaturesAndDate} />
            </Grid>

            <Grid item sm={6} xs={12}>
              <DashboardReportSexAgeTable data={props.reportsGroupedByFeatures} />
            </Grid>
          </Fragment>
        )}
    </Grid>
  );
}

NationalSocietyDashboardPageComponent.propTypes = {
  openDashboard: PropTypes.func
};

// Map redux state to component props
const mapStateToProps = state => ({
  nationalSocietyId: state.appData.route.params.nationalSocietyId,
  healthRisks: state.nationalSocietyDashboard.filtersData.healthRisks,
  organizations: state.nationalSocietyDashboard.filtersData.organizations,
  locations: state.nationalSocietyDashboard.filtersData.locations,
  summary: state.nationalSocietyDashboard.summary,
  filters: state.nationalSocietyDashboard.filters,
  reportsGroupedByLocation: state.nationalSocietyDashboard.reportsGroupedByLocation,
  reportsGroupedByLocationDetails: state.nationalSocietyDashboard.reportsGroupedByLocationDetails,
  reportsGroupedByVillageAndDate: state.nationalSocietyDashboard.reportsGroupedByVillageAndDate,
  reportsGroupedByLocationDetailsFetching: state.nationalSocietyDashboard.reportsGroupedByLocationDetailsFetching,
  isGeneratingPdf: state.nationalSocietyDashboard.isGeneratingPdf,
  isFetching: state.nationalSocietyDashboard.isFetching,
  userRoles: state.appData.user.roles,
  reportsGroupedByHealthRiskAndDate: state.nationalSocietyDashboard.reportsGroupedByHealthRiskAndDate,
  reportsGroupedByFeaturesAndDate: state.nationalSocietyDashboard.reportsGroupedByFeaturesAndDate,
  reportsGroupedByFeatures: state.nationalSocietyDashboard.reportsGroupedByFeatures,
});

const mapDispatchToProps = {
  openDashboard: nationalSocietyDashboardActions.openDashboard.invoke,
  getReportHealthRisks: nationalSocietyDashboardActions.getReportHealthRisks.invoke,
  getDashboardData: nationalSocietyDashboardActions.getDashboardData.invoke
};

export const NationalSocietyDashboardPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyDashboardPageComponent)
);
