import React from 'react';
import DialogTitle from '@material-ui/core/DialogTitle';
import Dialog from '@material-ui/core/Dialog';
import { strings, stringKeys } from "../../../strings";
import FormActions from "../../forms/formActions/FormActions";
import SubmitButton from "../../forms/submitButton/SubmitButton";
import Button from "@material-ui/core/Button";
import DialogContent from "@material-ui/core/DialogContent";
import useMediaQuery from '@material-ui/core/useMediaQuery';
import { useTheme, Grid } from "@material-ui/core";
import Typography from '@material-ui/core/Typography';
import WarningIcon from '@material-ui/icons/Warning';

export const AlertsEscalationWithoutNotificationDialog = ({ isOpened, close, alertId, isEscalating, escalateAlert }) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('xs'));

  return (
    <Dialog onClose={close} open={isOpened} fullScreen={fullScreen}>
      <DialogTitle>{strings(stringKeys.alerts.assess.alert.escalateWithoutNotificationDialogTitle)}</DialogTitle>
      <DialogContent>

        <Grid container spacing={2} style={{ alignItems: 'center'}}>
          <Grid item xs={2}>
            <WarningIcon color="primary" style={{ fontSize: 40, marginLeft: '10px' }} />
          </Grid>
          <Grid item xs={10}>
            <Typography variant="body1">
              {strings(stringKeys.alerts.assess.alert.escalateWithoutNotificationConfirmation)}
            </Typography>
          </Grid>
        </Grid>

        <FormActions>
          <Button onClick={close}>
            {strings(stringKeys.form.cancel)}
          </Button>
          <SubmitButton isFetching={isEscalating} onClick={() => escalateAlert(alertId, false)}>
            {strings(stringKeys.alerts.assess.alert.escalate)}
          </SubmitButton>
        </FormActions>
      </DialogContent>
    </Dialog>
  );
}
