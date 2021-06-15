import React, { useState } from "react";
import styles from "./CreateAlertEventDialog.module.scss";
import { Button, Dialog, DialogContent, DialogTitle, Grid, MenuItem, useMediaQuery, useTheme } from "@material-ui/core";
import { useSelector } from "react-redux";
import { createForm, useCustomErrors } from "../../../utils/forms";
import { Loading } from "../../common/loading/Loading";
import { stringKeys, strings } from "../../../strings";
import Form from "../../forms/form/Form";
import TextInputField from "../../forms/TextInputField";
import FormActions from "../../forms/formActions/FormActions";
import SubmitButton from "../../forms/submitButton/SubmitButton";
import * as alertEventsActions from "../logic/alertEventsActions";
import { connect } from "react-redux";

const EditAlertEventDialogComponent = ({ close, alertId, alertEventLogId, edit, text, ...props }) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('xs'));
  const isSaving = useSelector(state => state.alertEvents.formSaving)

  const [form] = useState(() => {
    const fields = {
      text: text
    };

    return createForm(fields);
  });

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    // const values = form.getValues();

    edit(alertId, alertEventLogId, form.fields.text.value);
    close();
  };

  // useCustomErrors(form, props.error);

  if (props.isFetching) {
    return <Loading />;
  }

  // if (!form || !props.data) {
  //   return null;
  // }

  return (
    <Dialog open={true} onClose={close} onClick={e => e.stopPropagation()} fullScreen={fullScreen}>

      <DialogTitle id="form-dialog-title">
        {strings(stringKeys.alerts.logs.edit)}
      </DialogTitle>

      <DialogContent>
        <Form onSubmit={handleSubmit} fullWidth>
          <Grid container spacing={2}>

            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.alerts.logs.form.text)}
                className={styles.fullWidth}
                type="text"
                name="text"
                field={form.fields.text}
                inputmode="text"
                multiline
              />
            </Grid>

          </Grid>

          <FormActions>
            <Button onClick={close}>
              {strings(stringKeys.form.cancel)}
            </Button>
            <SubmitButton isFetching={isSaving}>
              {strings(stringKeys.alerts.logs.edit)}
            </SubmitButton>
          </FormActions>

        </Form>
      </DialogContent>

    </Dialog>
  );
}

const mapStateToProps = (state, ownProps) => ({
});

const mapDispatchToProps = {
  edit: alertEventsActions.edit.invoke
};

export const EditAlertEventDialog = connect(mapStateToProps, mapDispatchToProps)(EditAlertEventDialogComponent)

