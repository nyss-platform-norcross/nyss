import React from "react";
import { strings, stringKeys } from "../../../strings";
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
import FormActions from "../../forms/formActions/FormActions";
import { trackEvent } from "../../../utils/appInsightsHelper";

export const AlertsCloseDialog = ({
  isOpened,
  close,
  alertId,
  isClosing,
  closeAlert,
}) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down("xs"));

  const handleCloseAlert = (alertId) => {
    // Track closeAlert event
    trackEvent("closeAlert", {
      alertId,
    });

    closeAlert(alertId);
  };

  return (
    <Dialog onClose={close} open={isOpened} fullScreen={fullScreen}>
      <DialogTitle>
        {strings(stringKeys.alerts.assess.alert.closeConfirmation)}
      </DialogTitle>
      <DialogContent>
        <Typography variant="body1">
          {strings(stringKeys.alerts.assess.alert.closeDescription)}
        </Typography>
        <FormActions>
          <CancelButton onClick={close}>
            {strings(stringKeys.form.cancel)}
          </CancelButton>
          <SubmitButton
            isFetching={isClosing}
            onClick={() => handleCloseAlert(alertId)}
          >
            {strings(stringKeys.alerts.assess.alert.close)}
          </SubmitButton>
        </FormActions>
      </DialogContent>
    </Dialog>
  );
};
