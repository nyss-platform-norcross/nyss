import { Fragment, useReducer, useEffect } from 'react';
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
import TableActions from '../common/tableActions/TableActions';
import { TableActionsButton } from '../common/tableActions/TableActionsButton';
import { stringKeys, strings } from '../../strings';
import { accessMap } from '../../authentication/accessMap';

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

const DataCollectorsPerformancePageComponent = ({ projectId, getDataCollectorPerformanceList, ...props }) => {
  useMount(() => {
    props.openDataCollectorsPerformanceList(projectId, props.filters);
  });

  const filterReducer = (state, action) => {
    switch (action.type) {
      case 'updateLocations': return { value: { ...state.value, locations: action.locations, pageNumber: action.pageNumber }, changed: !shallowEqual(state.value.locations, action.locations) };
      case 'updateName': return { value: { ...state.value, name: action.name }, changed: state.value.name !== action.name };
      case 'updateSupervisor': return { value: { ...state.value, supervisorId: action.supervisorId }, changed: state.value.supervisorId !== action.supervisorId };
      case 'updateTrainingStatus': return { value: { ...state.value, trainingStatus: action.trainingStatus }, changed: state.value.trainingStatus !== action.trainingStatus };
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
      <TableActions>
        <TableActionsButton onClick={() => props.exportPerformance(projectId, props.filters)} roles={accessMap.dataCollectors.export} isFetching={props.isExporting}>
          {strings(stringKeys.dataCollector.exportExcel)}
        </TableActionsButton>
      </TableActions>

      <DataCollectorsPerformanceFilters
        onChange={setFilters}
        filters={filters.value}
      />
      <DataCollectorsPerformanceTableLegend />
      <DataCollectorsPerformanceTable
        list={props.listData.data}
        completeness={props.completeness}
        epiDateRange={props.epiDateRange}
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
  epiDateRange: state.dataCollectors.epiDateRange,
  isExporting: state.dataCollectors.isExporting
});

const mapDispatchToProps = {
  openDataCollectorsPerformanceList: dataCollectorActions.openDataCollectorsPerformanceList.invoke,
  getDataCollectorPerformanceList: dataCollectorActions.getDataCollectorsPerformanceList.invoke,
  exportPerformance: dataCollectorActions.exportDataCollectorPerformance.invoke
};

export const DataCollectorsPerformancePage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsPerformancePageComponent)
);
