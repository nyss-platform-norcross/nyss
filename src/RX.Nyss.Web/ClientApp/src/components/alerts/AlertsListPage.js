import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as alertsActions from './logic/alertsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import AlertsTable from './AlertsTable';
import { useMount } from '../../utils/lifecycle';

const AlertsListPageComponent = (props) => {
  useMount(() => {
    props.openAlertsList(props.projectId);
  });

  if (!props.data) {
    return null;
  }

  return (
    <AlertsTable
      list={props.data.data}
      goToAssessment={props.goToAssessment}
      isListFetching={props.isListFetching}
      getList={props.getList}
      projectId={props.projectId}
      page={props.data.page}
      totalRows={props.data.totalRows}
      rowsPerPage={props.data.rowsPerPage}
    />
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
  isRemoving: state.alerts.listRemoving
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
