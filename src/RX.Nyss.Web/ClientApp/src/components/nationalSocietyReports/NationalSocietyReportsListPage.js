import styles from "./NationalSocietyReportsListPage.module.scss";
import React, { useState } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as nationalSocietyReportsActions from './logic/nationalSocietyReportsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import NationalSocietyReportsTable from './NationalSocietyReportsTable';
import { NationalSocietyReportsFilters } from './NationalSocietyReportsFilters';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';

const NationalSocietyReportsListPageComponent = (props) => {
  useMount(() => {
    props.openNationalSocietyReportsList(props.nationalSocietyId);
  });

  if (!props.data || !props.filters) {
    return null;
  }

  const handleFiltersChange = (filters) =>
    props.getList(props.nationalSocietyId, props.page, filters);

  return (
    <Grid container spacing={3}>
      <Grid item xs={12} className={styles.filtersGrid}>
        <NationalSocietyReportsFilters
          healthRisks={props.healthRisks}
          nationalSocietyId={props.nationalSocietyId}
          onChange={handleFiltersChange}
          filters={props.filters}
        />
      </Grid>

      <Grid item xs={12}>
        <NationalSocietyReportsTable
          list={props.data.data}
          isListFetching={props.isListFetching}
          getList={props.getList}
          nationalSocietyId={props.nationalSocietyId}
          page={props.data.page}
          totalRows={props.data.totalRows}
          rowsPerPage={props.data.rowsPerPage}
          reportsType={props.filters.reportsType}
        />
      </Grid>
    </Grid>
  );
}

NationalSocietyReportsListPageComponent.propTypes = {
  getNationalSocietyReports: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  data: state.nationalSocietyReports.paginatedListData,
  isListFetching: state.nationalSocietyReports.listFetching,
  isRemoving: state.nationalSocietyReports.listRemoving,
  filters: state.nationalSocietyReports.filters,
  healthRisks: state.nationalSocietyReports.filtersData.healthRisks
});

const mapDispatchToProps = {
  openNationalSocietyReportsList: nationalSocietyReportsActions.openList.invoke,
  getList: nationalSocietyReportsActions.getList.invoke
};

export const NationalSocietyReportsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyReportsListPageComponent)
);
