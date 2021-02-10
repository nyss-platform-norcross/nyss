import React, { Fragment, useReducer, useEffect } from 'react';
import PropTypes from "prop-types";
import { connect, shallowEqual } from "react-redux";
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import { useMount } from '../../utils/lifecycle';
import DataCollectorsPerformanceTable from './DataCollectorsPerformanceTable';
import * as dataCollectorActions from './logic/dataCollectorsActions';
import { DataCollectorsPerformanceFilters } from './DataCollectorsPerformanceFilters';
import { DataCollectorsPerformanceTableLegend } from './DataCollectorsPerformanceTableLegend';
import { initialState } from '../../initialState';

const initFilter = (filters) => {
  return {
    value: filters,
    changed: false
  }
}

const resetFilter = (filters) => {
  return {
    value: filters,
    changed: true
  }
}

const onSort = (state, week, filters) => {
  return {
    value: {
      ...state,
      [week]: {
        reportingCorrectly: filters.reportingCorrectly,
        reportingWithErrors: filters.reportingWithErrors,
        notReporting: filters.notReporting
      }
    },
    changed: !shallowEqual(state[week], filters)
  }
}

const DataCollectorsPerformancePageComponent = ({projectId, getDataCollectorPerformanceList, ...props}) => {
  useMount(() => {
    props.openDataCollectorsPerformanceList(projectId, props.filters);
  });

  const filterReducer = (state, action) => {
    switch (action.type) {
      case 'updateArea': return { value: { ...state.value, area: action.area, pageNumber: action.pageNumber }, changed: !shallowEqual(state.value.area, action.area) };
      case 'updateName': return { value: { ...state.value, name: action.name }, changed: state.value.name !== action.name };
      case 'updateSupervisor': return { value: { ...state.value, supervisorId: action.supervisorId }, changed: state.value.supervisorId !== action.supervisorId };
      case 'updateSorting': return onSort(state.value, action.week, action.filters);
      case 'changePage': return { value: { ...state.value, pageNumber: action.pageNumber }, changed: state.value.pageNumber !== action.pageNumber };
      case 'reset': return resetFilter(initialState.dataCollectors.performanceListFilters);
      default: return state;
    }
  }
  
  const [filters, setFilters] = useReducer(filterReducer, initialState.dataCollectors.performanceListFilters, initFilter);

  useEffect(() => {
    filters.changed && getDataCollectorPerformanceList(projectId, filters.value);
  }, [filters, projectId, getDataCollectorPerformanceList]);

  return (
    <Fragment>
      <DataCollectorsPerformanceFilters
        onChange={setFilters}
        filters={filters.value}
      />
      <DataCollectorsPerformanceTableLegend />
      <DataCollectorsPerformanceTable
        list={props.listData.data}
        completeness={props.completeness}
        rowsPerPage={props.listData.rowsPerPage}
        totalRows={props.listData.totalRows}
        page={props.listData.page}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        projectId={projectId}
        filters={filters.value}
        onChange={setFilters}
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
  completeness: state.dataCollectors.completeness,
  isListFetching: state.dataCollectors.performanceListFetching,
});

const mapDispatchToProps = {
  openDataCollectorsPerformanceList: dataCollectorActions.openDataCollectorsPerformanceList.invoke,
  getDataCollectorPerformanceList: dataCollectorActions.getDataCollectorsPerformanceList.invoke
};

export const DataCollectorsPerformancePage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsPerformancePageComponent)
);
