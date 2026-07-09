import React, { Fragment, useState } from "react";
import { stringKeys, strings, stringsFormat } from "../../../strings";
import {
  TableContainer,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
} from "@material-ui/core";
import * as dayjs from "dayjs";
import { logType } from "../logic/alertEventsConstants";
import { escalatedOutcomes } from "../../alerts/logic/alertsConstants";
import { AlertEventExpandableText } from "./AlertEventExpandableText";
import { EditAlertEventDialog } from "./EditAlertEventDialog";
import { TableRowActions } from "../../common/tableRowAction/TableRowActions";
import { TableRowAction } from "../../common/tableRowAction/TableRowAction";
import { accessMap } from "../../../authentication/accessMap";
import ClearIcon from "@material-ui/icons/Clear";
import EditIcon from "@material-ui/icons/Edit";

export const AlertEventsTable = ({
  alertId,
  list,
  edit,
  remove,
  isRemoving,
}) => {
  const [editDialogOpened, setEditDialogOpened] = useState(false);
  const [selectedEvent, setSelectedEvent] = useState(null);

  const selectTypeAndFormat = (row) => {
    if (row.logType != null) {
      if (
        row.logType === logType.closedAlert &&
        !!row.metadata.escalatedOutcome &&
        row.metadata.escalatedOutcome !== escalatedOutcomes.other
      ) {
        return `${stringsFormat(
          stringKeys.alerts.constants.logType[row.logType],
          row.metadata,
        )} - ${strings(
          stringKeys.alerts.constants.escalatedOutcomes[
            row.metadata.escalatedOutcome
          ],
        )}`;
      } else {
        return stringsFormat(
          stringKeys.alerts.constants.logType[row.logType],
          row.metadata,
        );
      }
    } else {
      return strings(
        stringKeys.alerts.constants.eventTypes[row.alertEventType],
      );
    }
  };

  const formatEventType = (row) => {
    if (row.alertEventType !== null) {
      return strings(
        stringKeys.alerts.constants.eventTypes[row.alertEventType],
      );
    }
  };

  const formatEventSubtype = (row) => {
    if (row.alertEventSubtype !== null) {
      return strings(
        stringKeys.alerts.constants.eventSubtypes[row.alertEventSubtype],
      );
    }
  };

  const dashIfEmpty = (text, ...args) => {
    return [text || "-", ...args].filter((x) => !!x).join(", ");
  };

  const showEditDialog = (eventLogItem) => {
    setSelectedEvent(eventLogItem);
    setEditDialogOpened(true);
  };

  return (
    <Fragment>
      <TableContainer>
        <Table stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell style={{ width: "10%", minWidth: 150 }}>
                {strings(stringKeys.alerts.eventLog.list.date)}
              </TableCell>
              <TableCell style={{ width: "20%", minWidth: 200 }}>
                {strings(stringKeys.alerts.eventLog.list.type)}
              </TableCell>
              <TableCell style={{ width: "20%", minWidth: 200 }}>
                {strings(stringKeys.alerts.eventLog.list.subtype)}
              </TableCell>
              <TableCell style={{ width: "10%", minWidth: 150 }}>
                {strings(stringKeys.alerts.eventLog.list.userName)}
              </TableCell>
              <TableCell style={{ width: "35%", minWidth: 200 }}>
                {strings(stringKeys.alerts.eventLog.list.comment)}
              </TableCell>
              <TableCell style={{ width: "5%", minWidth: 50 }} />
            </TableRow>
          </TableHead>

          <TableBody>
            {!!list &&
              list.map((row, index) => (
                <TableRow key={index} hover>
                  <TableCell>
                    {dayjs(row.date).format("YYYY-MM-DD HH:mm")}
                  </TableCell>
                  <TableCell>{selectTypeAndFormat(row)}</TableCell>
                  <TableCell>{formatEventSubtype(row)}</TableCell>
                  <TableCell>{dashIfEmpty(row.loggedBy)}</TableCell>
                  <TableCell>
                    <AlertEventExpandableText text={row.text} maxLength={70} />
                  </TableCell>

                  <TableCell>
                    {row.alertEventType && (
                      <TableRowActions>
                        <TableRowAction
                          onClick={() =>
                            showEditDialog(
                              row,
                              formatEventType(row),
                              formatEventSubtype(row),
                            )
                          }
                          roles={accessMap.alertEvents.edit}
                          icon={<EditIcon />}
                          title={"Edit"}
                        />
                        <TableRowAction
                          onClick={() => remove(alertId, row.alertEventLogId)}
                          roles={accessMap.alertEvents.delete}
                          confirmationText={strings(
                            stringKeys.alerts.eventLog.list.removalConfirmation,
                          )}
                          icon={<ClearIcon />}
                          title={"Delete"}
                          isFetching={isRemoving[row.alertEventLogId]}
                        />
                      </TableRowActions>
                    )}
                  </TableCell>
                </TableRow>
              ))}
          </TableBody>
        </Table>
      </TableContainer>

      <EditAlertEventDialog
        open={editDialogOpened}
        close={() => setEditDialogOpened(false)}
        edit={edit}
        alertId={alertId}
        eventLogItem={selectedEvent}
        formattedEventType={selectedEvent && formatEventType(selectedEvent)}
        formattedEventSubtype={
          selectedEvent && formatEventSubtype(selectedEvent)
        }
      />
    </Fragment>
  );
};
