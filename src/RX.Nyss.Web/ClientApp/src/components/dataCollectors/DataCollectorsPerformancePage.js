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
import { DataCollectorsPerformanceFilters } from './DataCollectorsPerformanceFilters';

const DataCollectorsPerformancePageComponent = (props) => {
  useMount(() => {
    props.openDataCollectorsPerformanceList(props.projectId, props.filters);
  });

  const onFilterChange = (filters) => {
    props.getDataCollectorPerformanceList(props.projectId, filters);
  }

  if (props.isListFetching) {
    return <Loading />;
  }

  return (
    <Fragment>
      <DataCollectorsPerformanceFilters
        filters={props.filters}
        nationalSocietyId={props.nationalSocietyId}
        onChange={onFilterChange}
      />
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
