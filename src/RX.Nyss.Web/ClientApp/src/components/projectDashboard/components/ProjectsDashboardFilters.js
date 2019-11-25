import styles from "./ProjectsDashboardFilters.module.scss";

import React, { useState } from 'react';
import Grid from '@material-ui/core/Grid';
import TextField from "@material-ui/core/TextField";
import MenuItem from "@material-ui/core/MenuItem";
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import { DatePicker } from "../../forms/DatePicker";
import { AreaFilter } from "../../common/filters/AreaFilter";

export const ProjectsDashboardFilters = ({ nationalSocietyId, healthRisks, onChange }) => {
  const [value, setValue] = useState({
    healthRisk: [],
    area: null,
    dateFrom: new Date(),
    dateTo: new Date()
  });

  const [selectedArea, setSelectedArea] = useState(null);

  const updateValue = (change) => {
    const newValue = {
      ...value,
      ...change
    }

    setValue(newValue);
    return newValue;
  };

  const handleAreaChange = (item) => {
    setSelectedArea(item);
    onChange(updateValue({ area: { type: item.type, id: item.id } }))
  }

  const handleHealthRiskChange = event =>
    onChange(updateValue({ healthRisk: event.target.value ? event.target.value : null }))

  const handleDateFromChange = date =>
    onChange(updateValue({ dateFrom: date["$d"] }))

  const handleDateToChange = date =>
    onChange(updateValue({ dateTo: date["$d"] }))

  return (
    <Card>
      <CardContent>
        <Grid container spacing={3} className={styles.filters}>
          <Grid item>
            <DatePicker
              className={styles.filterDate}
              onChange={handleDateFromChange}
              label="Date from"
              value={value.dateFrom}
            />
          </Grid>

          <Grid item>
            <DatePicker
              className={styles.filterDate}
              onChange={handleDateToChange}
              label="Date to"
              value={value.dateTo}
            />
          </Grid>

          <Grid item>
            <AreaFilter
              nationalSocietyId={nationalSocietyId}
              selectedItem={selectedArea}
              onChange={handleAreaChange}
            />
          </Grid>

          <Grid item>
            <TextField
              id="standard-select-currency"
              select
              label="Health risk"
              onChange={handleHealthRiskChange}
              value={value.healthRisk}
              className={styles.filterItem}
              InputLabelProps={{ shrink: true }}
            >
              <MenuItem value={""}>(all)</MenuItem>

              {healthRisks.map(healthRisk => (
                <MenuItem key={`filter_healthRisk_${healthRisk.id}`} value={healthRisk.id}>
                  {healthRisk.name}
                </MenuItem>
              ))}
            </TextField>
          </Grid>
        </Grid>
      </CardContent>
    </Card>
  );
}
