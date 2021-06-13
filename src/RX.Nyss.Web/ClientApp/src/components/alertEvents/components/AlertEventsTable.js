import React from 'react';
import { stringKeys, strings, stringsFormat } from "../../../strings";
import { TableContainer, Table, TableBody, TableCell, TableHead, TableRow } from "@material-ui/core";
import * as dayjs from "dayjs";
import { logType } from "../logic/alertEventsConstants";
import { escalatedOutcomes } from "../../alerts/logic/alertsConstants";


const formatLogType = (row) => {
  if (row.logType != null) {
    if (row.logType === logType.closedAlert && row.metadata.escalatedOutcome !== escalatedOutcomes.other) {
      return `${stringsFormat(stringKeys.alerts.constants.logType[row.logType], row.metadata)} - ${strings(stringKeys.alerts.constants.escalatedOutcomes[row.metadata.escalatedOutcome])}`;
    } else {
      return stringsFormat(stringKeys.alerts.constants.logType[row.logType], row.metadata);
    }
  }
  else {
    return strings(stringKeys.alerts.constants.eventTypes[row.alertEventType])
  }
}

const formatSubtype = (row) => {
  if (row.alertEventSubtype !== null) {
    return strings(stringKeys.alerts.constants.eventSubtypes[row.alertEventSubtype])
  }
}


const dashIfEmpty = (text, ...args) => {
  return [text || "-", ...args].filter(x => !!x).join(", ");
};

export const AlertEventsTable = ({ list }) => {
  return (
    <TableContainer sticky="true" >
      <Table>
        <TableHead>
          <TableRow>
            <TableCell style={{ width: "12%", minWidth: 150 }}>{strings(stringKeys.alerts.logs.list.date)}</TableCell>
            <TableCell>{strings(stringKeys.alerts.logs.list.userName)}</TableCell>
            <TableCell style={{ width: "20%", minWidth: 200 }}>{strings(stringKeys.alerts.logs.list.type)}</TableCell>
            <TableCell style={{ width: "20%", minWidth: 200 }}>{strings(stringKeys.alerts.logs.list.subtype)}</TableCell>
            <TableCell style={{ width: "40%", minWidth: 200 }}>{strings(stringKeys.alerts.logs.list.text)}</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {!!list && list.map((row, index) => (
            <TableRow key={index} hover>
              <TableCell>{dayjs(row.date).format("YYYY-MM-DD HH:mm")}</TableCell>
              <TableCell>{dashIfEmpty(row.loggedBy)}</TableCell>
              <TableCell>{formatLogType(row)}</TableCell>
              <TableCell>{dashIfEmpty(formatSubtype(row))}</TableCell>
              <TableCell>{dashIfEmpty(row.text)}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
