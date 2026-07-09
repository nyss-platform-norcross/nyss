import React, { Fragment, useCallback, useEffect } from "react";
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import * as alertsActions from "./logic/alertsActions";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import AlertsTable from "./components/AlertsTable";
import { useMount } from "../../utils/lifecycle";
import { AlertsFilters } from "./components/AlertsFilters";
import TableActions from "../common/tableActions/TableActions";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { stringKeys, strings } from "../../strings";
import { Loading } from "../common/loading/Loading";
import TableHeader from "../common/tableHeader/TableHeader";
import { trackEvent, trackPageView } from "../../utils/appInsightsHelper";

const AlertsListPageComponent = ({ openAlertsList, ...props }) => {
  useMount(() => {
    openAlertsList(props.projectId, props.location?.state?.alertsListState);

    // Track page view
    trackPageView("AlertsListPage");
  });

  const useRtlDirection = useSelector(
    (state) => state.appData.direction === "rtl",
  );

  //useCallback important to avoid infinite loop from useEffect in AlertsFilters
  const handleFilterChange = useCallback(
    (filters) => {
      props.getAlerts(props.projectId, 1, filters); // 1 is the default page number
    },
    [props.getAlerts, props.projectId],
  );

  const handleExportToExcel = () => {
    trackEvent("exportAlertsExcel", { exportFileType: "Excel" });

    props.export(props.projectId, props.filters);
  };

  const handlePageChange = (page) => {
    props.getAlerts(props.projectId, page, props.filters);
  };

  useEffect(() => {
    const scrollTop = props.location?.state?.alertsListState?.scrollTop;
    if (typeof scrollTop !== "number") {
      return;
    }

    const pageContent = document.getElementById("pageContent");
    if (!pageContent) {
      return;
    }

    pageContent.scrollTo(0, scrollTop);
  }, [props.location]);

  if (!props.filters) {
    return <Loading />;
  }

  return (
    <Fragment>
      <TableHeader>
        <TableActions>
          <TableActionsButton
            onClick={handleExportToExcel}
            variant={"outlined"}
            isFetching={props.isExporting}
          >
            {strings(stringKeys.alerts.list.export)}
          </TableActionsButton>
        </TableActions>
      </TableHeader>

      <AlertsFilters
        filters={props.filters}
        filtersData={props.filtersData}
        locations={props.locations}
        healthRisks={props.healthRisks}
        onChange={handleFilterChange}
        rtl={useRtlDirection}
      />

      <AlertsTable
        list={props.data.data}
        goToAssessment={props.goToAssessment}
        isListFetching={props.isListFetching}
        onChangePage={handlePageChange}
        onSort={handleFilterChange}
        projectId={props.projectId}
        page={props.data.page}
        totalRows={props.data.totalRows}
        rowsPerPage={props.data.rowsPerPage}
        rtl={useRtlDirection}
      />
    </Fragment>
  );
};

AlertsListPageComponent.propTypes = {
  getAlerts: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  data: state.alerts.listData,
  isListFetching: state.alerts.listFetching,
  isExporting: state.alerts.isExporting,
  isRemoving: state.alerts.listRemoving,
  filters: state.alerts.filters,
  locations: state.alerts.filtersData.locations,
  healthRisks: state.alerts.filtersData.healthRisks,
});

const mapDispatchToProps = {
  openAlertsList: alertsActions.openList.invoke,
  goToAssessment: alertsActions.goToAssessment,
  getAlerts: alertsActions.getAlerts.invoke,
  export: alertsActions.exportAlerts.invoke,
};

export const AlertsListPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(AlertsListPageComponent),
);
