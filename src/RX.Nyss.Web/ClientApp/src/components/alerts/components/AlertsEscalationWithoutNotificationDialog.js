import React from "react";
import { strings, stringKeys } from "../../../strings";
import FormActions from "../../forms/formActions/FormActions";
import SubmitButton from "../../common/buttons/submitButton/SubmitButton";
import {
  useTheme,
  Grid,
  DialogTitle,
  Dialog,
  DialogContent,
  useMediaQuery,
  Typography,
} from "@material-ui/core";
import WarningIcon from "@material-ui/icons/Warning";
import CancelButton from "../../common/buttons/cancelButton/CancelButton";
import { trackEvent } from "../../../utils/appInsightsHelper";

export const AlertsEscalationWithoutNotificationDialog = ({
  isOpened,
  close,
  alertId,
  isEscalating,
  escalateAlert,
}) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down("xs"));

  const handleEscalateAlert = (alertId, sendNotification) => {
    // Track escalateAlert event
    trackEvent("escalateAlert", {
      alertId: alertId,
      sendNotification: sendNotification,
    });

    escalateAlert(alertId, sendNotification);
  };

  return (
    <Dialog onClose={close} open={isOpened} fullScreen={fullScreen}>
      <DialogTitle>
        {strings(
          stringKeys.alerts.assess.alert.escalateWithoutNotificationDialogTitle,
        )}
      </DialogTitle>
      <DialogContent>
        <Grid container spacing={2} style={{ alignItems: "center" }}>
          <Grid item xs={2}>
            <WarningIcon
              color="primary"
              style={{ fontSize: 40, marginLeft: "10px" }}
            />
          </Grid>
          <Grid item xs={10}>
            <Typography variant="body1">
              {strings(
                stringKeys.alerts.assess.alert
                  .escalateWithoutNotificationConfirmation,
              )}
            </Typography>
          </Grid>
        </Grid>

        <FormActions>
          <CancelButton onClick={close}>
            {strings(stringKeys.form.cancel)}
          </CancelButton>
          <SubmitButton
            isFetching={isEscalating}
            onClick={() => handleEscalateAlert(alertId, false)}
          >
            {strings(stringKeys.alerts.assess.alert.escalate)}
          </SubmitButton>
        </FormActions>
      </DialogContent>
    </Dialog>
  );
};
