import React from 'react';
import DialogTitle from '@material-ui/core/DialogTitle';
import Dialog from '@material-ui/core/Dialog';
import { strings, stringKeys } from "../../strings";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../forms/submitButton/SubmitButton";
import Button from "@material-ui/core/Button";
import DialogContent from "@material-ui/core/DialogContent";
import useMediaQuery from '@material-ui/core/useMediaQuery';
import { useTheme } from "@material-ui/core";
import Typography from '@material-ui/core/Typography';

export const ConfirmationDialog = ({ isOpened, isFetching, close, submit, titlteText, contentText }) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('xs'));
  
  return (
    <Dialog onClose={close} open={isOpened} fullScreen={fullScreen}>
      <DialogTitle>
        {titlteText}
      </DialogTitle>
      <DialogContent>

        <Typography variant="body1">
        {contentText }
        </Typography>

        <FormActions>
          <Button onClick={close}>
            {strings(stringKeys.form.cancel)}
          </Button>
          <SubmitButton isFetching={isFetching} onClick={submit}>
          {strings(stringKeys.form.confirm)}
          </SubmitButton>
        </FormActions>
      </DialogContent>
    </Dialog>
  );
}
