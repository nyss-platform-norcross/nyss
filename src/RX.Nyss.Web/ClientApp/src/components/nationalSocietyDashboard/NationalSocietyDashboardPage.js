import styles from "./NationalSocietyDashboardPage.module.scss";
import React, { Fragment, useRef } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
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

const NationalSocietyDashboardPageComponent = ({ openDashbaord, getDashboardData, isGeneratingPdf, isFetching, ...props }) => {
  useMount(() => {
    openDashbaord(props.nationalSocietyId);
  });

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
          nationalSocietyId={props.nationalSocietyId}
          onChange={handleFiltersChange}
          filters={props.filters}
          isFetching={isFetching}
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
              <NationalSocietyDashboardReportVillageChart data={props.reportsGroupedByVillageAndDate} />
            </Grid>
          </Fragment>
        )}
    </Grid>
  );
}

NationalSocietyDashboardPageComponent.propTypes = {
  openDashbaord: PropTypes.func
};

const mapStateToProps = state => ({
  nationalSocietyId: state.appData.route.params.nationalSocietyId,
  healthRisks: state.nationalSocietyDashboard.filtersData.healthRisks,
  organizations: state.nationalSocietyDashboard.filtersData.organizations,
  filtersData: state.nationalSocietyDashboard.filtersData,
  summary: state.nationalSocietyDashboard.summary,
  filters: state.nationalSocietyDashboard.filters,
  reportsGroupedByLocation: state.nationalSocietyDashboard.reportsGroupedByLocation,
  reportsGroupedByLocationDetails: state.nationalSocietyDashboard.reportsGroupedByLocationDetails,
  reportsGroupedByVillageAndDate: state.nationalSocietyDashboard.reportsGroupedByVillageAndDate,
  reportsGroupedByLocationDetailsFetching: state.nationalSocietyDashboard.reportsGroupedByLocationDetailsFetching,
  isGeneratingPdf: state.nationalSocietyDashboard.isGeneratingPdf,
  isFetching: state.nationalSocietyDashboard.isFetching
});

const mapDispatchToProps = {
  openDashbaord: nationalSocietyDashboardActions.openDashbaord.invoke,
  getReportHealthRisks: nationalSocietyDashboardActions.getReportHealthRisks.invoke,
  getDashboardData: nationalSocietyDashboardActions.getDashboardData.invoke
};

export const NationalSocietyDashboardPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyDashboardPageComponent)
);
