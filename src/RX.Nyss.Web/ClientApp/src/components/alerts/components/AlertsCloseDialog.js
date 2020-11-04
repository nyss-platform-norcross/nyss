import React, { useState } from 'react';
import DialogTitle from '@material-ui/core/DialogTitle';
import Dialog from '@material-ui/core/Dialog';
import { strings, stringKeys } from "../../../strings";
import FormActions from "../../forms/formActions/FormActions";
import SubmitButton from "../../forms/submitButton/SubmitButton";
import Button from "@material-ui/core/Button";
import DialogContent from "@material-ui/core/DialogContent";
import useMediaQuery from '@material-ui/core/useMediaQuery';
import { useTheme, Grid, FormControlLabel, Radio } from "@material-ui/core";
import { escalatedOutcomes } from '../logic/alertsConstants';
import { validators, createForm } from '../../../utils/forms';
import Form from '../../forms/form/Form';
import RadioGroupField from '../../forms/RadioGroupField';
import TextInputField from '../../forms/TextInputField';

export const AlertsCloseDialog = ({ isOpened, close, alertId, isClosing, closeAlert }) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('xs'));

  const [form] = useState(() => {
    const fields = {
      escalatedOutcome: escalatedOutcomes.dismissed,
      comments: ""
    };

    const validation = {
      escalatedOutcome: [validators.required],
      comments: [validators.maxLength(500), validators.requiredWhen(x => x.escalatedOutcome === escalatedOutcomes.other)]
    };
    return createForm(fields, validation);
  });

  const handleSubmit = (event) => {
    event.preventDefault();

    if (!form.isValid()) {
      return;
    }
    closeAlert(alertId, form.fields.comments.value, form.fields.escalatedOutcome.value);
  }

  return (
    <Dialog onClose={close} open={isOpened} fullScreen={fullScreen}>
      <DialogTitle>{strings(stringKeys.alerts.assess.alert.closeConfirmation)}</DialogTitle>
      <DialogContent>

        <Form onSubmit={handleSubmit} fullWidth>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <RadioGroupField
                name="escalatedOutcome"
                field={form.fields.escalatedOutcome} >
                {Object.keys(escalatedOutcomes).map(option => (
                  <FormControlLabel key={option} control={<Radio />} label={strings(stringKeys.alerts.assess.escalatedOutcomes[escalatedOutcomes[option]])} value={escalatedOutcomes[option]} />
                ))}
              </RadioGroupField>
            </Grid>

            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.alerts.assess.alert.closeComments)}
                name="comments"
                multiline
                field={form.fields.comments}
              />
            </Grid>
          </Grid>

          <FormActions>
            <Button onClick={close}>
              {strings(stringKeys.form.cancel)}
            </Button>
            <SubmitButton isFetching={isClosing}>
              {strings(stringKeys.alerts.assess.alert.close)}
            </SubmitButton>
          </FormActions>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
