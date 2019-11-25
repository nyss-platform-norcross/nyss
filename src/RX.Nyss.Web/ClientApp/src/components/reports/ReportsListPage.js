import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as reportsActions from './logic/reportsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import ReportsTable from './ReportsTable';
import { useMount } from '../../utils/lifecycle';

const ReportsListPageComponent = (props) => {
  useMount(() => {
    props.openReportsList(props.projectId);
  });

  if (!props.data) {
    return null;
  }

  return (
    <ReportsTable
      list={props.data.data}
      isListFetching={props.isListFetching}
      getList={props.getList}
      projectId={props.projectId}
      page={props.data.page}
      totalRows={props.data.totalRows}
      rowsPerPage={props.data.rowsPerPage}
    />
  );
}

ReportsListPageComponent.propTypes = {
  getReports: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  data: state.reports.paginatedListData,
  isListFetching: state.reports.listFetching,
  isRemoving: state.reports.listRemoving
});

const mapDispatchToProps = {
  openReportsList: reportsActions.openList.invoke,
  getList: reportsActions.getList.invoke
};

export const ReportsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ReportsListPageComponent)
);
