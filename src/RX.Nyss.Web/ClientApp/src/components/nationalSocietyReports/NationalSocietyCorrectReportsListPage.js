import styles from "./NationalSocietyReportsListPage.module.scss";

import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import * as nationalSocietyReportsActions from './logic/nationalSocietyReportsActions';
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import NationalSocietyCorrectReportsTable from './NationalSocietyCorrectReportsTable';
import { ReportFilters } from '../common/filters/ReportFilters';
import { useMount } from '../../utils/lifecycle';

const NationalSocietyCorrectReportsListPageComponent = (props) => {
  useMount(() => {
    props.openNationalSocietyReportsList(props.nationalSocietyId);
  });

  const useRtlDirection = useSelector(state => state.appData.direction === 'rtl');

  if (!props.data || !props.filters || !props.sorting) {
    return null;
  }

  const handleFiltersChange = (filters) =>
    props.getList(props.nationalSocietyId, props.page, filters, props.sorting);

  const handlePageChange = (page) =>
    props.getList(props.nationalSocietyId, page, props.filters, props.sorting);

  const handleSortChange = (sorting) =>
    props.getList(props.nationalSocietyId, props.page, props.filters, sorting);

  return (
    <Fragment>
      <div className={styles.filtersGrid}>
        <ReportFilters
          healthRisks={props.healthRisks}
          locations={props.locations}
          nationalSocietyId={props.nationalSocietyId}
          onChange={handleFiltersChange}
          filters={props.filters}
          showCorrectReportFilters={true}
          hideTrainingStatusFilter
          rtl={useRtlDirection}
        />
      </div>

        <NationalSocietyCorrectReportsTable
          list={props.data.data}
          isListFetching={props.isListFetching}
          page={props.data.page}
          onChangePage={handlePageChange}
          totalRows={props.data.totalRows}
          rowsPerPage={props.data.rowsPerPage}
          reportsType={props.filters.reportsType}
          filters={props.filters}
          sorting={props.sorting}
          onSort={handleSortChange}
          rtl={useRtlDirection}
        />
    </Fragment>
  );
}

NationalSocietyCorrectReportsListPageComponent.propTypes = {
  getNationalSocietyReports: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  data: state.nationalSocietyReports.correctReportsPaginatedListData,
  isListFetching: state.nationalSocietyReports.listFetching,
  isRemoving: state.nationalSocietyReports.listRemoving,
  filters: state.nationalSocietyReports.correctReportsFilters,
  sorting: state.nationalSocietyReports.correctReportsSorting,
  healthRisks: state.nationalSocietyReports.filtersData.healthRisks,
  locations: state.nationalSocietyReports.filtersData.locations,
  nationalSocietyIsArchived: state.appData.siteMap.parameters.nationalSocietyIsArchived
});

const mapDispatchToProps = {
  openNationalSocietyReportsList: nationalSocietyReportsActions.openCorrectList.invoke,
  getList: nationalSocietyReportsActions.getCorrectList.invoke
};

export const NationalSocietyCorrectReportsListPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyCorrectReportsListPageComponent)
);
