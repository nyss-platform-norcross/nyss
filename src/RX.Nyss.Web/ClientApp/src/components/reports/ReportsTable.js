import styles from '../common/table/Table.module.scss';
import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import dayjs from "dayjs";
import TablePager from '../common/tablePagination/TablePager';

export const ReportsTable = ({ isListFetching, list, projectId, getList, page, rowsPerPage, totalRows, reportListType }) => {

  const onChangePage = (event, page) => {
    getList(projectId, page);
  };

  const dashIfEmpty = (text, ...args) => {
    return [text || "-", ...args].filter(x => !!x).join(", ");
  };

  return (
    <div className={styles.tableContainer}>
      {isListFetching && <Loading absolute />}
      <Table stickyHeader>
        <TableHead>
          <TableRow>
            <TableCell style={{ width: "9%", minWidth: "115px" }}>{strings(stringKeys.nationalSocietyReports.list.date)}</TableCell>
            <TableCell style={{ width: "6%" }}>{strings(stringKeys.nationalSocietyReports.list.status)}</TableCell>
            <TableCell style={{ width: "11%" }}>{strings(stringKeys.nationalSocietyReports.list.dataCollectorDisplayName)}</TableCell>
            <TableCell style={{ width: "8%" }}>{strings(stringKeys.nationalSocietyReports.list.dataCollectorPhoneNumber)}</TableCell>
            <TableCell style={{ width: "18%" }}>{strings(stringKeys.nationalSocietyReports.list.location)}</TableCell>
            <TableCell style={{ width: "11%" }}>{strings(stringKeys.nationalSocietyReports.list.healthRisk)}</TableCell>
            <TableCell style={{ width: "6%" }}>{strings(stringKeys.nationalSocietyReports.list.malesBelowFive)}</TableCell>
            <TableCell style={{ width: "7%" }}>{strings(stringKeys.nationalSocietyReports.list.malesAtLeastFive)}</TableCell>
            <TableCell style={{ width: "6%" }}>{strings(stringKeys.nationalSocietyReports.list.femalesBelowFive)}</TableCell>
            <TableCell style={{ width: "7%" }}>{strings(stringKeys.nationalSocietyReports.list.femalesAtLeastFive)}</TableCell>
            {reportListType === "fromDcp" &&
              <Fragment>
                <TableCell style={{ width: "9%", minWidth: "50px" }}>{strings(stringKeys.reports.list.referredCount)}</TableCell>
                <TableCell style={{ width: "9%", minWidth: "50px" }}>{strings(stringKeys.reports.list.deathCount)}</TableCell>
                <TableCell style={{ width: "9%", minWidth: "50px" }}>{strings(stringKeys.reports.list.fromOtherVillagesCount)}</TableCell>
              </Fragment>
            }
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map(row => (
            <TableRow key={row.id} hover>
              <TableCell>{dayjs(row.dateTime).format('YYYY-MM-DD HH:mm')}</TableCell>
              <TableCell>{row.isValid ? strings(stringKeys.reports.list.success) : strings(stringKeys.reports.list.error)}</TableCell>
              <TableCell>{row.dataCollectorDisplayName}</TableCell>
              <TableCell>{row.phoneNumber}</TableCell>
              <TableCell>{dashIfEmpty(row.region, row.district, row.village, row.zone)}</TableCell>
              <TableCell>{dashIfEmpty(row.healthRiskName)}</TableCell>
              <TableCell>{dashIfEmpty(row.countMalesBelowFive)}</TableCell>
              <TableCell>{dashIfEmpty(row.countMalesAtLeastFive)}</TableCell>
              <TableCell>{dashIfEmpty(row.countFemalesBelowFive)}</TableCell>
              <TableCell>{dashIfEmpty(row.countFemalesAtLeastFive)}</TableCell>
              {reportListType === "fromDcp" &&
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
      <TablePager totalRows={totalRows} rowsPerPage={rowsPerPage} page={page} onChangePage={onChangePage} />
    </div>
  );
}

ReportsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default ReportsTable;
