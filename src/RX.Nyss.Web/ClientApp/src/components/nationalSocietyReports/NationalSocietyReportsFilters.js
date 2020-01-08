import styles from "./NationalSocietyReportsFilters.module.scss";

import React, { useState } from 'react';
import Grid from '@material-ui/core/Grid';
import TextField from "@material-ui/core/TextField";
import MenuItem from "@material-ui/core/MenuItem";
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import FormControl from '@material-ui/core/FormControl';
import InputLabel from '@material-ui/core/InputLabel';
import Select from '@material-ui/core/Select';
import { FormControlLabel, Radio, RadioGroup } from '@material-ui/core';
import { AreaFilter } from "../common/filters/AreaFilter";
import { strings, stringKeys } from "../../strings";

export const NationalSocietyReportsFilters = ({ filters, nationalSocietyId, healthRisks, onChange }) => {
  const [value, setValue] = useState(filters);

  const [selectedArea, setSelectedArea] = useState(filters && filters.area);

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
    onChange(updateValue({ area: item ? { type: item.type, id: item.id, name: item.name } : null }))
  }

  const handleHealthRiskChange = event =>
    onChange(updateValue({ healthRiskId: event.target.value === 0 ? null : event.target.value }))

  const handleReportsTypeChange = event =>
    onChange(updateValue({ reportsType: event.target.value }))

  const handleIsTrainingChange = event =>
    onChange(updateValue({ isTraining: event.target.value === "true" }))

  const handleStatusChange = event =>
    onChange(updateValue({ status: event.target.value === "true" }))

  if (!value) {
    return null;
  }

  return (
    <Card className={styles.filters}>
      <CardContent>
        <Grid container spacing={3}>
          <Grid item>
            <FormControl className={styles.filterItem}>
              <InputLabel>{strings(stringKeys.nationalSocietyReports.list.filters.selectReportListType)}</InputLabel>
              <Select
                onChange={handleReportsTypeChange}
                value={filters.reportsType}
              >
                <MenuItem value="unknownSender">
                  {strings(stringKeys.nationalSocietyReports.list.filters.unknownSenderReportListType)}
                </MenuItem>
                <MenuItem value="main">
                  {strings(stringKeys.nationalSocietyReports.list.filters.mainReportsListType)}
                </MenuItem>
                <MenuItem value="fromDcp">
                  {strings(stringKeys.nationalSocietyReports.list.filters.dcpReportListType)}
                </MenuItem>
              </Select>
            </FormControl>
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
              select
              label={strings(stringKeys.nationalSocietyReports.list.filters.healthRisk)}
              onChange={handleHealthRiskChange}
              value={value.healthRiskId || 0}
              className={styles.filterItem}
              InputLabelProps={{ shrink: true }}
            >
              <MenuItem value={0}>{strings(stringKeys.nationalSocietyReports.list.filters.healthRiskAll)}</MenuItem>

              {healthRisks.map(healthRisk => (
                <MenuItem key={`filter_healthRisk_${healthRisk.id}`} value={healthRisk.id}>
                  {healthRisk.name}
                </MenuItem>
              ))}
            </TextField>
          </Grid>

          <Grid item>
            <InputLabel className={styles.filterLabel}>{strings(stringKeys.nationalSocietyReports.list.filters.status)}</InputLabel>
            <RadioGroup
              value={filters.status}
              onChange={handleStatusChange}
              className={styles.filterRadioGroup}
            >
              <FormControlLabel control={<Radio />} label={strings(stringKeys.nationalSocietyReports.list.success)} value={true} />
              <FormControlLabel control={<Radio />} label={strings(stringKeys.nationalSocietyReports.list.error)} value={false} />
            </RadioGroup>
          </Grid>
        </Grid>
      </CardContent>
    </Card>
  );
}
