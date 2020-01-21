import styles from "./NationalSocietyReportsTable.module.scss";

import React, { Fragment, useState } from 'react';
import PropTypes from "prop-types";
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import Tooltip from '@material-ui/core/Tooltip';
import { TableContainer } from '../common/table/TableContainer';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import dayjs from "dayjs";
import TablePager from '../common/tablePagination/TablePager';
import { ReportListType } from '../common/filters/logic/reportFilterConstsants'
import { DateColumnName } from './logic/nationalSocietyReportsConstants'
import TableSortLabel from '@material-ui/core/TableSortLabel';
import { Typography } from "@material-ui/core";

export const NationalSocietyReportsTable = ({ isListFetching, list, page, onChangePage, rowsPerPage, totalRows, reportsType, filters, sorting, onSort }) => {

  const [value, setValue] = useState(sorting);

  const updateValue = (change) => {
    const newValue = {
      ...value,
      ...change
    }

    setValue(newValue);
    return newValue;
  };

  const dashIfEmpty = (text, ...args) => {
    return [text || "-", ...args].filter(x => !!x).join(", ");
  };

  const createSortHandler = column => event => {
    handleSortChange(event, column);
  };

  const handleSortChange = (event, column) => {
    const isAscending = sorting.orderBy === column && sorting.sortAscending;
    onSort(updateValue({ orderBy: column, sortAscending: !isAscending }));
  }

  const handlePageChange = (event, page) => {
    onChangePage(page);
  }

  return (
    <TableContainer>
      {isListFetching && <Loading absolute />}
      <Table stickyHeader>
        <TableHead>
          <TableRow>
            <TableCell style={{ width: "6%", minWidth: "80px" }} className={styles.tableHeader}>
              <TableSortLabel
                active={sorting.orderBy === DateColumnName}
                direction={sorting.sortAscending ? 'asc' : 'desc'}
                onClick={createSortHandler(DateColumnName)}
              >
                {strings(stringKeys.nationalSocietyReports.list.date)}
              </TableSortLabel>
            </TableCell>
            <TableCell style={{ width: "6%" }} className={styles.tableHeader}>{strings(stringKeys.nationalSocietyReports.list.status)}</TableCell>
            <TableCell style={{ width: "11%" }} className={styles.tableHeader}>{strings(stringKeys.nationalSocietyReports.list.project)}</TableCell>
            <TableCell style={{ width: "11%" }} className={styles.tableHeader}>{strings(stringKeys.nationalSocietyReports.list.dataCollectorDisplayName)}</TableCell>
            <TableCell style={{ width: "14%" }} className={styles.tableHeader}>{strings(stringKeys.nationalSocietyReports.list.location)}</TableCell>
            <TableCell style={{ width: "11%" }} className={styles.tableHeader}>{strings(stringKeys.nationalSocietyReports.list.healthRisk)}</TableCell>
            {!filters.status &&
              <TableCell style={{ width: "11%" }} className={styles.tableHeader}>{strings(stringKeys.nationalSocietyReports.list.message)}</TableCell>
            }
            <TableCell style={{ width: "7%" }} className={styles.tableHeader}>{strings(stringKeys.nationalSocietyReports.list.malesBelowFive)}</TableCell>
            <TableCell style={{ width: "8%" }} className={styles.tableHeader}>{strings(stringKeys.nationalSocietyReports.list.malesAtLeastFive)}</TableCell>
            <TableCell style={{ width: "7%" }} className={styles.tableHeader}>{strings(stringKeys.nationalSocietyReports.list.femalesBelowFive)}</TableCell>
            <TableCell style={{ width: "8%" }} className={styles.tableHeader}>{strings(stringKeys.nationalSocietyReports.list.femalesAtLeastFive)}</TableCell>
            {reportsType === ReportListType.fromDcp &&
              <Fragment>
                <TableCell style={{ width: "10%", minWidth: "50px" }} className={styles.tableHeader}>{strings(stringKeys.nationalSocietyReports.list.referredCount)}</TableCell>
                <TableCell style={{ width: "10%", minWidth: "50px" }} className={styles.tableHeader}>{strings(stringKeys.nationalSocietyReports.list.deathCount)}</TableCell>
                <TableCell style={{ width: "10%", minWidth: "50px" }} className={styles.tableHeader}>{strings(stringKeys.nationalSocietyReports.list.fromOtherVillagesCount)}</TableCell>
              </Fragment>
            }
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map(row => (
            <TableRow key={row.id} hover>
              <TableCell>
                <Tooltip title={row.projectTimeZone || "UTC"}>
                  <span>{dayjs(row.dateTime).format('YYYY-MM-DD HH:mm')}</span>
                </Tooltip>
              </TableCell>
              <TableCell>
                {row.isMarkedAsError
                  ? strings(stringKeys.nationalSocietyReports.list.markedAsError)
                  : row.isValid ? strings(stringKeys.nationalSocietyReports.list.success) : strings(stringKeys.nationalSocietyReports.list.error)}
              </TableCell>
              <TableCell>{dashIfEmpty(row.projectName)}</TableCell>
              <TableCell>
                {row.dataCollectorDisplayName}
                {row.dataCollectorDisplayName ? <br /> : ""}
                {row.phoneNumber}
              </TableCell>
              <TableCell>{dashIfEmpty(row.region, row.district, row.village, row.zone)}</TableCell>
              <TableCell>{dashIfEmpty(row.healthRiskName)}</TableCell>
              {!filters.status &&
                <TableCell>
                  <Typography className={styles.message} title={row.message}>{dashIfEmpty(row.message)}</Typography>
                </TableCell>
              }
              <TableCell>{dashIfEmpty(row.countMalesBelowFive)}</TableCell>
              <TableCell>{dashIfEmpty(row.countMalesAtLeastFive)}</TableCell>
              <TableCell>{dashIfEmpty(row.countFemalesBelowFive)}</TableCell>
              <TableCell>{dashIfEmpty(row.countFemalesAtLeastFive)}</TableCell>
              {reportsType === ReportListType.fromDcp &&
                <Fragment>
                  <TableCell>{dashIfEmpty(row.referredCount)}</TableCell>
                  <TableCell>{dashIfEmpty(row.deathCount)}</TableCell>
                  <TableCell>{dashIfEmpty(row.fromOtherVillagesCount)}</TableCell>
                </Fragment>
              }
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <TablePager totalRows={totalRows} rowsPerPage={rowsPerPage} page={page} onChangePage={handlePageChange} />
    </TableContainer>
  );
}

NationalSocietyReportsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default NationalSocietyReportsTable;
