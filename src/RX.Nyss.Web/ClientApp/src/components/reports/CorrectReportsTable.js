import styles from "./ReportsTable.module.scss";
import { Fragment, useEffect, useRef, useState } from "react";
import PropTypes from "prop-types";
import { Loading } from "../common/loading/Loading";
import { strings, stringKeys } from "../../strings";
import dayjs from "dayjs";
import TablePager from "../common/tablePagination/TablePager";
import { TableRowActions } from "../common/tableRowAction/TableRowActions";
import { TableRowAction } from "../common/tableRowAction/TableRowAction";
import EditIcon from "@material-ui/icons/Edit";
import { accessMap } from "../../authentication/accessMap";
import { TableRowMenu } from "../common/tableRowAction/TableRowMenu";
import MoreVertIcon from "@material-ui/icons/MoreVert";
import { DataCollectorType } from "../common/filters/logic/reportFilterConstsants";
import {
  DateColumnName,
  reportStatus,
  ReportType,
} from "./logic/reportsConstants";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableSortLabel,
  TableContainer
} from "@material-ui/core";
import { alertStatus } from "../alerts/logic/alertsConstants";
import {
  renderDataCollectorDisplayName,
  renderReportValue,
} from "./logic/reportsService";
import { ReportStatusChip } from "../common/chip/ReportStatusChip";
import { trackEvent } from "../../utils/appInsightsHelper";

