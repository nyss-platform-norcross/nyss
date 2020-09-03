import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import { useMount } from '../../utils/lifecycle';
import DataCollectorsPerformanceTable from './DataCollectorsPerformanceTable';
import * as dataCollectorActions from './logic/dataCollectorsActions';
import { DataCollectorsPerformanceFilters } from './DataCollectorsPerformanceFilters';
import { DataCollectorsPerformanceMapLegend } from './DataCollectorsPerformanceMapLegend';
import { DataCollectorsPerformanceTableLegend } from './DataCollectorsPerformanceTableLegend';

const DataCollectorsPerformancePageComponent = (props) => {
  useMount(() => {
    props.openDataCollectorsPerformanceList(props.projectId, props.filters);
  });

  const onFilterChange = (filters) => {
    props.getDataCollectorPerformanceList(props.projectId, filters);
  }

  return (
    <Fragment>
      <DataCollectorsPerformanceFilters
        onChange={onFilterChange}
      />
      <DataCollectorsPerformanceTableLegend />
      <DataCollectorsPerformanceTable
        list={props.list}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        projectId={props.projectId}
        filters={props.filters}
        getDataCollectorPerformanceList={props.getDataCollectorPerformanceList}
      />
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
  nationalSocietyId: state.dataCollectors.filtersData.nationalSocietyId,
  filters: state.dataCollectors.performanceListFilters,
  list: state.dataCollectors.performanceListData,
  isListFetching: state.dataCollectors.performanceListFetching,
});

const mapDispatchToProps = {
  openDataCollectorsPerformanceList: dataCollectorActions.openDataCollectorsPerformanceList.invoke,
  getDataCollectorPerformanceList: dataCollectorActions.getDataCollectorsPerformanceList.invoke
};

export const DataCollectorsPerformancePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsPerformancePageComponent)
);
