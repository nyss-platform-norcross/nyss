import styles from "./DataCollectorsPerformanceMapFilters.module.scss"
import { useState } from 'react';
import { Grid } from '@material-ui/core';
import { strings, stringKeys } from "../../../strings";
import { DatePicker } from "../../forms/DatePicker";
import { convertToLocalDate, convertToUtc } from "../../../utils/date";

export const DataCollectorsPerformanceMapFilters = ({ filters, onChange }) => {
  const [value, setValue] = useState(filters);

  const handleChange = (change) => {
    const newValue = {
      ...value,
      startDate: convertToUtc(change.startDate),
      endDate: convertToUtc(change.endDate)
    }

    setValue(newValue);
    onChange(newValue);
  };

  const handleDateFromChange = date =>
    handleChange({ startDate: date });

  const handleDateToChange = date =>
    handleChange({ endDate: date });

  return (
    <Grid container spacing={2} className={styles.filters}>
      <Grid item>
        <DatePicker
          onChange={handleDateFromChange}
          label={strings(stringKeys.dashboard.filters.startDate)}
          value={convertToLocalDate(value.startDate)}
        />
      </Grid>

      <Grid item>
        <DatePicker
          onChange={handleDateToChange}
          label={strings(stringKeys.dashboard.filters.endDate)}
          value={convertToLocalDate(value.endDate)}
        />
      </Grid>
    </Grid>
  );
}
