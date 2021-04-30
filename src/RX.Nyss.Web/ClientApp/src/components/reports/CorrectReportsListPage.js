import styles from "./ReportsListPage.module.scss";

import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as reportsActions from './logic/reportsActions';
import TableActions from '../common/tableActions/TableActions';
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import CorrectReportsTable from './CorrectReportsTable';
import { useMount } from '../../utils/lifecycle';
import { ReportFilters } from '../common/filters/ReportFilters';
import { strings, stringKeys } from "../../strings";
import { TableActionsButton } from "../common/tableActions/TableActionsButton";
import { Hidden, Icon } from "@material-ui/core";
import * as roles from '../../authentication/roles';
import { SendReportDialog } from "./SendReportDialog";
import { useState } from "react";

const CorrectReportsListPageComponent = (props) => {
  const [open, setOpen] = useState(false);

  const canSendReport = props.user && [roles.Administrator, roles.Manager, roles.TechnicalAdvisor, roles.Supervisor, roles.HeadSupervisor]
    .some(neededRole => props.user.roles.some(userRole => userRole === neededRole));

  useMount(() => {
    props.openReportsList(props.projectId);
  });

  if (!props.data || !props.filters || !props.sorting) {
    return null;
  }

  const handleRefresh = () =>
    props.getList(props.projectId, 1);

  const handleFiltersChange = (filters) =>
    props.getList(props.projectId, 1, filters, props.sorting);

  const handlePageChange = (page) =>
    props.getList(props.projectId, page, props.filters, props.sorting);

  const handleSortChange = (sorting) =>
    props.getList(props.projectId, props.page, props.filters, sorting);

  const handleMarkAsError = (reportId) => {
    props.markAsError(reportId);
  }

  const handleSendReport = (e) => {
    e.stopPropagation();
    props.openSendReport(props.projectId);
    setOpen(true);
  }

  return (
    <Fragment>
      <TableActions>
        <Hidden xsDown>
          <TableActionsButton onClick={handleRefresh} isFetching={props.isListFetching}>
            <Icon>refresh</Icon>
          </TableActionsButton>
        </Hidden>

        <TableActionsButton onClick={() => props.exportToCsv(props.projectId, props.filters, props.sorting)}>
          {strings(stringKeys.reports.list.exportToCsv)}
        </TableActionsButton>

        <TableActionsButton onClick={() => props.exportToExcel(props.projectId, props.filters, props.sorting)}>
          {strings(stringKeys.reports.list.exportToExcel)}
        </TableActionsButton>

        {canSendReport &&
          <TableActionsButton onClick={handleSendReport}>
            {strings(stringKeys.reports.list.sendReport)}
          </TableActionsButton>
        }
      </TableActions>

      {open && (
        <SendReportDialog close={() => setOpen(false)}
          projectId={props.projectId}
          openSendReport={props.openSendReport}
          sendReport={props.sendReport} />
      )}

      <div className={styles.filtersGrid}>
        <ReportFilters
          healthRisks={props.healthRisks}
          nationalSocietyId={props.nationalSocietyId}
          onChange={handleFiltersChange}
          filters={props.filters}
          showCorrectReportFilters={true}
          showTrainingFilter={true}
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
        goToEdition={props.goToEdition}
        markAsError={handleMarkAsError}
        isMarkingAsError={props.isMarkingAsError}
        user={props.user}
        projectIsClosed={props.projectIsClosed}
        goToAlert={props.goToAlert}
        acceptReport={props.acceptReport}
        dismissReport={props.dismissReport}
      />
    </Fragment>
  );
}

CorrectReportsListPageComponent.propTypes = {
  getReports: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  nationalSocietyId: state.appData.siteMap.parameters.nationalSocietyId,
  data: state.reports.correctReportsPaginatedListData,
  isListFetching: state.reports.listFetching,
  isRemoving: state.reports.listRemoving,
  filters: state.reports.correctReportsFilters,
  sorting: state.reports.correctReportsSorting,
  healthRisks: state.reports.filtersData.healthRisks,
  user: state.appData.user,
  isMarkingAsError: state.reports.markingAsError,
  projectIsClosed: state.appData.siteMap.parameters.projectIsClosed,
});

const mapDispatchToProps = {
  openReportsList: reportsActions.openCorrectReportsList.invoke,
  getList: reportsActions.getCorrectList.invoke,
  exportToExcel: reportsActions.exportToExcel.invoke,
  exportToCsv: reportsActions.exportToCsv.invoke,
  markAsError: reportsActions.markAsError.invoke,
  goToEdition: reportsActions.goToEdition,
  openSendReport: reportsActions.openSendReport.invoke,
  sendReport: reportsActions.sendReport.invoke,
  goToAlert: reportsActions.goToAlert,
  acceptReport: reportsActions.acceptReport.invoke,
  dismissReport: reportsActions.dismissReport.invoke
};

export const CorrectReportsListPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(CorrectReportsListPageComponent)
);
