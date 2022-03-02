import React, { Fragment, useCallback, useState } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as dataCollectorsActions from './logic/dataCollectorsActions';
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import DataCollectorsTable from './DataCollectorsTable';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { TableActionsButton } from '../common/buttons/tableActionsButton/TableActionsButton';
import { accessMap } from '../../authentication/accessMap';
import { DataCollectorsFilters } from './DataCollectorsFilters';
import { ReplaceSupervisorDialog } from './ReplaceSupervisorDialog';

const DataCollectorsListPageComponent = ({getDataCollectorList, projectId, ...props}) => {
  useMount(() => {
    props.openDataCollectorsList(projectId, props.filters);
  });

  const [replaceSupervisorDialogOpened, setReplaceSupervisorDialogOpened] = useState(false);
  const [selectedDataCollectors, setSelectedDataCollectors] = useState([]);

  const handleFilterChange = useCallback((filters) =>
    getDataCollectorList(projectId, filters), [getDataCollectorList, projectId]);

  const handleReplaceSupervisor = (dataCollectors) => {
    setSelectedDataCollectors(dataCollectors);
    setReplaceSupervisorDialogOpened(true);
  }

  const onChangePage = (e, page) => {
    getDataCollectorList(projectId, { ...props.filters, pageNumber: page });
  }

  return (
    <Fragment>
      {!props.isClosed &&
        <TableActions>
          <TableActionsButton
            onClick={() => props.exportToCsv(projectId, props.filters)}
            variant="outlined"
            roles={accessMap.dataCollectors.export}
            isFetching={props.isExportingToCsv}
          >
            {strings(stringKeys.dataCollector.exportCsv)}
          </TableActionsButton>
          <TableActionsButton  onClick={() => props.exportToExcel(projectId, props.filters)}
             variant="outlined"
             roles={accessMap.dataCollectors.export} isFetching={props.isExportingToExcel}
          >
            {strings(stringKeys.dataCollector.exportExcel)}
          </TableActionsButton>
          <TableActionsButton
            onClick={() => props.goToCreation(projectId)}
            variant="contained"
            icon={<AddIcon />}
          >
            {strings(stringKeys.common.buttons.add)}
          </TableActionsButton>
        </TableActions>
      }

      <DataCollectorsFilters
        supervisors={props.supervisors}
        locations={props.locations}
        onChange={handleFilterChange}
        callingUserRoles={props.callingUserRoles}
        filters={props.filters} />

      <DataCollectorsTable
        list={props.listData.data}
        page={props.listData.page}
        rowsPerPage={props.listData.rowsPerPage}
        totalRows={props.listData.totalRows}
        goToEdition={props.goToEdition}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        isRemoving={props.isRemoving}
        remove={props.remove}
        projectId={projectId}
        setTrainingState={props.setTrainingState}
        isUpdatingDataCollector={props.isUpdatingDataCollector}
        selectDataCollector={props.selectDataCollector}
        selectAllDataCollectors={props.selectAllDataCollectors}
        listSelectedAll={props.listSelectedAll}
        replaceSupervisor={handleReplaceSupervisor}
        onChangePage={onChangePage}
        setDeployedState={props.setDeployedState}
      />

      <ReplaceSupervisorDialog
        isOpened={replaceSupervisorDialogOpened}
        replaceSupervisor={props.replaceSupervisor}
        supervisors={props.supervisors}
        dataCollectors={selectedDataCollectors}
        close={() => setReplaceSupervisorDialogOpened(false)}
      />
    </Fragment>
  );
}

DataCollectorsListPageComponent.propTypes = {
  getDataCollectors: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  listData: PropTypes.object,
  isClosed: PropTypes.bool
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  isClosed: state.appData.siteMap.parameters.projectIsClosed,
  listData: state.dataCollectors.listData,
  isListFetching: state.dataCollectors.listFetching,
  isRemoving: state.dataCollectors.listRemoving,
  isUpdatingDataCollector: state.dataCollectors.updatingDataCollector,
  listSelectedAll: state.dataCollectors.listSelectedAll,
  supervisors: state.dataCollectors.filtersData.supervisors,
  locations: state.dataCollectors.filtersData.locations,
  filters: state.dataCollectors.filters,
  callingUserRoles: state.appData.user.roles,
  isExportingToExcel: state.dataCollectors.isExportingToExcel,
  isExportingToCsv: state.dataCollectors.isExportingToCsv
});

const mapDispatchToProps = {
  openDataCollectorsList: dataCollectorsActions.openList.invoke,
  getDataCollectorList: dataCollectorsActions.getList.invoke,
  goToCreation: dataCollectorsActions.goToCreation,
  goToEdition: dataCollectorsActions.goToEdition,
  selectDataCollector: dataCollectorsActions.selectDataCollector,
  selectAllDataCollectors: dataCollectorsActions.selectAllDataCollectors,
  remove: dataCollectorsActions.remove.invoke,
  setTrainingState: dataCollectorsActions.setTrainingState.invoke,
  exportToExcel: dataCollectorsActions.exportToExcel.invoke,
  exportToCsv: dataCollectorsActions.exportToCsv.invoke,
  replaceSupervisor: dataCollectorsActions.replaceSupervisor.invoke,
  setDeployedState: dataCollectorsActions.setDeployedState.invoke
};

export const DataCollectorsListPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsListPageComponent)
);
