import React, { Fragment, useState } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as dataCollectorsActions from './logic/dataCollectorsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import DataCollectorsTable from './DataCollectorsTable';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { TableActionsButton } from '../common/tableActions/TableActionsButton';
import { accessMap } from '../../authentication/accessMap';
import { DataCollectorsFilters } from './DataCollectorsFilters';
import { ReplaceSupervisorDialog } from './ReplaceSupervisorDialog';

const DataCollectorsListPageComponent = (props) => {
  useMount(() => {
    props.openDataCollectorsList(props.projectId, props.filters);
  });

  const [replaceSupervisorDialogOpened, setReplaceSupervisorDialogOpened] = useState(false);
  const [selectedDataCollectors, setSelectedDataCollectors] = useState([]);

  const handleFilterChange = (filters) =>
    props.getDataCollectorList(props.projectId, filters);

  const handleReplaceSupervisor = (dataCollectors) => {
    setSelectedDataCollectors(dataCollectors);
    setReplaceSupervisorDialogOpened(true);
  }

  return (
    <Fragment>
      {!props.isClosed &&
        <TableActions>
          <TableActionsButton onClick={() => props.goToCreation(props.projectId)} icon={<AddIcon />}>
            {strings(stringKeys.dataCollector.addNew)}
          </TableActionsButton>
          <TableActionsButton onClick={() => props.exportToExcel(props.projectId, props.filters)} roles={accessMap.dataCollectors.export}>
            {strings(stringKeys.dataCollector.exportExcel)}
          </TableActionsButton>
          <TableActionsButton onClick={() => props.exportToCsv(props.projectId, props.filters)} roles={accessMap.dataCollectors.export}>
            {strings(stringKeys.dataCollector.exportCsv)}
          </TableActionsButton>
        </TableActions>
      }

      <DataCollectorsFilters
        nationalSocietyId={props.nationalSocietyId}
        supervisors={props.supervisors}
        filters={props.filters}
        onChange={handleFilterChange}
        callingUserRoles={props.callingUserRoles} />

      <DataCollectorsTable
        list={props.list}
        goToEdition={props.goToEdition}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        isRemoving={props.isRemoving}
        remove={props.remove}
        projectId={props.projectId}
        setTrainingState={props.setTrainingState}
        isUpdatingDataCollector={props.isUpdatingDataCollector}
        selectDataCollector={props.selectDataCollector}
        selectAllDataCollectors={props.selectAllDataCollectors}
        listSelectedAll={props.listSelectedAll}
        replaceSupervisor={handleReplaceSupervisor}
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
  list: PropTypes.array,
  isClosed: PropTypes.bool
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  isClosed: state.appData.siteMap.parameters.projectIsClosed,
  list: state.dataCollectors.listData,
  isListFetching: state.dataCollectors.listFetching,
  isRemoving: state.dataCollectors.listRemoving,
  isUpdatingDataCollector: state.dataCollectors.updatingDataCollector,
  listSelectedAll: state.dataCollectors.listSelectedAll,
  supervisors: state.dataCollectors.filtersData.supervisors,
  nationalSocietyId: state.dataCollectors.filtersData.nationalSocietyId,
  filters: state.dataCollectors.filters,
  callingUserRoles: state.appData.user.roles
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
  replaceSupervisor: dataCollectorsActions.replaceSupervisor.invoke
};

export const DataCollectorsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsListPageComponent)
);
