import React, { useEffect, useState, Fragment } from "react";
import styles from "./CreateAlertEventDialog.module.scss";
import {
  Dialog,
  DialogContent,
  Grid,
  useMediaQuery,
  useTheme,
} from "@material-ui/core";
import { useSelector } from "react-redux";
import { createForm, validators } from "../../../utils/forms";
import { Loading } from "../../common/loading/Loading";
import { stringKeys, strings } from "../../../strings";
import Form from "../../forms/form/Form";
import TextInputField from "../../forms/TextInputField";
import FormActions from "../../forms/formActions/FormActions";
import SubmitButton from "../../common/buttons/submitButton/SubmitButton";
import CancelButton from "../../common/buttons/cancelButton/CancelButton";
import * as dayjs from "dayjs";
import Typography from "@material-ui/core/Typography";

export const EditAlertEventDialog = ({
  open,
  close,
  edit,
  alertId,
  eventLogItem,
  formattedEventType,
  formattedEventSubtype,
  ...props
}) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down("xs"));
  const isSaving = useSelector((state) => state.alertEvents.formSaving);
  const [form, setForm] = useState(null);

  useEffect(() => {
    if (!eventLogItem) return;

    const fields = {
      text: eventLogItem.text,
    };

    const validation = {
      text: [validators.maxLength(4000)],
    };

    setForm(createForm(fields, validation));
  }, [eventLogItem]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    }

    edit(alertId, eventLogItem.alertEventLogId, form.fields.text.value);
    close();
  };

  if (props.isFetching) {
    return <Loading />;
  }

  return (
    !!form && (
      <Dialog open={open} onClose={close} fullScreen={fullScreen}>
        <DialogContent>
          <Form onSubmit={handleSubmit} fullWidth>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <Typography variant="h6">
                  {strings(stringKeys.alerts.eventLog.form.dateOfEvent)}
                </Typography>
                <Typography variant="body1">
                  {dayjs(eventLogItem.date).format("YYYY-MM-DD HH:mm")}
                </Typography>

                <Typography variant="h6">
                  {strings(stringKeys.alerts.eventLog.list.type)}
                </Typography>
                <Typography variant="body1">{formattedEventType}</Typography>

                {!!formattedEventSubtype && (
                  <Fragment>
                    <Typography variant="h6">
                      {strings(stringKeys.alerts.eventLog.list.subtype)}
                    </Typography>
                    <Typography variant="body1">
                      {formattedEventSubtype}
                    </Typography>
                  </Fragment>
                )}
              </Grid>

              <Grid item xs={12}>
                <TextInputField
                  label={strings(stringKeys.alerts.eventLog.form.comment)}
                  className={`${styles.fullWidth} ${styles.bold}`}
                  labelClassName={styles.bold}
                  type="text"
                  name="text"
                  field={form.fields.text}
                  inputmode="text"
                  multiline
                />
              </Grid>
            </Grid>

            <FormActions>
              <CancelButton onClick={close}>
                {strings(stringKeys.form.cancel)}
              </CancelButton>
              <SubmitButton isFetching={isSaving}>
                {strings(stringKeys.alerts.eventLog.edit)}
              </SubmitButton>
            </FormActions>
          </Form>
        </DialogContent>
      </Dialog>
    )
  );
};
