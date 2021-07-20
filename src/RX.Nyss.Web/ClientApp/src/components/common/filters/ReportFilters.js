import styles from './ReportFilters.module.scss';

import React, { useState } from 'react';
import {
  Grid,
  TextField,
  MenuItem,
  Card,
  CardContent,
  FormControl,
  InputLabel,
  Select
} from '@material-ui/core';
import { AreaFilter } from './AreaFilter';
import { strings, stringKeys } from '../../../strings';
import { reportErrorFilterTypes, DataCollectorType } from './logic/reportFilterConstsants';
import { Fragment } from 'react';
import { ReportStatusFilter } from './ReportStatusFilter';
import { ReportTypeFilter } from './ReportTypeFilter';

export const ReportFilters = ({ filters, nationalSocietyId, healthRisks, onChange, showCorrectReportFilters, showTrainingFilter, hideReportTypeFilter }) => {
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

  const handleDataCollectorTypeChange = event =>
    onChange(updateValue({ dataCollectorType: event.target.value }));

  const handleErrorTypeChange = event =>
    onChange(updateValue({ errorType: event.target.value }));

  const handleReportStatusChange = event =>
    onChange(updateValue({ reportStatus: { ...value.reportStatus, [event.target.name]: event.target.checked }}));

  const handleReportTypeChange = event =>
    onChange(updateValue({ reportType: { ...value.reportType, [event.target.name]: event.target.checked }}));

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
              showUnknown={true}
            />
          </Grid>

          <Grid item>
            <FormControl className={styles.filterItem}>
              <InputLabel>{strings(stringKeys.filters.report.selectReportListType)}</InputLabel>
              <Select
                onChange={handleDataCollectorTypeChange}
                value={filters.dataCollectorType}
              >
                <MenuItem value={DataCollectorType.unknownSender}>
                  {strings(stringKeys.filters.report.unknownSenderReportListType)}
                </MenuItem>
                <MenuItem value={DataCollectorType.human}>
                  {strings(stringKeys.filters.report.mainReportsListType)}
                </MenuItem>
                <MenuItem value={DataCollectorType.collectionPoint}>
                  {strings(stringKeys.filters.report.dcpReportListType)}
                </MenuItem>
              </Select>
            </FormControl>
          </Grid>

          {showCorrectReportFilters && (
            <Fragment>
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

              <Grid item>
                <ReportStatusFilter
                  filter={value.reportStatus}
                  onChange={handleReportStatusChange}
                  correctReports={showCorrectReportFilters}
                  showTrainingFilter={showTrainingFilter}
                  showDismissedFilter
                />
              </Grid>
            </Fragment>
          )}

          {!showCorrectReportFilters && (
            <Fragment>
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

              {!hideReportTypeFilter && (
                <Grid item>
                  <ReportTypeFilter
                    filter={value.reportType}
                    onChange={handleReportTypeChange}
                    showTrainingFilter={showTrainingFilter}
                    showCorrectedFilter={false}
                  />
                </Grid>
              )}
            </Fragment>
          )}
        </Grid>
      </CardContent>
    </Card>
  );
}
