import styles from "./ConfirmationDialog.module.scss";

import React from "react";
import { strings, stringKeys } from "../../../strings";
import FormActions from "../../forms/formActions/FormActions";
import SubmitButton from "../../common/buttons/submitButton/SubmitButton";
import CancelButton from "../../common/buttons/cancelButton/CancelButton";
import { withAccessRestriction } from "../hasAccess/HasAccess";
import {
  useTheme,
  DialogTitle,
  Dialog,
  DialogContent,
  useMediaQuery,
  Typography,
} from "@material-ui/core";

export const ConfirmationDialogComponent = ({
  children,
  isOpened,
  isFetching,
  close,
  submit,
  titleText,
  contentText,
}) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down("xs"));

  return (
    <Dialog onClose={close} open={isOpened} fullScreen={fullScreen}>
      <DialogTitle>{titleText}</DialogTitle>
      <DialogContent className={styles.content}>
        {contentText && <Typography variant="body1">{contentText}</Typography>}
        {children}
        <FormActions>
          <CancelButton onClick={close}>
            {strings(stringKeys.form.cancel)}
          </CancelButton>
          <SubmitButton isFetching={isFetching} onClick={submit}>
            {strings(stringKeys.form.confirm)}
          </SubmitButton>
        </FormActions>
      </DialogContent>
    </Dialog>
  );
};

export const ConfirmationDialog = withAccessRestriction(
  ConfirmationDialogComponent,
);
