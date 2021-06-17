import React, { useState } from "react";
import styles from "./CreateAlertEventDialog.module.scss";
import {
  Button,
  Dialog,
  DialogContent,
  Grid,
  useMediaQuery,
  useTheme
} from "@material-ui/core";
import { useSelector } from "react-redux";
import { createForm, validators } from "../../../utils/forms";
import { Loading } from "../../common/loading/Loading";
import { stringKeys, strings } from "../../../strings";
import Form from "../../forms/form/Form";
import TextInputField from "../../forms/TextInputField";
import FormActions from "../../forms/formActions/FormActions";
import SubmitButton from "../../forms/submitButton/SubmitButton";
import * as alertEventsActions from "../logic/alertEventsActions";
import { connect } from "react-redux";
import * as dayjs from "dayjs";
import Typography from "@material-ui/core/Typography";

const EditAlertEventDialogComponent = ({ close, edit, alertId, eventLogItem, formattedEventType, formattedEventSubtype, ...props }) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('xs'));
  const isSaving = useSelector(state => state.alertEvents.formSaving)

  const [form] = useState(() => {
    const fields = {
      text: eventLogItem.text
    };

      const validation = {

        text: [validators.maxLength(4000)]
      };
      return createForm(fields, validation)
  });

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };


    edit(alertId, eventLogItem.alertEventLogId, form.fields.text.value);
    close();
  };

  if (props.isFetching) {
    return <Loading/>;
  }

  return (
    <Dialog open={true} onClose={close} fullScreen={fullScreen}>

      <DialogContent>
        <Form onSubmit={handleSubmit} fullWidth>
          <Grid container spacing={2}>

            <Grid item xs={12}>
              <Typography variant="h6">
                {dayjs(eventLogItem.date).format("YYYY-MM-DD HH:mm")}
              </Typography>
              <Typography >
                {formattedEventType}
              </Typography>
              <Typography >
                {formattedEventSubtype}
              </Typography>
            </Grid>

            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.alerts.eventLog.form.comment)}
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
              {strings(stringKeys.alerts.eventLog.edit)}
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

