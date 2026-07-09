import React, { Fragment, useState, useEffect } from "react";
import { stringKeys, strings } from "../../../strings";
import SubmitButton from "../../common/buttons/submitButton/SubmitButton";
import { AlertsEscalationDialog } from "./AlertsEscalationDialog";
import { assessmentStatus } from "../logic/alertsConstants";
import { AlertsCloseDialog } from "./AlertsCloseDialog";
import { AlertsEscalationWithoutNotificationDialog } from "./AlertsEscalationWithoutNotificationDialog";
import CheckboxField from "../../forms/CheckboxField";
import { validators, createForm } from "../../../utils/forms";
import { Button, CircularProgress, Grid, Snackbar, useMediaQuery} from "@material-ui/core";
import { Alert, AlertTitle } from "@material-ui/lab";
import { trackEvent } from "../../../utils/appInsightsHelper";
import GoBackToButton from "../../common/buttons/goBackToButton/GoBackToButton";

export const AlertsAssessmentActions = ({
  projectId,
  alertId,
  alertAssessmentStatus,
  isNationalSocietyEidsrEnabled,
  validateEidsrResult,
  hasAccess,
  listState,
  ...props
}) => {
  const [escalationDialogOpened, setEscalationDialogOpened] = useState(false);
  const [
    escalationWithoutNotificationDialogOpened,
    setEscalationWithoutNotificationDialogOpened,
  ] = useState(false);
  const [closeDialogOpened, setCloseDialogOpened] = useState(false);

  const [openValidationSnackbar, setOpenValidationSnackbar] =
    React.useState(false);

  const isSmallScreen = useMediaQuery(theme => theme.breakpoints.down("sm"));

  useEffect(() => {
    if (validateEidsrResult) {
      setOpenValidationSnackbar(true);
    }
  }, [validateEidsrResult]);

  const handleCloseValidationSnackbar = (event, reason) => {
    if (reason === "clickaway") {
      return;
    }

    setOpenValidationSnackbar(false);
  };

  const [form] = useState(() => {
    const fields = {
      comments: "",
      escalateWithoutNotification: false,
    };
    const validation = { comments: [validators.maxLength(500)] };
    return createForm(fields, validation);
  });

  const handleEscalateAlert = () => {
    if (form.fields.escalateWithoutNotification.value) {
      setEscalationWithoutNotificationDialogOpened(true);
    } else {
      props.fetchRecipients(alertId);
      setEscalationDialogOpened(true);
    }
  };

  const handleDismissAlert = (alertId) => {
    // Track dismissAlert event
    trackEvent("dismissAlert", { alertId: alertId });

    props.dismissAlert(alertId);
  };

  return (
    <Grid container spacing={2}>
      <Grid item xs={6}>
        {isNationalSocietyEidsrEnabled && (
          <>
            <div hidden="true">
              <Button
                disabled={props.isLoadingValidateEidsr}
                color="primary"
                onClick={() => props.validateEidsr(alertId)}
              >
                {" "}
                Validate Eidsr migration{" "}
              </Button>
            </div>
            {props.isLoadingValidateEidsr && <CircularProgress size={12} />}
            <Snackbar open={openValidationSnackbar}>
              <Alert
                style={{ backgroundColor: "#eee" }}
                onClose={handleCloseValidationSnackbar}
                severity="info"
              >
                <AlertTitle>
                  {strings(
                    stringKeys.alerts.assess.alert.eidsrValidation.title,
                  )}
                </AlertTitle>
                <ul>
                  <li>
                    {strings(
                      stringKeys.alerts.assess.alert.eidsrValidation
                        .connection,
                    )}
                    :{" "}
                    {validateEidsrResult?.isEidsrApiConnectionRunning
                      ? strings(stringKeys.common.boolean.true)
                      : strings(stringKeys.common.boolean.false)}
                  </li>
                  <li>
                    {strings(
                      stringKeys.alerts.assess.alert.eidsrValidation
                        .integrationValid,
                    )}
                    :{" "}
                    {validateEidsrResult?.isIntegrationConfigValid
                      ? strings(stringKeys.common.boolean.true)
                      : strings(stringKeys.common.boolean.false)}{" "}
                  </li>
                  <li>
                    {strings(
                      stringKeys.alerts.assess.alert.eidsrValidation
                        .validReportsCount,
                    )}
                    :{" "}
                    {validateEidsrResult?.areReportsValidCount
                      ? validateEidsrResult?.areReportsValidCount
                      : 0}
                  </li>
                </ul>
                <p>
                  {strings(
                    stringKeys.alerts.assess.alert.eidsrValidation.tip,
                  )}
                </p>
              </Alert>
            </Snackbar>
          </>
        )}
      </Grid>
      <Grid item container justifyContent={isSmallScreen ? "" : "space-between"} spacing={1}>
        <Grid item xs={6} style={{ order: isSmallScreen ? 2 : 1 }}>
          <GoBackToButton onClick={() => props.goToList(projectId, listState)} >
            {strings(stringKeys.alerts.assess.goBack)}
          </GoBackToButton>
        </Grid>
        {!props.isPendingAlertState && (
          <Fragment>
            {alertAssessmentStatus === assessmentStatus.toEscalate &&
              hasAccess && (
                <Fragment>
                  <AlertsEscalationDialog
                    alertId={alertId}
                    escalateAlert={props.escalateAlert}
                    isEscalating={props.isEscalating}
                    isFetchingRecipients={props.isFetchingRecipients}
                    notificationEmails={props.notificationEmails}
                    notificationPhoneNumbers={
                      props.notificationPhoneNumbers
                    }
                    isOpened={escalationDialogOpened}
                    close={() => setEscalationDialogOpened(false)}
                  />

                  <AlertsEscalationWithoutNotificationDialog
                    alertId={alertId}
                    escalateAlert={props.escalateAlert}
                    isEscalating={props.isEscalating}
                    isOpened={escalationWithoutNotificationDialogOpened}
                    close={() =>
                      setEscalationWithoutNotificationDialogOpened(false)
                    }
                  />
                <Grid item xs={isSmallScreen ? 12 : 6} container alignItems="center" justifyContent={isSmallScreen ? "space-between" : "flex-end"} style={{ order: isSmallScreen ? 1 : 2 }}>
                  <Grid item style={{ marginRight: 10 }}>
                    <CheckboxField
                      name="escalateWithoutNotification"
                      label={strings(
                        stringKeys.alerts.assess.alert
                        .escalateWithoutNotification,
                        )}
                        field={form.fields.escalateWithoutNotification}
                    />
                  </Grid>
                  <Grid item>
                    <SubmitButton onClick={handleEscalateAlert}>
                      {strings(stringKeys.alerts.assess.alert.escalate)}
                    </SubmitButton>
                  </Grid>
                </Grid>
                </Fragment>
              )}

            {(alertAssessmentStatus === assessmentStatus.toDismiss ||
              alertAssessmentStatus === assessmentStatus.rejected) &&
              hasAccess && (
                <SubmitButton
                  isFetching={props.isDismissing}
                  onClick={() => handleDismissAlert(alertId)}
                >
                  {strings(stringKeys.alerts.assess.alert.dismiss)}
                </SubmitButton>
            )}
            {alertAssessmentStatus === assessmentStatus.escalated &&
                hasAccess && (
                  <Fragment>
                    <AlertsCloseDialog
                      alertId={alertId}
                      closeAlert={props.closeAlert}
                      isClosing={props.isClosing}
                      isOpened={closeDialogOpened}
                      close={() => setCloseDialogOpened(false)}
                    />
                  <Grid container item xs={6} justifyContent={"flex-end"}  style={{ order: isSmallScreen ? 2 : 1 }}>
                    <SubmitButton onClick={() => setCloseDialogOpened(true)}>
                      {strings(stringKeys.alerts.assess.alert.close)}
                    </SubmitButton>
                  </Grid>
                  </Fragment>
              )}
          </Fragment>
        )}
      </Grid>
    </Grid>
  );
};
