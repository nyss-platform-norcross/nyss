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
import { useSelector } from "react-redux";
import { DatePicker } from "../forms/DatePicker";
import AutocompleteTextInputField from "../forms/AutocompleteTextInputField";


export const SendReportDialog = ({ close, props, sendReport }) => {
  const [form, setForm] = useState(null);
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('xs'))
  dayjs.extend(utc);

  const dataCollectors = useSelector(state => state.reports.sendReport.dataCollectors.map(dc => ({ title: `${dc.name} / ${dc.phoneNumber}` })));
  const [date, setDate] = useState(dayjs().format('YYYY-MM-DD'));
  const isSending = useSelector(state => state.reports.formSaving);

  useEffect(() => {
    const fields = {
      dataCollector: '',
      message: '',
      time: dayjs().format('HH:mm')
    };

    const validation = {
      dataCollector: [validators.required],
      message: [validators.required],
      time: [validators.required, validators.time]
    };

    setForm(createForm(fields, validation));
  }, []);

  const handleDateChange = date => {
    setDate(date.format('YYYY-MM-DD'));
  }

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    sendReport({
      sender: values.dataCollector.split('/')[1].trim(),
      text: values.message,
      timestamp: dayjs(`${date} ${values.time}`).utc().format('YYYYMMDDHHmmss')
    });

    close();
  };

  if (!form) {
    return <Loading />;
  }

  return (
    <Fragment>
      <Dialog open={true} onClose={close} onClick={e => e.stopPropagation()} fullScreen={fullScreen}>
        <DialogTitle id="form-dialog-title">{strings(stringKeys.reports.sendReport.sendReport)}</DialogTitle>
        <DialogContent>
          <Form onSubmit={handleSubmit}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <AutocompleteTextInputField
                  label={strings(stringKeys.reports.sendReport.dataCollector)}
                  field={form.fields.dataCollector}
                  options={dataCollectors}
                  allowAddingValue={false}
                  autoSelect
                  name="dataCollectors"
                />
              </Grid>

              <Grid item xs={6}>
                <DatePicker
                  label={strings(stringKeys.reports.sendReport.dateOfReport)}
                  fullWidth
                  onChange={handleDateChange}
                  value={date}
                />
              </Grid>

              <Grid item xs={6}>
                <TextInputField
                  label={strings(stringKeys.reports.sendReport.timeOfReport)}
                  type="time"
                  name="time"
                  field={form.fields.time}
                  pattern="[0-9]{2}:[0-9]{2}"
                />
              </Grid>

              <Grid item xs={12}>
                <TextInputField
                  className={styles.fullWidth}
                  label={strings(stringKeys.reports.sendReport.message)}
                  name="message"
                  field={form.fields.message}
                />
              </Grid>
            </Grid>

            <FormActions>
              <Button onClick={close}>
                {strings(stringKeys.form.cancel)}
              </Button>
              <SubmitButton isFetching={isSending}>{strings(stringKeys.reports.sendReport.sendReport)}</SubmitButton>
            </FormActions>
          </Form>
        </DialogContent>
      </Dialog>
    </Fragment>
  );
}
