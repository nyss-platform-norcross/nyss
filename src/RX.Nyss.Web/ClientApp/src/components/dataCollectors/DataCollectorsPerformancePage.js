import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import { useMount } from '../../utils/lifecycle';
import DataCollectorsPerformanceTable from './DataCollectorsPerformanceTable';
import * as dataCollectorActions from './logic/dataCollectorsActions';
import { DataCollectorsPerformanceTableLegend } from './DataCollectorsPerformanceTableLegend';
import { Loading } from '../common/loading/Loading';

const DataCollectorsPerformancePageComponent = (props) => {
  useMount(() => {
    props.openDataCollectorsPerformanceList(props.projectId);
  });

  if (props.isListFetching) {
    return <Loading />;
  }

  return (
    <Fragment>
      <DataCollectorsPerformanceTable
        list={props.list}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        projectId={props.projectId}
      />

      <DataCollectorsPerformanceTableLegend />
    </Fragment>
  );
}

DataCollectorsPerformancePageComponent.propTypes = {
  openDataCollectorsPerformanceList: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  list: state.dataCollectors.performanceListData,
  isListFetching: state.dataCollectors.performanceListFetching,
});

const mapDispatchToProps = {
  openDataCollectorsPerformanceList: dataCollectorActions.openDataCollectorsPerformanceList.invoke
};

export const DataCollectorsPerformancePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsPerformancePageComponent)
);
