import styles from "./ReportFilters.module.scss";

import React, { useState } from 'react';
import {
  FormControlLabel,
  Radio,
  RadioGroup,
  Grid,
  TextField,
  MenuItem,
  Card,
  CardContent,
  FormControl,
  InputLabel,
  Select,
} from '@material-ui/core';
import { AreaFilter } from "./AreaFilter";
import { strings, stringKeys } from "../../../strings";
import { reportErrorFilterTypes, ReportListType } from './logic/reportFilterConstsants';

export const ReportFilters = ({ filters, nationalSocietyId, healthRisks, onChange, showTrainingFilter, showCorrectReportFilters }) => {
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
    onChange(updateValue({ area: item ? { type: item.type, id: item.id, name: item.name } : null }));
  }

  const handleHealthRiskChange = event =>
    onChange(updateValue({ healthRiskId: event.target.value === 0 ? null : event.target.value }));

  const handleReportsTypeChange = event =>
    onChange(updateValue({ reportsType: event.target.value }));

  const handleIsTrainingChange = event =>
    onChange(updateValue({ isTraining: event.target.value === "true" }));

  const handleErrorTypeChange = event =>
    onChange(updateValue({ errorType: event.target.value }));

  if (!value) {
    return null;
  }

  return (
    <Card>
      <CardContent>
        <Grid container spacing={2}>
          <Grid item>
            <AreaFilter
              nationalSocietyId={nationalSocietyId}
              selectedItem={selectedArea}
              onChange={handleAreaChange}
            />
          </Grid>

          <Grid item>
            <FormControl className={styles.filterItem}>
              <InputLabel>{strings(stringKeys.filters.report.selectReportListType)}</InputLabel>
              <Select
                onChange={handleReportsTypeChange}
                value={filters.reportsType}
              >
                <MenuItem value={ReportListType.unknownSender}>
                  {strings(stringKeys.filters.report.unknownSenderReportListType)}
                </MenuItem>
                <MenuItem value={ReportListType.main}>
                  {strings(stringKeys.filters.report.mainReportsListType)}
                </MenuItem>
                <MenuItem value={ReportListType.fromDcp}>
                  {strings(stringKeys.filters.report.dcpReportListType)}
                </MenuItem>
              </Select>
            </FormControl>
          </Grid>

          {showCorrectReportFilters && (
            <Grid item>
              <TextField
                select
                label={strings(stringKeys.filters.report.healthRisk)}
                onChange={handleHealthRiskChange}
                value={value.healthRiskId || 0}
                className={styles.filterItem}
                InputLabelProps={{ shrink: true }}
              >
                <MenuItem value={0}>{strings(stringKeys.filters.report.healthRiskAll)}</MenuItem>

                {healthRisks.map(healthRisk => (
                  <MenuItem key={`filter_healthRisk_${healthRisk.id}`} value={healthRisk.id}>
                    {healthRisk.name}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
          )}

          {!showCorrectReportFilters && (
            <Grid item>
              <FormControl className={styles.filterItem}>
                <InputLabel>{strings(stringKeys.filters.report.selectErrorType)}</InputLabel>
                <Select
                  onChange={handleErrorTypeChange}
                  value={filters.errorType}
                >
                  {reportErrorFilterTypes.map(errorType => (
                    <MenuItem value={errorType} key={`errorfilter_${errorType}`}>
                      {strings(stringKeys.filters.report.errorTypes[errorType])}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
          )}

          {showTrainingFilter &&
            <Grid item>
              <InputLabel className={styles.filterLabel}>{strings(stringKeys.filters.report.reportType)}</InputLabel>
              <RadioGroup
                value={filters.isTraining}
                onChange={handleIsTrainingChange}
                className={styles.filterRadioGroup}
              >
                <FormControlLabel control={<Radio />} label={strings(stringKeys.filters.report.nonTrainingReports)} value={false} />
                <FormControlLabel control={<Radio />} label={strings(stringKeys.filters.report.trainingReports)} value={true} />
              </RadioGroup>
            </Grid>
          }
        </Grid>
      </CardContent>
    </Card>
  );
}
