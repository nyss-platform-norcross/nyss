import React from "react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableContainer
} from "@material-ui/core";
import { stringKeys, strings } from "../../../strings";
import styles from "./AlertNotificationRecipients.module.scss";

const AlertNotificationRecipients = ({ recipients }) => {
  return (
    <TableContainer>
    <Table className={styles.table} stickyHeader>
      <TableHead>
        <TableRow>
          <TableCell>
            {strings(stringKeys.alerts.assess.escalatedTo.role)}
          </TableCell>
          <TableCell>
            {strings(stringKeys.alerts.assess.escalatedTo.organization)}
          </TableCell>
          <TableCell>
            {strings(stringKeys.alerts.assess.escalatedTo.phoneNumber)}
          </TableCell>
          <TableCell>{strings(stringKeys.common.email)}</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {recipients.map((recipient, i) => (
          <TableRow key={`recipient_${i}`}>
            <TableCell>{recipient.role}</TableCell>
            <TableCell>{recipient.organization}</TableCell>
            <TableCell>{recipient.phoneNumber}</TableCell>
            <TableCell>{recipient.email}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
    </TableContainer>
  );
};

export default AlertNotificationRecipients;
