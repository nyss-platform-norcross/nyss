import styles from "./ReportsListPage.module.scss";

import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as reportsActions from './logic/reportsActions';
import TableActions from '../common/tableActions/TableActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import ReportsTable from './ReportsTable';
import { useMount } from '../../utils/lifecycle';
import Grid from '@material-ui/core/Grid';
import Button from '@material-ui/core/Button';
import { ReportFilters } from '../common/filters/ReportFilters';
import { strings, stringKeys } from "../../strings";

const ReportsListPageComponent = (props) => {
  useMount(() => {
    props.openReportsList(props.projectId);
  });

  if (!props.data || !props.filters || !props.sorting) {
    return null;
  }

  const handleFiltersChange = (filters) =>
    props.getList(props.projectId, props.page, filters, props.sorting);

  const handlePageChange = (page) =>
    props.getList(props.projectId, page, props.filters, props.sorting);

  const handleSortChange = (sorting) =>
    props.getList(props.projectId, props.page, props.filters, sorting);

  return (
    <Grid container spacing={3}>
      <Grid item xs={12}>
        <TableActions>
          <Button onClick={() => props.exportToExcel(props.projectId, props.filters, props.sorting)} variant="outlined" color="primary">
            {strings(stringKeys.reports.list.exportToExcel)}
          </Button>
        </TableActions>
      </Grid>

      <Grid item xs={12} className={styles.filtersGrid}>
        <ReportFilters
          healthRisks={props.healthRisks}
          nationalSocietyId={props.nationalSocietyId}
          onChange={handleFiltersChange}
          filters={props.filters}
          showUnknownSenderOption={false}
          showTrainingFilter={true}
        />
      </Grid>

      <Grid item xs={12}>
        <ReportsTable
          list={props.data.data}
          isListFetching={props.isListFetching}
          page={props.data.page}
          onChangePage={handlePageChange}
          totalRows={props.data.totalRows}
          rowsPerPage={props.data.rowsPerPage}
          reportsType={props.filters.reportsType}
          sorting={props.sorting}
          onSort={handleSortChange}
          markAsError = {props.markAsError}
          isMarkingAsError ={props.isMarkingAsError}
          user = {props.user}
        />
      </Grid>
    </Grid>
  );
}

ReportsListPageComponent.propTypes = {
  getReports: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  nationalSocietyId: state.appData.siteMap.parameters.nationalSocietyId,
  data: state.reports.paginatedListData,
  isListFetching: state.reports.listFetching,
  isRemoving: state.reports.listRemoving,
  filters: state.reports.filters,
  sorting: state.reports.sorting,
  healthRisks: state.reports.filtersData.healthRisks
  user: state.appData.user,
  isMarkingAsError: state.reports.markingAsError
});

const mapDispatchToProps = {
  openReportsList: reportsActions.openList.invoke,
  getList: reportsActions.getList.invoke,
  exportToExcel: reportsActions.exportToExcel.invoke,
  markAsError: reportsActions.markAsError.invoke
};

export const ReportsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ReportsListPageComponent)
);
