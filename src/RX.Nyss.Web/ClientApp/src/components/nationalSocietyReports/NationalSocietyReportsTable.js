import styles from '../common/table/Table.module.scss';
import React from 'react';
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

export const NationalSocietyReportsTable = ({ isListFetching, list, nationalSocietyId, getList, page, rowsPerPage, totalRows }) => {

  const onChangePage = (event, page) => {
    getList(nationalSocietyId, page);
  };

  const dashIfEmpty = (text) => {
    return text || "-";
  };

  return (
    <div className={styles.tableContainer}>
      {isListFetching && <Loading absolute />}
      <Table>
        <TableHead>
          <TableRow>
            <TableCell style={{ width: "6%", "minWidth": "100px" }}>{strings(stringKeys.nationalSocietyReports.list.date)}</TableCell>
            <TableCell style={{ width: "5%" }}>{strings(stringKeys.nationalSocietyReports.list.time)}</TableCell>
            <TableCell style={{ width: "7%" }}>{strings(stringKeys.nationalSocietyReports.list.status)}</TableCell>
            <TableCell style={{ width: "6%", "minWidth": "200px" }}>{strings(stringKeys.nationalSocietyReports.list.dataCollectorDisplayName)}</TableCell>
            <TableCell style={{ width: "12%" }}>{strings(stringKeys.nationalSocietyReports.list.dataCollectorPhoneNumber)}</TableCell>
            <TableCell style={{ width: "18%", "minWidth": "250px" }}>{strings(stringKeys.nationalSocietyReports.list.location)}</TableCell>
            <TableCell style={{ width: "10%" }}>{strings(stringKeys.nationalSocietyReports.list.healthRisk)}</TableCell>
            <TableCell style={{ width: "9%", "minWidth": "50px" }}>{strings(stringKeys.nationalSocietyReports.list.malesBelowFive)}</TableCell>
            <TableCell style={{ width: "9%", "minWidth": "50px" }}>{strings(stringKeys.nationalSocietyReports.list.malesAtLeastFive)}</TableCell>
            <TableCell style={{ width: "9%", "minWidth": "50px" }}>{strings(stringKeys.nationalSocietyReports.list.femalesBelowFive)}</TableCell>
            <TableCell style={{ width: "9%", "minWidth": "50px" }}>{strings(stringKeys.nationalSocietyReports.list.femalesAtLeastFive)}</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map(row => (
            <TableRow key={row.id} hover>
              <TableCell>{dayjs(row.dateTime).format('YYYY-MM-DD')}</TableCell>
              <TableCell>{dayjs(row.dateTime).format('HH:mm')}</TableCell>
              <TableCell>{row.isValid ? strings(stringKeys.nationalSocietyReports.list.success) : strings(stringKeys.nationalSocietyReports.list.error)}</TableCell>
              <TableCell>{row.dataCollectorDisplayName}</TableCell>
              <TableCell>{row.phoneNumber}</TableCell>
              <TableCell>{row.region}, {row.district}, {row.village}{row.zone ? ',' : null} {row.zone}</TableCell>
              <TableCell>{dashIfEmpty(row.healthRiskName)}</TableCell>
              <TableCell>{dashIfEmpty(row.countMalesBelowFive)}</TableCell>
              <TableCell>{dashIfEmpty(row.countMalesAtLeastFive)}</TableCell>
              <TableCell>{dashIfEmpty(row.countFemalesBelowFive)}</TableCell>
              <TableCell>{dashIfEmpty(row.countFemalesAtLeastFive)}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <TablePager totalRows={totalRows} rowsPerPage={rowsPerPage} page={page} onChangePage={onChangePage} />
    </div>
  );
}

NationalSocietyReportsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default NationalSocietyReportsTable;
