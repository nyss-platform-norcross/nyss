import React, { Fragment, useState } from "react";
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import * as dataCollectorsActions from "./logic/dataCollectorsActions";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import TableActions from "../common/tableActions/TableActions";
import TableHeader from "../common/tableHeader/TableHeader";
import DataCollectorsTable from "./components/DataCollectorsTable";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { accessMap } from "../../authentication/accessMap";
import { DataCollectorsFilters } from "./components/DataCollectorsFilters";
import { ReplaceSupervisorDialog } from "./components/ReplaceSupervisorDialog";
import { trackEvent, trackPageView } from "../../utils/appInsightsHelper";

const DataCollectorsListPageComponent = ({
  getDataCollectorList,
  projectId,
  ...props
}) => {
  useMount(() => {
    props.openDataCollectorsList(projectId, props.filters);

    // Track page view
    trackPageView("DataCollectorsListPage");
  });

  const useRtlDirection = useSelector(
    (state) => state.appData.direction === "rtl",
  );

  const [replaceSupervisorDialogOpened, setReplaceSupervisorDialogOpened] =
    useState(false);
  const [selectedDataCollectors, setSelectedDataCollectors] = useState([]);

  const handleFilterChange = (filters) =>
    getDataCollectorList(projectId, filters);

  const handleReplaceSupervisor = (dataCollectors) => {
    setSelectedDataCollectors(dataCollectors);
    setReplaceSupervisorDialogOpened(true);
  };

  const handleExportToCsv = () => {
    trackEvent("exportDataCollectorsCsv", { exportFileType: "Csv" });

    props.exportToCsv(projectId, props.filters);
  };

  const handleExportToExcel = () => {
    trackEvent("exportDataCollectorsExcel", { exportFileType: "Excel" });

    props.exportToExcel(projectId, props.filters);
  };

  const onChangePage = (e, page) => {
    getDataCollectorList(projectId, { ...props.filters, pageNumber: page });
  };

  return (
    <Fragment>
      <TableHeader>
        {!props.isClosed && (
          <TableActions>
            <TableActionsButton
              onClick={handleExportToCsv}
              variant="outlined"
              roles={accessMap.dataCollectors.export}
              isFetching={props.isExportingToCsv}
            >
              {strings(stringKeys.dataCollectors.exportCsv)}
            </TableActionsButton>
            <TableActionsButton
              onClick={handleExportToExcel}
              variant="outlined"
              roles={accessMap.dataCollectors.export}
              isFetching={props.isExportingToExcel}
            >
              {strings(stringKeys.dataCollectors.exportExcel)}
            </TableActionsButton>
            <TableActionsButton
              onClick={() => props.goToCreation(projectId)}
              variant="contained"
              add
              rtl={useRtlDirection}
            >
              {strings(stringKeys.common.buttons.add)}
            </TableActionsButton>
          </TableActions>
        )}
      </TableHeader>

      <DataCollectorsFilters
        supervisors={props.supervisors}
        locations={props.locations}
        onChange={handleFilterChange}
        callingUserRoles={props.callingUserRoles}
        filters={props.filters}
        rtl={useRtlDirection}
      />

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
        rtl={useRtlDirection}
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
};

DataCollectorsListPageComponent.propTypes = {
  getDataCollectors: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  listData: PropTypes.object,
  isClosed: PropTypes.bool,
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
  isExportingToCsv: state.dataCollectors.isExportingToCsv,
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
  setDeployedState: dataCollectorsActions.setDeployedState.invoke,
};

export const DataCollectorsListPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsListPageComponent),
);
