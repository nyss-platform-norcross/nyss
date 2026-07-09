import styles from "../../common/table/Table.module.scss";
import React from "react";
import PropTypes from "prop-types";
import { strings, stringKeys } from "../../../strings";
import dayjs from "dayjs";
import TablePager from "../../common/tablePagination/TablePager";
import { TableNoData } from "../../common/table/TableNoData";
import { statusColumn, timeTriggeredColumn } from "../logic/alertsConstants";
import {
  TableSortLabel,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableContainer,
} from "@material-ui/core";
import { useSelector } from "react-redux";
import { AlertStatusChip } from "../../common/chip/AlertStatusChip";
import { trackEvent } from "../../../utils/appInsightsHelper";
import { Loading } from "../../common/loading/Loading";

export const AlertsTable = ({
  isListFetching,
  list,
  projectId,
  goToAssessment,
  onChangePage,
  onSort,
  page,
  rowsPerPage,
  totalRows,
  rtl,
}) => {
  const filters = useSelector((state) => state.alerts.filters);

  const handlePageChange = (event, page) => {
    onChangePage(page);
  };

  const createSortHandler = (column) => (event) => {
    handleSortChange(event, column);
  };

  const handleSortChange = (event, column) => {
    const isAscending = filters.orderBy === column && filters.sortAscending;
    onSort({ ...filters, orderBy: column, sortAscending: !isAscending });
  };

  const handleGoToAssessment = (projectId, AlertId) => {
    const pageContent = document.getElementById("pageContent");
    const listState = {
      page,
      scrollTop: pageContent ? pageContent.scrollTop : 0,
    };

    // Track goToAlertAssessment event
    trackEvent("goToAlertAssessment", { projectId, AlertId });
    goToAssessment(projectId, AlertId, listState);
  };

  if (isListFetching) {
    return <Loading />;
  }

  if (!list.length) {
    return <TableNoData />;
  }

  return (
    !!filters && (
      <TableContainer>
        <Table stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell style={{ width: "6%", minWidth: "100px" }}>
                <TableSortLabel
                  active={filters.orderBy === timeTriggeredColumn}
                  direction={filters.sortAscending ? "asc" : "desc"}
                  onClick={createSortHandler(timeTriggeredColumn)}
                >
                  {strings(stringKeys.alerts.list.dateTime)}
                </TableSortLabel>
              </TableCell>
              <TableCell style={{ width: "10%", minWidth: "200px" }}>
                {strings(stringKeys.alerts.list.healthRisk)}
              </TableCell>
              <TableCell style={{ width: "6%" }}>
                {strings(stringKeys.alerts.list.reportCount)}
              </TableCell>
              <TableCell style={{ width: "18%" }}>
                {strings(stringKeys.alerts.list.village)}
              </TableCell>
              <TableCell align="center" style={{ width: "7%" }}>
                <TableSortLabel
                  hideSortIcon={false}
                  active={filters.orderBy === statusColumn}
                  direction={filters.sortAscending ? "asc" : "desc"}
                  onClick={createSortHandler(statusColumn)}
                >
                  {strings(stringKeys.alerts.list.status)}
                </TableSortLabel>
              </TableCell>
              <TableCell style={{ width: "6%" }}>
                {strings(stringKeys.alerts.list.id)}
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {list.map((row) => (
              <TableRow
                key={row.id}
                hover
                onClick={() => handleGoToAssessment(projectId, row.id)}
                className={styles.clickableRow}
              >
                <TableCell>
                  {dayjs(row.createdAt).format("YYYY-MM-DD HH:mm")}
                </TableCell>
                <TableCell>{row.healthRisk}</TableCell>
                <TableCell>{row.reportCount}</TableCell>
                <TableCell>
                  {[
                    row.lastReportRegion,
                    row.lastReportDistrict,
                    row.lastReportVillage,
                    row.lastReportZone,
                  ]
                    .filter((l) => l)
                    .join(", ")}
                </TableCell>
                <TableCell align="center">
                  <AlertStatusChip status={row.status} />
                </TableCell>
                <TableCell>{row.id}</TableCell>
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
    )
  );
};

AlertsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

export default AlertsTable;
