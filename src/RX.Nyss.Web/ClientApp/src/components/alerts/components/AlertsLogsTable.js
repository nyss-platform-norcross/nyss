import styles from "./AlertsAssessmentReport.module.scss";

import React from 'react';
import dayjs from "dayjs";
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import { stringKeys, strings, stringsFormat } from "../../../strings";
import { TableContainer } from "@material-ui/core";

export const AlertsLogsTable = ({ list }) => {
  return (
    <TableContainer>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell style={{ width: "12%", minWidth: 115 }}>{strings(stringKeys.alerts.logs.list.date)}</TableCell>
            <TableCell style={{ width: "12%", minWidth: 200 }}>{strings(stringKeys.alerts.logs.list.logType)}</TableCell>
            <TableCell>{strings(stringKeys.alerts.logs.list.userName)}</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map(row => (
            <TableRow key={`log_item_${row.id}`} hover>
              <TableCell>{dayjs(row.date).format("YYYY-MM-DD HH:mm")}</TableCell>
              <TableCell>{stringsFormat(stringKeys.alerts.constants.logType[row.logType], row.metadata)}</TableCell>
              <TableCell>{row.userName}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
