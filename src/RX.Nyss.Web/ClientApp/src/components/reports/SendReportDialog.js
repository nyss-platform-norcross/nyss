import styles from "./ReportsEditPage.module.scss";

import React, { useEffect, useState, Fragment } from 'react';
import { validators, createForm } from '../../utils/forms';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import { strings, stringKeys } from '../../strings';
import { useTheme, Grid, Button, MenuItem, LinearProgress } from "@material-ui/core"
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import { Dialog, DialogContent, DialogTitle, useMediaQuery } from "@material-ui/core";
import { useSelector } from "react-redux";
import { DatePicker } from "../forms/DatePicker";
import AutocompleteTextInputField from "../forms/AutocompleteTextInputField";
import SelectField from "../forms/SelectField";
import { getUtcOffset } from "../../utils/date";


export const SendReportDialog = ({ close, sendReport }) => {
  const [form, setForm] = useState(null);
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('xs'))
  dayjs.extend(utc);
  const isFetching = useSelector(state => state.reports.formFetching);
  const dataCollectors = useSelector(state => state.reports.sendReport.dataCollectors.map(dc => ({ title: `${dc.name} / ${dc.phoneNumber}`, id: dc.id })));
  const formData = useSelector(state => state.reports.sendReport.formData);
  const [date, setDate] = useState(dayjs().format('YYYY-MM-DD'));
  const isSending = useSelector(state => state.reports.formSaving);

  const canSelectModem = !!formData && formData.modems.length > 0;

  useEffect(() => {
    if (!formData) {
      return null;
    }

    const fields = {
      dataCollector: null,
      gatewayModemId: !!formData.currentUserModemId ? formData.currentUserModemId.toString() : '',
      message: '',
      time: dayjs().format('HH:mm')
    };

    const validation = {
      dataCollector: [validators.required],
      gatewayModemId: [validators.requiredWhen(_ => canSelectModem)],
      message: [validators.required],
      time: [validators.required, validators.time]
    };

    setForm(createForm(fields, validation));
  }, [canSelectModem, formData]);

  const handleDateChange = date => {
    setDate(date.format('YYYY-MM-DD'));
  }

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    const dataCollector = dataCollectors.filter(dc => dc.title === values.dataCollector)[0];
    sendReport({
      dataCollectorId: dataCollector.id,
      text: values.message,
      timestamp: dayjs(`${date} ${values.time}`).utc().format('YYYYMMDDHHmmss'),
      modemId: !!values.gatewayModemId ? parseInt(values.gatewayModemId) : null,
      utcOffset: getUtcOffset()
    });

    close();
  };

  return !!form && (
    <Fragment>
      <Dialog open={true} onClose={close} onClick={e => e.stopPropagation()} fullScreen={fullScreen}>
        {isFetching && <LinearProgress />}
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

              {canSelectModem && (
                <Grid item xs={12}>
                  <SelectField
                    label={strings(stringKeys.reports.sendReport.modem)}
                    field={form.fields.gatewayModemId}
                    name="gatewayModems"
                  >
                    {formData.modems.map(modem => (
                      <MenuItem key={`gatewayModem_${modem.id}`} value={modem.id.toString()}>
                        {modem.name}
                      </MenuItem>
                    ))}
                  </SelectField>
                </Grid>
              )}

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
                  inputMode={"tel"}
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
