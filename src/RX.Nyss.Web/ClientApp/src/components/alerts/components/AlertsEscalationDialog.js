import React from 'react';
import DialogTitle from '@material-ui/core/DialogTitle';
import Dialog from '@material-ui/core/Dialog';
import { strings, stringKeys } from "../../../strings";
import DisplayField from "../../forms/DisplayField";
import FormActions from "../../forms/formActions/FormActions";
import SubmitButton from "../../forms/submitButton/SubmitButton";
import Button from "@material-ui/core/Button";
import DialogContent from "@material-ui/core/DialogContent";
import useMediaQuery from '@material-ui/core/useMediaQuery';
import { useTheme } from "@material-ui/core";

export const AlertsEscalationDialog = ({ isOpened, close, alertId, isEscalating, escalateAlert, notificationEmails, notificationPhoneNumbers }) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('xs'));

  return (
    <Dialog onClose={close} open={isOpened} fullScreen={fullScreen}>
      <DialogTitle>{strings(stringKeys.alerts.assess.alert.escalateConfirmation)}</DialogTitle>
      <DialogContent>
        <DisplayField label={strings(stringKeys.alerts.assess.alert.escalateNotificationEmails)}>
          {notificationEmails.map(email => (
            <div>{email}</div>
          ))}
        </DisplayField>

        <FormActions>
          <Button onClick={close}>
            {strings(stringKeys.form.cancel)}
          </Button>
          <SubmitButton isFetching={isEscalating} onClick={() => escalateAlert(alertId)}>
            {strings(stringKeys.alerts.assess.alert.escalate)}
          </SubmitButton>
        </FormActions>
      </DialogContent>
    </Dialog>
  );
}
