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
import { NationalSocietyDashboardReportsMap } from './components/NationalSocietyDashboardReportsMap';
import { NationalSocietyDashboardReportVillageChart } from './components/NationalSocietyDashboardReportVillageChart';
import { ProjectsDashboardReportChart } from '../projectDashboard/components/ProjectsDashboardReportChart';

const NationalSocietyDashboardPageComponent = ({ openDashboard, getDashboardData, isGeneratingPdf, isFetching, userRoles, ...props }) => {
  useMount(() => {
    openDashboard(props.match.params.nationalSocietyId);
  });

  const useRtlDirection = useSelector(state => state.appData.direction === 'rtl');

  const dashboardElement = useRef(null);

  const handleFiltersChange = (filters) =>
    getDashboardData(props.nationalSocietyId, filters);

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
              <NationalSocietyDashboardReportsMap
                nationalSocietyId={props.nationalSocietyId}
                data={props.reportsGroupedByLocation}
                detailsFetching={props.reportsGroupedByLocationDetailsFetching}
                details={props.reportsGroupedByLocationDetails}
                getReportHealthRisks={props.getReportHealthRisks}
              />
            </Grid>

            <Grid item xs={12}>
              <ProjectsDashboardReportChart data={props.nationalSocietyDashboard.reportsGroupedByHealthRiskAndDate}/>
            </Grid>

            <Grid item xs={12}>
              <NationalSocietyDashboardReportVillageChart data={props.reportsGroupedByVillageAndDate} />
            </Grid>
          </Fragment>
        )}
    </Grid>
  );
}

NationalSocietyDashboardPageComponent.propTypes = {
  openDashboard: PropTypes.func
};

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
