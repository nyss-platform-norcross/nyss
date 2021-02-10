import React, { useEffect, Fragment } from 'react';
import { connect } from "react-redux";
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import { Loading } from '../common/loading/Loading';
import { useMount } from '../../utils/lifecycle';
import * as alertsActions from './logic/alertsActions';
import { AlertsLogsTable } from './components/AlertsLogsTable';

const AlertsLogsPageComponent = ({ alertId, projectId, data, ...props }) => {
  useMount(() => {
    props.openLogs(projectId, alertId);
  });

  useEffect(() => {
    if (!props.data) {
      return;
    }

  }, [props.data, props.match]);

  if (props.isFetching || !data) {
    return <Loading />;
  }

  return (
    <Fragment>
      <AlertsLogsTable
        list={data}
      />
    </Fragment>
  );
}

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  alertId: ownProps.match.params.alertId,
  isFetching: state.alerts.logsFetching,
  data: state.alerts.logsData
});

const mapDispatchToProps = {
  openLogs: alertsActions.openLogs.invoke
};

export const AlertsLogsPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(AlertsLogsPageComponent)
);
