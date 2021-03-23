import React from 'react';
import dayjs from "dayjs";
import { stringKeys, strings, stringsFormat } from "../../../strings";
import { TableContainer, Table, TableBody, TableCell, TableHead, TableRow } from "@material-ui/core";
import { logType, escalatedOutcomes } from '../logic/alertsConstants';

const formatString = (row) => {
  if (row.logType === logType.closedAlert && row.metadata.escalatedOutcome !== escalatedOutcomes.other) {
    return `${stringsFormat(stringKeys.alerts.constants.logType[row.logType], row.metadata)} - ${strings(stringKeys.alerts.constants.escalatedOutcomes[row.metadata.escalatedOutcome])}`;
  } else {
    return stringsFormat(stringKeys.alerts.constants.logType[row.logType], row.metadata);
  }
}

export const AlertsLogsTable = ({ list }) => {
  return (
    <TableContainer sticky="true">
      <Table>
        <TableHead>
          <TableRow>
            <TableCell style={{ width: "12%", minWidth: 150 }}>{strings(stringKeys.alerts.logs.list.date)}</TableCell>
            <TableCell style={{ width: "30%", minWidth: 200 }}>{strings(stringKeys.alerts.logs.list.logType)}</TableCell>
            <TableCell>{strings(stringKeys.alerts.logs.list.userName)}</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map(row => (
            <TableRow key={`log_item_${row.date}`} hover>
              <TableCell>{dayjs(row.date).format("YYYY-MM-DD HH:mm")}</TableCell>
              <TableCell>{formatString(row)}</TableCell>
              <TableCell>{row.userName}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
