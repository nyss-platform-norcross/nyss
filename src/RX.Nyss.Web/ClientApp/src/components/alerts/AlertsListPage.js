import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as alertsActions from './logic/alertsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import AlertsTable from './components/AlertsTable';
import { useMount } from '../../utils/lifecycle';
import { AlertsFilters } from './components/AlertsFilters';

const AlertsListPageComponent = (props) => {
  useMount(() => {
    props.openAlertsList(props.projectId);
  });

  const handleFilterChange = (filters) => {
    props.getList(props.projectId, props.data.page, filters);
  }

  const handlePageChange = (page) => {
    props.getList(props.projectId, page, props.filters);
  }

  if (!props.data) {
    return null;
  }

  return (
    <Fragment>
      <AlertsFilters 
        filters={props.filters}
        filtersData={props.filtersData}
        onChange={handleFilterChange}
      />

      <AlertsTable
        list={props.data.data}
        goToAssessment={props.goToAssessment}
        isListFetching={props.isListFetching}
        onChangePage={handlePageChange}
        onSort={handleFilterChange}
        projectId={props.projectId}
        page={props.data.page}
        totalRows={props.data.totalRows}
        rowsPerPage={props.data.rowsPerPage}
      />
    </Fragment>
  );
}

AlertsListPageComponent.propTypes = {
  getAlerts: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  data: state.alerts.listData,
  isListFetching: state.alerts.listFetching,
  isRemoving: state.alerts.listRemoving,
  filters: state.alerts.filters,
  filtersData: state.alerts.filtersData
});

const mapDispatchToProps = {
  openAlertsList: alertsActions.openList.invoke,
  goToAssessment: alertsActions.goToAssessment,
  getList: alertsActions.getList.invoke
};

export const AlertsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(AlertsListPageComponent)
);
