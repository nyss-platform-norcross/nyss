import styles from '../common/table/Table.module.scss';
import React from 'react';
import PropTypes from "prop-types";
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import ClearIcon from '@material-ui/icons/Clear';
import EditIcon from '@material-ui/icons/Edit';
import { TableRowAction } from '../common/tableRowAction/TableRowAction';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';

export const ReportsTable = ({ isListFetching, list }) => {
  if (isListFetching) {
    return <Loading />;
  }

  return (
    <Table>
      <TableHead>
        <TableRow>
          <TableCell style={{ width: "6%" }}>{strings(stringKeys.reports.list.date)}</TableCell>
          <TableCell style={{ width: "6%" }}>{strings(stringKeys.reports.list.time)}</TableCell>
          <TableCell style={{ width: "6%" }}>{strings(stringKeys.reports.list.healthRisk)}</TableCell>
          <TableCell style={{ width: "6%" }}>{strings(stringKeys.reports.list.status)}</TableCell>
          <TableCell style={{ width: "6%" }}>{strings(stringKeys.reports.list.region)}</TableCell>
          <TableCell style={{ width: "6%" }}>{strings(stringKeys.reports.list.district)}</TableCell>
          <TableCell style={{ width: "6%" }}>{strings(stringKeys.reports.list.village)}</TableCell>
          <TableCell style={{ width: "6%" }}>{strings(stringKeys.reports.list.zone)}</TableCell>
          <TableCell style={{ width: "6%" }}>{strings(stringKeys.reports.list.dataCollectorDisplayName)}</TableCell>
          <TableCell style={{ width: "6%" }}>{strings(stringKeys.reports.list.dataCollectorPhoneNumber)}</TableCell>
          <TableCell style={{ width: "6%" }}>{strings(stringKeys.reports.list.malesBelowFive)}</TableCell>
          <TableCell style={{ width: "6%" }}>{strings(stringKeys.reports.list.malesAtLeastFive)}</TableCell>
          <TableCell style={{ width: "6%" }}>{strings(stringKeys.reports.list.femalesBelowFive)}</TableCell>
          <TableCell style={{ width: "6%" }}>{strings(stringKeys.reports.list.femalesAtLeastFive)}</TableCell>
          <TableCell style={{ width: "6%" }} />
        </TableRow>
      </TableHead>
      <TableBody>
        {list.map(row => (
          <TableRow key={row.id} hover className={styles.clickableRow}>
            <TableCell>{row.createdAt}</TableCell>
            <TableCell>{row.createdAt}</TableCell>
            <TableCell>{row.healthRisk}</TableCell>
            <TableCell>{row.status}</TableCell>
            <TableCell>{row.region}</TableCell>
            <TableCell>{row.district}</TableCell>
            <TableCell>{row.village}</TableCell>
            <TableCell>{row.zone}</TableCell>
            <TableCell>{row.dataCollectorDisplayName}</TableCell>
            <TableCell>{row.dataCollectorPhoneNumber}</TableCell>
            <TableCell>{row.countMalesBelowFive}</TableCell>
            <TableCell>{row.countMalesAtLeastFive}</TableCell>
            <TableCell>{row.countFemalesBelowFive}</TableCell>
            <TableCell>{row.countFemalesAtLeastFive}</TableCell>            
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}

ReportsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default ReportsTable;