export const CorrectReportsTable = ({
  isListFetching,
  isMarkingAsError,
  goToEditing,
  projectId,
  list,
  page,
  onChangePage,
  rowsPerPage,
  totalRows,
  filters,
  sorting,
  onSort,
  projectIsClosed,
  goToAlert,
  acceptReport,
  dismissReport,
  rtl,
}) => {
  const [value, setValue] = useState(sorting);
  const tableContainerRef = useRef(null);
  const isDataCollectionPointTable = filters.dataCollectorType === DataCollectorType.collectionPoint;

  useEffect(() => {
    tableContainerRef.current?.scrollTo({ top: 0, behavior: "auto" });
  }, [page]);

  const updateValue = (change) => {
    const newValue = {
      ...value,
      ...change,
    };

    setValue(newValue);
    return newValue;
  };

  const dashIfEmpty = (text, ...args) => {
    return [text || "-", ...args].filter((x) => !!x).join(", ");
  };

  const createSortHandler = (column) => (event) => {
    handleSortChange(event, column);
  };

  const handleSortChange = (event, column) => {
    const isAscending = sorting.orderBy === column && sorting.sortAscending;
    onSort(updateValue({ orderBy: column, sortAscending: !isAscending }));
  };

  const handlePageChange = (event, page) => {
    tableContainerRef.current?.scrollTo({ top: 0, behavior: "auto" });
    onChangePage(page);
  };

  const alertAllowsCrossCheckingOfReport = (alert) =>
    alert.status === alertStatus.open ||
    (alert.status === alertStatus.escalated &&
      !alert.reportWasCrossCheckedBeforeEscalation);

  const canCrossCheck = (report, reportStatus) =>
    !report.isAnonymized &&
    report.isValid &&
    !report.isMarkedAsError &&
    !report.isActivityReport &&
    report.status !== reportStatus &&
    (!report.alert || alertAllowsCrossCheckingOfReport(report.alert)) &&
    !!report.village;

  const handleGoToAlert = (projectId, alertId) => {
    // Track go to alert event
    trackEvent("GoToAlert", { projectId, alertId });
    goToAlert(projectId, alertId);
  };

  const handleAcceptReport = (reportId) => {
    // Track accept report event
    trackEvent("AcceptReport", { reportId });
    acceptReport(reportId);
  };

  const handleDismissReport = (reportId) => {
    // Track dismiss report event
    trackEvent("DismissReport", { reportId });
    dismissReport(reportId);
  };

  const getRowMenu = (row) => [
    {
      title: strings(stringKeys.reports.list.goToAlert),
      roles: accessMap.reports.goToAlert,
      disabled: !row.alert,
      action: () => handleGoToAlert(projectId, row.alert.id),
    },
    {
      title: strings(stringKeys.reports.list.acceptReport),
      roles: accessMap.reports.crossCheck,
      disabled: !canCrossCheck(row, reportStatus.accepted),
      action: () => handleAcceptReport(row.id),
    },
    {
      title: strings(stringKeys.reports.list.dismissReport),
      roles: accessMap.reports.crossCheck,
      disabled: !canCrossCheck(row, reportStatus.rejected),
      action: () => handleDismissReport(row.id),
    },
  ];

  const canEdit = (row) =>
    (!row.isActivityReport ||
      filters.dataCollectorType === DataCollectorType.unknownSender) &&
    row.status === reportStatus.new &&
    !projectIsClosed &&
    !row.dataCollectorIsDeleted;
  return (
    <Fragment>
      <TableContainer ref={tableContainerRef}>
        {isListFetching && <Loading absolute />}
        <Table stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell style={{ width: "6%", minWidth: "80px" }}>
                <TableSortLabel
                  active={sorting.orderBy === DateColumnName}
                  direction={sorting.sortAscending ? "asc" : "desc"}
                  onClick={createSortHandler(DateColumnName)}
                >
                  {strings(stringKeys.reports.list.date)}
                </TableSortLabel>
              </TableCell>
              <TableCell align="center" style={{ width: "6%", minWidth: 185 }}>
                {strings(stringKeys.reports.list.status)}
              </TableCell>
              <TableCell style={{ width: "12%" }}>
                {strings(stringKeys.reports.list.dataCollectorDisplayName)}
              </TableCell>
              <TableCell style={{ width: "20%" }}>
                {strings(stringKeys.common.location)}
              </TableCell>
              <TableCell style={{ width: "14%" }}>
                {strings(stringKeys.reports.list.healthRisk)}
              </TableCell>
              <TableCell style={{ width: "7%" }}>
                {strings(stringKeys.reports.list.malesBelowFive)}
              </TableCell>
              <TableCell style={{ width: "8%" }}>
                {strings(stringKeys.reports.list.malesAtLeastFive)}
              </TableCell>
              <TableCell style={{ width: "7%" }}>
                {strings(stringKeys.reports.list.femalesBelowFive)}
              </TableCell>
              <TableCell style={{ width: "8%" }}>
                {strings(stringKeys.reports.list.femalesAtLeastFive)}
              </TableCell>
              {filters.dataCollectorType ===
                DataCollectorType.collectionPoint && (
                <Fragment>
                  <TableCell style={{ width: "10%", minWidth: "50px" }}>
                    {strings(stringKeys.reports.list.referredCount)}
                  </TableCell>
                  
                </Fragment>
              )}
              <TableCell style={{ width: "3%" }} />
            </TableRow>
          </TableHead>
          <TableBody>
            {list.map((row) => (
              <TableRow key={row.id} hover>
                <TableCell>
                  {dayjs(row.dateTime).format("YYYY-MM-DD HH:mm")}
                </TableCell>
                <TableCell align="center">
                  {row.isActivityReport ? (
                    "-"
                  ) : (
                    <ReportStatusChip report={row} rtl={rtl} />
                  )}
                </TableCell>
                <TableCell className={styles.phoneNumber}>
                  {renderDataCollectorDisplayName(row)}
                </TableCell>
                <TableCell>
                  {dashIfEmpty(row.region, row.district, row.village, row.zone)}
                </TableCell>
                <TableCell>{dashIfEmpty(row.healthRiskName)}</TableCell>
                <TableCell>
                  {renderReportValue(row.countMalesBelowFive, isDataCollectionPointTable)}
                </TableCell>
                <TableCell>
                  {renderReportValue(row.countMalesAtLeastFive, isDataCollectionPointTable)}
                </TableCell>
                <TableCell>
                  {renderReportValue(row.countFemalesBelowFive, isDataCollectionPointTable)}
                </TableCell>
                <TableCell>
                  {renderReportValue(row.countFemalesAtLeastFive, isDataCollectionPointTable)}
                </TableCell>
                {isDataCollectionPointTable && (
                  <Fragment>
                    <TableCell>
                      {renderReportValue(row.referredCount, true)}
                    </TableCell>
                  </Fragment>
                )}
                <TableCell>
                  <TableRowActions directionRtl={rtl}>
                    <TableRowAction
                      onClick={() => goToEditing(projectId, row.id)}
                      icon={<EditIcon />}
                      title={strings(stringKeys.reports.list.editReport)}
                      roles={accessMap.reports.edit}
                      condition={canEdit(row)}
                      directionRtl={rtl}
                    />
                    {!row.isActivityReport && (
                      <TableRowMenu
                        id={row.id}
                        icon={<MoreVertIcon />}
                        isFetching={isMarkingAsError[row.id]}
                        items={getRowMenu(row)}
                        directionRtl={rtl}
                      />
                    )}
                  </TableRowActions>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        <TablePager
          totalRows={totalRows}
          rowsPerPage={rowsPerPage}
          page={page}
          onChangePage={handlePageChange}
          rtl={rtl}
        />
      </TableContainer>
    </Fragment>
  );
};

CorrectReportsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

export default CorrectReportsTable;