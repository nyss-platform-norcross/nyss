import styles from "./NationalSocietyReportsListPage.module.scss";

import { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as nationalSocietyReportsActions from './logic/nationalSocietyReportsActions';
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import { ReportFilters } from '../common/filters/ReportFilters';
import { useMount } from '../../utils/lifecycle';
import NationalSocietyIncorrectReportsTable from "./NationalSocietyIncorrectReportsTable";

const NationalSocietyIncorrectReportsListPageComponent = (props) => {
  useMount(() => {
    props.openNationalSocietyReportsList(props.nationalSocietyId);
  });

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
          locations={props.locations}
          onChange={handleFiltersChange}
          filters={props.filters}
          hideTrainingStatusFilter
          hideCorrectedFilter
        />
      </div>

        <NationalSocietyIncorrectReportsTable
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
        />
    </Fragment>
  );
}

NationalSocietyIncorrectReportsListPageComponent.propTypes = {
  getNationalSocietyReports: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  data: state.nationalSocietyReports.incorrectReportsPaginatedListData,
  isListFetching: state.nationalSocietyReports.listFetching,
  isRemoving: state.nationalSocietyReports.listRemoving,
  filters: state.nationalSocietyReports.incorrectReportsFilters,
  sorting: state.nationalSocietyReports.incorrectReportsSorting,
  nationalSocietyIsArchived: state.appData.siteMap.parameters.nationalSocietyIsArchived,
  locations: state.nationalSocietyReports.filtersData.locations
});

const mapDispatchToProps = {
  openNationalSocietyReportsList: nationalSocietyReportsActions.openIncorrectList.invoke,
  getList: nationalSocietyReportsActions.getIncorrectList.invoke
};

export const NationalSocietyIncorrectReportsListPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyIncorrectReportsListPageComponent)
);
