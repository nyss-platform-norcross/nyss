import React, { useState, useEffect } from "react";
import DialogTitle from "@material-ui/core/DialogTitle";
import DialogContent from "@material-ui/core/DialogContent";
import Dialog from "@material-ui/core/Dialog";
import useMediaQuery from "@material-ui/core/useMediaQuery";
import {
  useTheme,
  Grid,
  Typography,
} from "@material-ui/core";
import { strings, stringKeys } from "../../strings";
import TextInputField from "../forms/TextInputField";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import { createForm, validators } from "../../utils/forms";
import styles from "./FeedbackDialog.module.scss";
import { sendFeedback } from '../app/logic/appActions';
import { useDispatch } from "react-redux";

export const FeedbackDialog = ({
  isOpened,
  close,
  isSending,
  result,
}) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down("xs"));
  const dispatch = useDispatch();
  const [form, setForm] = useState(() => {
    const fields = { message: "" };
    const validation = {
      message: [validators.maxLength(1000), validators.required],
    };

    return createForm(fields, validation);
  });
  const [hasSent, setHasSent] = useState(false);

  const resetForm = () => {
    setForm(() => {
      const fields = { message: "" };
      const validation = {
        message: [validators.maxLength(1000), validators.required],
      };

      return createForm(fields, validation);
    });
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.isValid()) {
      return;
    }

    dispatch(sendFeedback.invoke({
      message: form.fields.message.value,
    }));
  };

  const handleClose = () => {
    setHasSent(false);
    close();
  };

  useEffect(() => {
    if (result === "ok") {
      setHasSent(true);
      resetForm();
      setTimeout(handleClose, 1500);
    }
  }, [result]);

  if (!isOpened) {
    return null;
  }

  return (
    <Dialog onClose={handleClose} open={isOpened} fullScreen={fullScreen}>
      <DialogTitle>
        {!hasSent && strings(stringKeys.feedback.dialogTitle)}
      </DialogTitle>
      <DialogContent className={styles.feedbackContent}>
        {hasSent ? (
          <Typography variant="h3" className={styles.thankYouMessage}>
            {strings(stringKeys.feedback.thankYou)}
          </Typography>
        ) : (
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.feedback.dialogDescription)}
                name="feedback"
                multiline
                rows="4"
                disabled={isSending}
                field={form.fields.message}
              />
            </Grid>
            <Grid item xs={12} style={{ textAlign: "right" }}>
              <SubmitButton isFetching={isSending} onClick={handleSubmit}>
                {strings(stringKeys.feedback.submit)}
              </SubmitButton>
            </Grid>
          </Grid>
        )}
      </DialogContent>
    </Dialog>
  );
};
