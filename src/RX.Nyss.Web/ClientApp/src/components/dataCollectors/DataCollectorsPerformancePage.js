import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import { useMount } from '../../utils/lifecycle';
import DataCollectorsPerformanceTable from './DataCollectorsPerformanceTable';
import * as dataCollectorActions from './logic/dataCollectorsActions';
import { DataCollectorsPerformanceFilters } from './DataCollectorsPerformanceFilters';
import { DataCollectorsPerformanceTableLegend } from './DataCollectorsPerformanceTableLegend';
import { useCallback } from 'react';

const DataCollectorsPerformancePageComponent = ({filters, projectId, getDataCollectorPerformanceList, ...props}) => {
  useMount(() => {
    props.openDataCollectorsPerformanceList(projectId, filters);
  });

  const onFilterChange = useCallback((filters) =>
    getDataCollectorPerformanceList(projectId, filters), [projectId, getDataCollectorPerformanceList]);

  return (
    <Fragment>
      <DataCollectorsPerformanceFilters
        filters={filters}
        onChange={onFilterChange}
      />
      <DataCollectorsPerformanceTableLegend />
      <DataCollectorsPerformanceTable
        list={props.listData.data}
        rowsPerPage={props.listData.rowsPerPage}
        totalRows={props.listData.totalRows}
        page={props.listData.page}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        projectId={projectId}
        filters={filters}
        getDataCollectorPerformanceList={getDataCollectorPerformanceList}
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
  listData: state.dataCollectors.performanceListData,
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
