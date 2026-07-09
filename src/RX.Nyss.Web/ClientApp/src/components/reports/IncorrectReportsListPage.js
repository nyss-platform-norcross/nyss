import styles from "./ReportsListPage.module.scss";

import React, { useState, Fragment, useCallback } from "react";
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import * as reportsActions from "./logic/reportsActions";
import TableActions from "../common/tableActions/TableActions";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import IncorrectReportsTable from "./IncorrectReportsTable";
import { useMount } from "../../utils/lifecycle";
import { ReportFilters } from "../common/filters/ReportFilters";
import { strings, stringKeys } from "../../strings";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { Hidden, Icon } from "@material-ui/core";
import * as roles from "../../authentication/roles";
import { SendReportDialog } from "./SendReportDialog";
import * as appActions from "../app/logic/appActions";
import TableHeader from "../common/tableHeader/TableHeader";
import { trackEvent, trackPageView } from "../../utils/appInsightsHelper";

const Page = "incorrect";

const IncorrectReportsListPageComponent = (props) => {
  const [open, setOpen] = useState(false);
  const useRtlDirection = useSelector(
    (state) => state.appData.direction === "rtl",
  );

  const canSendReport =
    props.user &&
    [
      roles.Administrator,
      roles.Manager,
      roles.TechnicalAdvisor,
      roles.Supervisor,
      roles.HeadSupervisor,
    ].some((neededRole) =>
      props.user.roles.some((userRole) => userRole === neededRole),
    );

  useMount(() => {
    props.openReportsList(props.projectId);

    // Track page view
    trackPageView("IncorrectReportsListPage");
  });

  const handleRefresh = () => props.getList(props.projectId, 1);

  //useCallback important to avoid infinite loop from useEffect in ReportFilters
  const handleFiltersChange = useCallback(
    (filters) => props.getList(props.projectId, 1, filters, props.sorting),
    [props.getList, props.projectId, props.sorting],
  );

  const handlePageChange = (page) =>
    props.getList(props.projectId, page, props.filters, props.sorting);

  const handleSortChange = (sorting) =>
    props.getList(props.projectId, props.page, props.filters, sorting);

  const handleSendReport = (e) => {
    e.stopPropagation();
    props.openSendReport(props.projectId);
    setOpen(true);
  };

  function handleExportToCsv() {
    trackEvent("exportIncorrectReportsCsv", { exportFileType: "Csv" });
    props.trackReportExport(Page, "Csv", props.projectId); //Uses old AppInsights tracking method (could be redundant, mabye remove)

    props.exportToCsv(props.projectId, props.filters, props.sorting);
  }

  function handleExportToExcel() {
    trackEvent("exportIncorrectReportsExcel", { exportFileType: "Excel" });
    props.trackReportExport(Page, "Excel", props.projectId); //Uses old AppInsights tracking method (could be redundant, mabye remove)

    props.exportToExcel(props.projectId, props.filters, props.sorting);
  }

  function onCorrectToggle(row) {
    if (row.isCorrected) {
      props.markAsNotCorrected(row.id);
    } else {
      props.markAsCorrected(row.id);
    }
  }

  if (!props.data || !props.filters || !props.sorting) {
    return null;
  }

  return (
    <Fragment>
      <TableHeader>
        <TableActions>
          <Hidden xsDown>
            <TableActionsButton
              onClick={handleRefresh}
              isFetching={props.isListFetching}
              variant={"text"}
            >
              <Icon>refresh</Icon>
            </TableActionsButton>
          </Hidden>

          <TableActionsButton
            onClick={handleExportToCsv}
            variant={"outlined"}
            isFetching={props.isExportingToCsv}
          >
            {strings(stringKeys.reports.list.exportToCsv)}
          </TableActionsButton>

          <TableActionsButton
            onClick={handleExportToExcel}
            variant={"outlined"}
            isFetching={props.isExportingToExcel}
          >
            {strings(stringKeys.reports.list.exportToExcel)}
          </TableActionsButton>

          {canSendReport && (
            <TableActionsButton
              onClick={handleSendReport}
              variant={"contained"}
            >
              {strings(stringKeys.reports.list.sendReport)}
            </TableActionsButton>
          )}
        </TableActions>
      </TableHeader>

      {open && (
        <SendReportDialog
          close={() => setOpen(false)}
          projectId={props.projectId}
          openSendReport={props.openSendReport}
          showMessage={props.showMessage}
        />
      )}

      <div className={styles.filtersGrid}>
        <ReportFilters
          locations={props.locations}
          onChange={handleFiltersChange}
          filters={props.filters}
          showCorrectReportFilters={false}
          rtl={useRtlDirection}
          isListFetching={props.isListFetching}
        />
      </div>

      <IncorrectReportsTable
        list={props.data.data}
        isListFetching={props.isListFetching}
        page={props.data.page}
        onChangePage={handlePageChange}
        totalRows={props.data.totalRows}
        rowsPerPage={props.data.rowsPerPage}
        reportsType={props.filters.reportsType}
        filters={props.filters}
        sorting={props.sorting}
        onSort={handleSortChange}
        user={props.user}
        onCorrectToggle={onCorrectToggle}
        rtl={useRtlDirection}
      />
    </Fragment>
  );
};

IncorrectReportsListPageComponent.propTypes = {
  getReports: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  data: state.reports.incorrectReportsPaginatedListData,
  isListFetching: state.reports.listFetching,
  isExportingToExcel: state.reports.isExportingToExcel,
  isExportingToCsv: state.reports.isExportingToCsv,
  isRemoving: state.reports.listRemoving,
  filters: state.reports.incorrectReportsFilters,
  sorting: state.reports.incorrectReportsSorting,
  user: state.appData.user,
  locations: state.reports.filtersData.locations,
});

const mapDispatchToProps = {
  openReportsList: reportsActions.openIncorrectReportsList.invoke,
  getList: reportsActions.getIncorrectList.invoke,
  exportToExcel: reportsActions.exportToExcel.invoke,
  exportToCsv: reportsActions.exportToCsv.invoke,
  openSendReport: reportsActions.openSendReport.invoke,
  trackReportExport: reportsActions.trackReportExport,
  markAsCorrected: reportsActions.markAsCorrected.invoke,
  markAsNotCorrected: reportsActions.markAsNotCorrected.invoke,
  showMessage: appActions.showMessage,
};

export const IncorrectReportsListPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(IncorrectReportsListPageComponent),
);
