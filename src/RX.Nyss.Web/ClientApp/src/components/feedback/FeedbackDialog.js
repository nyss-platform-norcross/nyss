import React from 'react';
import DialogTitle from '@material-ui/core/DialogTitle';
import DialogContent from '@material-ui/core/DialogContent';
import Dialog from '@material-ui/core/Dialog';
import useMediaQuery from '@material-ui/core/useMediaQuery';
import { useTheme } from "@material-ui/core";
import { strings, stringKeys } from "../../strings";

export const FeedbackDialog = ({ isOpened, close }) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('xs'));

  if (!isOpened) {
    return null;
  }

  return (
    <Dialog onClose={close} open={isOpened} fullScreen={fullScreen}>
      <DialogTitle>{strings(stringKeys.feedback.dialogTitle)}</DialogTitle>
      <DialogContent>Hey man!</DialogContent>
    </Dialog>
  );
}