import styles from "./ReportsEditPage.module.scss";

import React, { useEffect, useState, useRef, Fragment } from 'react';
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
import * as http from "../../utils/http";
import CancelButton from "../forms/cancelButton/CancelButton";

export const SendReportDialog = ({ close, showMessage }) => {
  const [form, setForm] = useState(null);
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('xs'))
  dayjs.extend(utc);
  const isFetching = useSelector(state => state.reports.formFetching);
  const dataCollectors = useSelector(state => state.reports.sendReport.dataCollectors);
  const formData = useSelector(state => state.reports.sendReport.formData);
  const [date, setDate] = useState(dayjs().format('YYYY-MM-DD'));
  const [isSending, setIsSending] = useState(false);
  const [gatewayModemsDisabled, setGatewayModemsDisabled] = useState(false);
  const abortController = useRef(new AbortController());
  const intervalHandler = useRef(0);
  const dcFieldSubscription = useRef(null);

  const canSelectModem = !!formData && formData.modems.length > 0;
  const dataCollectorsOptions = dataCollectors.map(dc => ({ title: `${dc.name} / ${dc.phoneNumber}`, id: dc.id }));

  const handleDateChange = date => {
    setDate(date.format('YYYY-MM-DD'));
  }

  function getReportStatus(timestamp, dataCollectorId) {
    return new Promise((resolve, reject) => {
      intervalHandler.current = setInterval(() => {
        http.get(`/api/report/status?timestamp=${timestamp}&dataCollectorId=${dataCollectorId}`, false, abortController.current.signal)
        .then(status => {
          if (!status) return;

          clearInterval(intervalHandler.current);
          resolve(status);
        })
        .catch(err => {
          clearInterval(intervalHandler.current);
          reject(err);
        });
      }, 3000);
    });
  }

  async function handleSubmit(e) {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    const dataCollector = dataCollectorsOptions.filter(dc => dc.title === values.dataCollector)[0];

    const data = {
      dataCollectorId: dataCollector.id,
      text: values.message,
      timestamp: dayjs(`${date} ${values.time}`).utc().format('YYYYMMDDHHmmss'),
      modemId: !!values.gatewayModemId ? parseInt(values.gatewayModemId) : null,
      utcOffset: getUtcOffset()
    };

    try {
      setIsSending(true);
      await http.post("/api/report/sendReport", data, false, abortController.current.signal);
      const status = await getReportStatus(data.timestamp, data.dataCollectorId);

      showMessage(status.feedbackMessage ? status.feedbackMessage : stringKeys.reports.sendReport.success);

      setIsSending(false);
      close();
    } catch (error) {
      setIsSending(false);

      if (error.name === 'AbortError') {
        close();
        return;
      };

      showMessage?.(error.message);
    }
  };

  function onClose() {
    if (isSending) {
      abortController.current.abort();

      if (intervalHandler.current > 0) {
        clearInterval(intervalHandler.current);
        close();
      }

      return;
    }

    close();
  }

  function onDataCollectorChange({ newValue }) {
    const dcOption = dataCollectorsOptions.find(dc => dc.title === newValue);
    const dc = dataCollectors.find(dc => dc.id === dcOption?.id);

    if (dc?.phoneNumber && canSelectModem) {
      setGatewayModemsDisabled(false);
      form.fields.gatewayModemId.setValidators([validators.required]);
    } else {
      form.fields.gatewayModemId.setValidators([]);
      form.fields.gatewayModemId.update("");
      setGatewayModemsDisabled(true);
    }
  }

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
      gatewayModemId: [],
      message: [validators.required],
      time: [validators.required, validators.time, validators.timeNotInFuture],
    };

    setForm(createForm(fields, validation));
  }, [canSelectModem, formData]);


  useEffect(() => {
    if (!form) return;

    dcFieldSubscription.current = form.fields.dataCollector.subscribe(onDataCollectorChange);

    return () => {
      dcFieldSubscription.current?.unsubscribe();
    }
  }, [form]);

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
                  options={dataCollectorsOptions}
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
                    disabled={gatewayModemsDisabled}
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
              <CancelButton variant={"contained"} onClick={onClose}>
                {strings(stringKeys.form.cancel)}
              </CancelButton>
              <SubmitButton isFetching={isSending}>{strings(stringKeys.reports.sendReport.sendReport)}</SubmitButton>
            </FormActions>
          </Form>
        </DialogContent>
      </Dialog>
    </Fragment>
  );
}
