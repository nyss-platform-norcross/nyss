import { Fragment } from "react";
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import { useMount } from "../../utils/lifecycle";
import DataCollectorsPerformanceTable from "./components/DataCollectorsPerformanceTable";
import * as dataCollectorActions from "./logic/dataCollectorsActions";
import { DataCollectorsPerformanceFilters } from "./components/DataCollectorsPerformanceFilters";
import { DataCollectorsPerformanceTableLegend } from "./components/DataCollectorsPerformanceTableLegend";
import TableActions from "../common/tableActions/TableActions";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { stringKeys, strings } from "../../strings";
import { accessMap } from "../../authentication/accessMap";
import TableHeader from "../common/tableHeader/TableHeader";
import { trackEvent, trackPageView } from "../../utils/appInsightsHelper";

const DataCollectorsPerformancePageComponent = ({
  projectId,
  getDataCollectorPerformanceList,
  ...props
}) => {
  useMount(() => {
    props.openDataCollectorsPerformanceList(projectId);

    // Track page view
    trackPageView("DataCollectorsPerformancePage");
  });

  const useRtlDirection = useSelector(
    (state) => state.appData.direction === "rtl",
  );

  const handleFilterChange = (filters) =>
    getDataCollectorPerformanceList(projectId, filters);

  const handleExportToExcel = () => {
    trackEvent("exportDataCollectorsPerformanceExcel", {
      exportFileType: "Excel",
    });

    props.exportPerformance(projectId, props.filters);
  };

  return (
    <Fragment>
      <TableHeader>
        <TableActions>
          <TableActionsButton
            onClick={handleExportToExcel}
            roles={accessMap.dataCollectors.export}
            isFetching={props.isExporting}
            variant={"outlined"}
          >
            {strings(stringKeys.dataCollectors.exportExcel)}
          </TableActionsButton>
        </TableActions>
      </TableHeader>

      <DataCollectorsPerformanceFilters
        onChange={handleFilterChange}
        filters={props.filters}
        rtl={useRtlDirection}
        locations={props.locations}
        supervisors={props.supervisors}
        healthRisks={props.healthRisks}
        userRoles={props.userRoles}
      />
      <DataCollectorsPerformanceTableLegend rtl={useRtlDirection} />
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
        filters={props.filters}
        onChange={handleFilterChange}
        rtl={useRtlDirection}
      />
    </Fragment>
  );
};

DataCollectorsPerformancePageComponent.propTypes = {
  openDataCollectorsPerformanceList: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  nationalSocietyId: state.dataCollectors.filtersData.nationalSocietyId,
  filters: state.dataCollectors.performanceListFilters,
  listData: state.dataCollectors.performanceListData,
  completeness: state.dataCollectors.completeness,
  isListFetching: state.dataCollectors.performanceListFetching,
  epiDateRange: state.dataCollectors.epiDateRange,
  isExporting: state.dataCollectors.isExporting,
  locations: state.dataCollectors.filtersData.locations,
  supervisors: state.dataCollectors.filtersData.supervisors,
  healthRisks: state.dataCollectors.filtersData.healthRisks,
  userRoles: state.appData.user.roles,
});

const mapDispatchToProps = {
  openDataCollectorsPerformanceList:
    dataCollectorActions.openDataCollectorsPerformanceList.invoke,
  getDataCollectorPerformanceList:
    dataCollectorActions.getDataCollectorsPerformanceList.invoke,
  exportPerformance: dataCollectorActions.exportDataCollectorPerformance.invoke,
};

export const DataCollectorsPerformancePage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(DataCollectorsPerformancePageComponent),
);
