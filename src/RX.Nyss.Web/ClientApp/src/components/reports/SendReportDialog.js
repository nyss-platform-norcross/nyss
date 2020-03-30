import styles from "./ReportsEditPage.module.scss";

import React, { useEffect, useState, Fragment } from 'react';
import { validators, createForm } from '../../utils/forms';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import { useTheme, Grid, Button } from "@material-ui/core"
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import { Dialog } from "@material-ui/core";
import DialogContent from '@material-ui/core/DialogContent';
import DialogTitle from '@material-ui/core/DialogTitle';
import useMediaQuery from '@material-ui/core/useMediaQuery'


export const SendReportDialog = ({ close, props }) => {
  const [form, setForm] = useState(null);
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('xs'))
  dayjs.extend(utc);

  useEffect(() => {
    const fields = {
      phoneNumber: "",
      message: "",
      apiKey: ""
    };

    const validation = {
      phoneNumber: [validators.required, validators.phoneNumber],
      message: [validators.required],
      apiKey: [validators.required]
    };

    setForm(createForm(fields, validation));
  }, [props.data, props.match]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    props.sendReport({
      sender: values.phoneNumber,
      text: values.message,
      timestamp: dayjs.utc().format("YYYYMMDDHHmmss"),
      apiKey: values.apiKey
    });
  };

  if (!form) {
    return <Loading />;
  }

  return (
    <Fragment>
      <Dialog open={true} onClose={close} onClick={e => e.stopPropagation()} fullScreen={fullScreen}>
        <DialogTitle id="form-dialog-title">Send report</DialogTitle>
        <DialogContent style={{ width: 400 }}>
          <Form onSubmit={handleSubmit}>
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <TextInputField
                  className={styles.fullWidth}
                  label={strings(stringKeys.reports.sendReport.phoneNumber)}
                  name="phoneNumber"
                  field={form.fields.phoneNumber}
                />
              </Grid>
            </Grid>
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <TextInputField
                  className={styles.fullWidth}
                  label={strings(stringKeys.reports.sendReport.message)}
                  name="message"
                  field={form.fields.message}
                />
              </Grid>
            </Grid>
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <TextInputField
                  className={styles.fullWidth}
                  label={strings(stringKeys.reports.sendReport.apiKey)}
                  name="apiKey"
                  field={form.fields.apiKey}
                />
              </Grid>
            </Grid>

            <FormActions>
              <Button onClick={close}>
                {strings(stringKeys.form.cancel)}
              </Button>
              <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.reports.sendReport.sendReport)}</SubmitButton>
            </FormActions>
          </Form>
        </DialogContent>
      </Dialog>
    </Fragment>
  );
}
