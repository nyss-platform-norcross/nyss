import React from "react";
import { strings, stringKeys } from "../../../strings";
import DisplayField from "../../forms/DisplayField";
import FormActions from "../../forms/formActions/FormActions";
import SubmitButton from "../../common/buttons/submitButton/SubmitButton";
import CancelButton from "../../common/buttons/cancelButton/CancelButton";
import {
  useTheme,
  DialogTitle,
  Dialog,
  DialogContent,
  useMediaQuery,
  Typography,
} from "@material-ui/core";
import { trackEvent } from "../../../utils/appInsightsHelper";

export const AlertsEscalationDialog = ({
  isOpened,
  close,
  alertId,
  isEscalating,
  isFetchingRecipients,
  escalateAlert,
  notificationEmails,
  notificationPhoneNumbers,
}) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down("xs"));

  const handleEscalateAlert = (alertId, sendNotification) => {
    // track escalateAlert event
    trackEvent("escalateAlert", {
      alertId: alertId,
      sendNotification: sendNotification,
    });

    escalateAlert(alertId, sendNotification);
  };

  return (
    <Dialog onClose={close} open={isOpened} fullScreen={fullScreen}>
      <DialogTitle>
        {strings(stringKeys.alerts.assess.alert.escalateConfirmation)}
      </DialogTitle>
      <DialogContent>
        {notificationEmails.length > 0 && (
          <DisplayField
            label={strings(
              stringKeys.alerts.assess.alert.escalateNotificationEmails,
            )}
          >
            {notificationEmails.map((email) => (
              <div key={`email_${email}`}>{email}</div>
            ))}
          </DisplayField>
        )}

        {notificationPhoneNumbers.length > 0 && (
          <DisplayField
            label={strings(
              stringKeys.alerts.assess.alert.escalateNotificationSmses,
            )}
          >
            {notificationPhoneNumbers.map((phoneNumber) => (
              <div key={`phone_${phoneNumber}`}>{phoneNumber}</div>
            ))}
          </DisplayField>
        )}

        <Typography variant="body1">
          {strings(
            stringKeys.alerts.assess.alert
              .escalateConfirmationInformDataCollectors,
          )}
        </Typography>

        <FormActions>
          <CancelButton onClick={close}>
            {strings(stringKeys.form.cancel)}
          </CancelButton>
          <SubmitButton
            isFetching={isEscalating}
            onClick={() => handleEscalateAlert(alertId, true)}
          >
            {strings(stringKeys.alerts.assess.alert.escalate)}
          </SubmitButton>
        </FormActions>
      </DialogContent>
    </Dialog>
  );
};
