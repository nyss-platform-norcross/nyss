import React from "react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
} from "@material-ui/core";
import styles from "./AlertNotificationRecipients.module.scss";

const AlertNotificationRecipients = ({ recipients }) => {
  return (
    <Table className={styles.table}>
      <TableHead>
        <TableRow>
          <TableCell>Role</TableCell>
          <TableCell>Organization</TableCell>
          <TableCell>Phone number</TableCell>
          <TableCell>Email</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {recipients.map(recipient => <TableRow>
          <TableCell>{recipient.role}</TableCell>
          <TableCell>{recipient.organization}</TableCell>
          <TableCell>{recipient.phoneNumber}</TableCell>
          <TableCell>{recipient.email}</TableCell>
        </TableRow>)}
      </TableBody>
    </Table>
  );
};

export default AlertNotificationRecipients;
