import styles from './DataCollectorsPerformanceColumnFilters.module.scss';
import React, { useState, useEffect } from 'react';
import Popover from "@material-ui/core/Popover";
import Grid from "@material-ui/core/Grid";
import Form from '../forms/form/Form';
import CheckboxWithIcon from '../forms/CheckboxWithIconField';
import { createForm } from '../../utils/forms';
import { DataCollectorStatusIcon } from '../common/icon/DataCollectorStatusIcon';
import { getIconFromStatus } from './logic/dataCollectorsService';
import { performanceStatus } from './logic/dataCollectorsConstants';

export const DataCollectorsPerformanceColumnFilters = ({ filters, anchorEl, open, onClose }) => {
  const [form, setForm] = useState(null);

  useEffect(() => {
    if (!filters) {
      return;
    }

    const fields = {
      reportingCorrectly: filters.reportingCorrectly,
      reportingWithErrors: filters.reportingWithErrors,
      notReporting: filters.notReporting
    };

    setForm(createForm(fields));
  }, [filters]);



  const handleClose = () => {
    onClose({
      reportingCorrectly: form.fields.reportingCorrectly.value,
      reportingWithErrors: form.fields.reportingWithErrors.value,
      notReporting: form.fields.notReporting.value
    });
  }

  return !!form && (
    <Popover
      open={open}
      onClose={handleClose}
      anchorEl={anchorEl}
      anchorOrigin={{
        vertical: 'bottom',
        horizontal: 'center',
      }}
      transformOrigin={{
        vertical: 'top',
        horizontal: 'center',
      }}
    >
      <Form className={styles.filterContainer}>
        <Grid item xs={12}>
          <CheckboxWithIcon
            labelicon={
              <DataCollectorStatusIcon status={performanceStatus.reportingCorrectly} icon={getIconFromStatus(performanceStatus.reportingCorrectly)} />
            }
            field={form.fields.reportingCorrectly}
          />
        </Grid>
        <Grid item xs={12}>
          <CheckboxWithIcon
            labelicon={
              <DataCollectorStatusIcon status={performanceStatus.reportingWithErrors} icon={getIconFromStatus(performanceStatus.reportingWithErrors)} />
            }
            field={form.fields.reportingWithErrors}
          />
        </Grid>
        <Grid item xs={12}>
          <CheckboxWithIcon
            labelicon={
              <DataCollectorStatusIcon status={performanceStatus.notReporting} icon={getIconFromStatus(performanceStatus.notReporting)} />
            }
            field={form.fields.notReporting}
          />
        </Grid>
      </Form>
    </Popover>
  )
}