import styles from '../../common/table/Table.module.scss';
import alertTableStyles from './AlertsTable.module.scss';
import React from 'react';
import PropTypes from "prop-types";
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import { strings, stringKeys } from '../../../strings';
import dayjs from "dayjs";
import TablePager from '../../common/tablePagination/TablePager';
import { TableNoData } from '../../common/table/TableNoData';
import { TableContainer } from '../../common/table/TableContainer';
import InfoIcon from '@material-ui/icons/InfoOutlined';
import { assessmentStatus, escalatedOutcomes, statusColumn, timeTriggeredColumn } from '../logic/alertsConstants';
import { TableSortLabel, Tooltip } from '@material-ui/core';
import { useSelector } from 'react-redux';

export const AlertsTable = ({ isListFetching, list, projectId, goToAssessment, onChangePage, onSort, page, rowsPerPage, totalRows }) => {
  const filters = useSelector(state => state.alerts.filters);

  const handlePageChange = (event, page) => {
    onChangePage(page);
  };

  const createSortHandler = column => event => {
    handleSortChange(event, column);
  };

  const handleSortChange = (event, column) => {
    const isAscending = filters.orderBy === column && filters.sortAscending;
    onSort({ ...filters, orderBy: column, sortAscending: !isAscending });
  }

  const handleTooltipClick = (event) => {
    event.stopPropagation();
  }

  if (!list.length) {
    return <TableNoData />;
  }

  const renderStatus = (alert) => {
    if (alert.status === assessmentStatus.closed) {
      const tooltipText = alert.escalatedOutcome === undefined || alert.escalatedOutcome === escalatedOutcomes.other ? alert.comments : strings(stringKeys.alerts.constants.escalatedOutcomes[alert.escalatedOutcome]);
      return (
        <div className={alertTableStyles.closeStatus}>
          {strings(stringKeys.alerts.constants.alertStatus[alert.status])}
          <Tooltip title={tooltipText} onClick={handleTooltipClick} arrow className={alertTableStyles.tooltip}>
            <InfoIcon fontSize="small" />
          </Tooltip>
        </div>
      );
    } else {
      return strings(stringKeys.alerts.constants.alertStatus[alert.status]);
    }
  }

  return !!filters && (
    <TableContainer sticky isFetching={isListFetching}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell style={{ width: "6%", "minWidth": "100px" }}>
              <TableSortLabel
                active={filters.orderBy === timeTriggeredColumn}
                direction={filters.sortAscending ? 'asc' : 'desc'}
                onClick={createSortHandler(timeTriggeredColumn)}
              >
                {strings(stringKeys.alerts.list.dateTime)}
              </TableSortLabel>
            </TableCell>
            <TableCell style={{ width: "10%", "minWidth": "200px" }}>{strings(stringKeys.alerts.list.healthRisk)}</TableCell>
            <TableCell style={{ width: "6%" }}>{strings(stringKeys.alerts.list.reportCount)}</TableCell>
            <TableCell style={{ width: "18%" }}>{strings(stringKeys.alerts.list.village)}</TableCell>
            <TableCell style={{ width: "7%" }}>
              <TableSortLabel
                active={filters.orderBy === statusColumn}
                direction={filters.sortAscending ? 'asc' : 'desc'}
                onClick={createSortHandler(statusColumn)}
              >
                {strings(stringKeys.alerts.list.status)}
              </TableSortLabel>
            </TableCell>
            <TableCell style={{ width: "6%" }}>{strings(stringKeys.alerts.list.id)}</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map(row => (
            <TableRow key={row.id} hover onClick={() => goToAssessment(projectId, row.id)} className={styles.clickableRow}>
              <TableCell>{dayjs(row.createdAt).format('YYYY-MM-DD HH:mm')}</TableCell>
              <TableCell>{row.healthRisk}</TableCell>
              <TableCell>{row.reportCount}</TableCell>
              <TableCell>{[row.lastReportRegion, row.lastReportDistrict, row.lastReportVillage].filter(l => l).join(", ")}</TableCell>
              <TableCell>{renderStatus(row)}</TableCell>
              <TableCell>{row.id}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <TablePager totalRows={totalRows} rowsPerPage={rowsPerPage} page={page} onChangePage={handlePageChange} />
    </TableContainer>
  );
}

AlertsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default AlertsTable;
