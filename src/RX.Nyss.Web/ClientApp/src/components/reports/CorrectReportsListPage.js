import styles from "./ReportsListPage.module.scss";

import React, { Fragment, useCallback, useState } from "react";
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import * as reportsActions from "./logic/reportsActions";
import TableActions from "../common/tableActions/TableActions";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import CorrectReportsTable from "./CorrectReportsTable";
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

const Page = "correct";

const CorrectReportsListPageComponent = (props) => {
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
    trackPageView("CorrectReportsListPage");
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
    trackEvent("exportCorrectReportsCsv", { exportFileType: "Csv" });
    props.trackReportExport(Page, "Csv", props.projectId); //Uses old AppInsights tracking method (could be redundant, mabye remove)

    props.exportToCsv(props.projectId, props.filters, props.sorting);
  }

  function handleExportToExcel() {
    trackEvent("exportCorrectReportsExcel", { exportFileType: "Excel" });
    props.trackReportExport(Page, "Excel", props.projectId); //Uses old AppInsights tracking method (could be redundant, mabye remove)

    props.exportToExcel(props.projectId, props.filters, props.sorting);
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
              variant={"text"}
              onClick={handleRefresh}
              isFetching={props.isListFetching}
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
          healthRisks={props.healthRisks}
          locations={props.locations}
          nationalSocietyId={props.nationalSocietyId}
          onChange={handleFiltersChange}
          filters={props.filters}
          showCorrectReportFilters={true}
          rtl={useRtlDirection}
          isListFetching={props.isListFetching}
        />
      </div>

      <CorrectReportsTable
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
        projectId={props.projectId}
        goToEditing={props.goToEditing}
        isMarkingAsError={props.isMarkingAsError}
        user={props.user}
        projectIsClosed={props.projectIsClosed}
        goToAlert={props.goToAlert}
        acceptReport={props.acceptReport}
        dismissReport={props.dismissReport}
        rtl={useRtlDirection}
      />
    </Fragment>
  );
};

CorrectReportsListPageComponent.propTypes = {
  getReports: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  nationalSocietyId: state.appData.siteMap.parameters.nationalSocietyId,
  data: state.reports.correctReportsPaginatedListData,
  isListFetching: state.reports.listFetching,
  isExportingToExcel: state.reports.isExportingToExcel,
  isExportingToCsv: state.reports.isExportingToCsv,
  isRemoving: state.reports.listRemoving,
  filters: state.reports.correctReportsFilters,
  sorting: state.reports.correctReportsSorting,
  healthRisks: state.reports.filtersData.healthRisks,
  locations: state.reports.filtersData.locations,
  user: state.appData.user,
  isMarkingAsError: state.reports.markingAsError,
  projectIsClosed: state.appData.siteMap.parameters.projectIsClosed,
});

const mapDispatchToProps = {
  openReportsList: reportsActions.openCorrectReportsList.invoke,
  getList: reportsActions.getCorrectList.invoke,
  exportToExcel: reportsActions.exportToExcel.invoke,
  exportToCsv: reportsActions.exportToCsv.invoke,
  goToEditing: reportsActions.goToEditing,
  openSendReport: reportsActions.openSendReport.invoke,
  goToAlert: reportsActions.goToAlert,
  acceptReport: reportsActions.acceptReport.invoke,
  dismissReport: reportsActions.dismissReport.invoke,
  trackReportExport: reportsActions.trackReportExport,
  showMessage: appActions.showMessage,
};

export const CorrectReportsListPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(CorrectReportsListPageComponent),
);
