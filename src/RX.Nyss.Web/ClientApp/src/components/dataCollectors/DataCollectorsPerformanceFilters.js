import styles from "./DataCollectorsPerformanceFilters.module.scss"

import React, { useState } from 'react';
import Grid from '@material-ui/core/Grid';
import { strings, stringKeys } from "../../strings";
import { DatePicker } from "../forms/DatePicker";

export const DataCollectorsPerformanceFilters = ({ filters, onChange }) => {
  const [value, setValue] = useState(filters);

  const handleChange = (change) => {
    const newValue = {
      ...value,
      ...change
    }

    setValue(newValue);
    onChange(newValue);
  };

  const handleDateFromChange = date =>
    handleChange({ startDate: date.format('YYYY-MM-DD') });

  const handleDateToChange = date =>
    handleChange({ endDate: date.format('YYYY-MM-DD') });

  return (
    <Grid container spacing={3} className={styles.filters}>
      <Grid item>
        <DatePicker
          onChange={handleDateFromChange}
          label={strings(stringKeys.project.dashboard.filters.startDate)}
          value={value.startDate}
        />
      </Grid>

      <Grid item>
        <DatePicker
          onChange={handleDateToChange}
          label={strings(stringKeys.project.dashboard.filters.endDate)}
          value={value.endDate}
        />
      </Grid>
    </Grid>
  );
}
