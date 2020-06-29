import styles from '../../common/table/Table.module.scss';
import alertTableStyles from './AlertsTable.module.scss';
import React from 'react';
import PropTypes from "prop-types";
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import { Loading } from '../../common/loading/Loading';
import { strings, stringKeys } from '../../../strings';
import dayjs from "dayjs";
import TablePager from '../../common/tablePagination/TablePager';
import { TableNoData } from '../../common/table/TableNoData';
import { TableContainer } from '../../common/table/TableContainer';
import HelpOutlineOutlinedIcon from '@material-ui/icons/HelpOutlineOutlined';
import { assessmentStatus, closeOptions } from '../logic/alertsConstants';
import { Tooltip } from '@material-ui/core';

export const AlertsTable = ({ isListFetching, list, projectId, goToAssessment, getList, page, rowsPerPage, totalRows }) => {
  const onChangePage = (event, page) => {
    getList(projectId, page);
  };

  const handleTooltipClick = (event) => {
    event.stopPropagation();
  }

  if (!list.length) {
    return <TableNoData />;
  }

  const renderStatus = (alert) => {
    if (alert.status === assessmentStatus.closed) {
      const tooltipText = alert.closeOption === undefined || alert.closeOption === closeOptions.other ? alert.comments : strings(stringKeys.alerts.constants.closeOptions[alert.closeOption]);
      return (
        <div className={alertTableStyles.closeStatus}>
          {strings(stringKeys.alerts.constants.alertStatus[alert.status])}
          <Tooltip title={tooltipText} onClick={handleTooltipClick} arrow className={alertTableStyles.tooltip}>
            <HelpOutlineOutlinedIcon fontSize="small" />
          </Tooltip>
        </div>
      );
    } else {
      return strings(stringKeys.alerts.constants.alertStatus[alert.status]);
    }
  }

  return (
    <TableContainer sticky>
      {isListFetching && <Loading absolute />}
      <Table>
        <TableHead>
          <TableRow>
            <TableCell style={{ width: "6%", "minWidth": "100px" }}>{strings(stringKeys.alerts.list.dateTime)}</TableCell>
            <TableCell style={{ width: "10%", "minWidth": "200px" }}>{strings(stringKeys.alerts.list.healthRisk)}</TableCell>
            <TableCell style={{ width: "6%" }}>{strings(stringKeys.alerts.list.reportCount)}</TableCell>
            <TableCell style={{ width: "18%" }}>{strings(stringKeys.alerts.list.village)}</TableCell>
            <TableCell style={{ width: "7%" }}>{strings(stringKeys.alerts.list.status)}</TableCell>
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
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <TablePager totalRows={totalRows} rowsPerPage={rowsPerPage} page={page} onChangePage={onChangePage} />
    </TableContainer>
  );
}

AlertsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default AlertsTable;
