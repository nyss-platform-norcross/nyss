import styles from './FeedbackDialog.module.scss';

import React, { useState } from 'react';
import DialogTitle from '@material-ui/core/DialogTitle';
import DialogContent from '@material-ui/core/DialogContent';
import Dialog from '@material-ui/core/Dialog';
import useMediaQuery from '@material-ui/core/useMediaQuery';
import { useTheme, Grid, FormControlLabel, Checkbox, Typography } from "@material-ui/core";
import { strings, stringKeys } from "../../strings";
import TextInputField from '../forms/TextInputField';
import SubmitButton from '../forms/submitButton/SubmitButton';
import { createForm, validators } from '../../utils/forms';

export const FeedbackDialog = ({ isOpened, close, isSending, sendFeedback }) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('xs'));
  const [form, setForm] = useState(() => {
    const fields = { message: "" }
    const validation = { message: [validators.maxLength(1000), validators.required] }

    return createForm(fields, validation);
  });
  const [includeContactDetails, setIncludeContactDetails] = useState(false);
  const [hasSent, setHasSent] = useState(false);

  if (!isOpened) {
    return null;
  }

  const handleIncludeContactDetails = (e) => {
    setIncludeContactDetails(e.target.checked);
  }

  const resetForm = () => {
    setForm(() => {
      const fields = { message: "" }
      const validation = { message: [validators.maxLength(1000), validators.required] }

      return createForm(fields, validation);
    });
    setIncludeContactDetails(false);
  }

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.isValid()) {
      return;
    }

    sendFeedback({
      message: form.fields.message.value,
      includeContactDetails: includeContactDetails
    });

    setHasSent(true);
    resetForm()
    setTimeout(handleClose, 1500);
  }

  const handleClose = () => {
    setHasSent(false);
    close();
  }

  return (
    <Dialog onClose={handleClose} open={isOpened} fullScreen={fullScreen}>
      <DialogTitle>{!hasSent && strings(stringKeys.feedback.dialogTitle)}</DialogTitle>
      <DialogContent className={styles.feedbackContent}>
        {hasSent
          ? <Typography variant="h3" className={styles.thankYouMessage}>{strings(stringKeys.feedback.thankYou)}</Typography>
          : (
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
              <Grid item xs={12}>
                <FormControlLabel
                  control={<Checkbox
                    checked={includeContactDetails ? true : false}
                    onChange={handleIncludeContactDetails}
                    color="primary"
                  />}
                  label={strings(stringKeys.feedback.privacyMessage)}
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
}