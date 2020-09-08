import styles from "./NationalSocietyDashboardFilters.module.scss";
import React, { useState } from 'react';
import Grid from '@material-ui/core/Grid';
import TextField from "@material-ui/core/TextField";
import MenuItem from "@material-ui/core/MenuItem";
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import { DatePicker } from "../../forms/DatePicker";
import { AreaFilter } from "../../common/filters/AreaFilter";
import { strings, stringKeys } from "../../../strings";

export const NationalSocietyDashboardFilters = ({ filters, nationalSocietyId, healthRisks, organizations, onChange }) => {
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

  const handleOrganizationChange = event =>
    onChange(updateValue({ organizationId: event.target.value === 0 ? null : event.target.value }))

  const handleDateFromChange = date =>
    onChange(updateValue({ startDate: date.format('YYYY-MM-DD') }))

  const handleDateToChange = date =>
    onChange(updateValue({ endDate: date.format('YYYY-MM-DD') }))

  const handleGroupingTypeChange = event =>
    onChange(updateValue({ groupingType: event.target.value }))

  const handleReportsTypeChange = event =>
    onChange(updateValue({ reportsType: event.target.value }))

  if (!value) {
    return null;
  }

  return (
    <Card className={styles.filters}>
      <CardContent>
        <Grid container spacing={2}>
          <Grid item>
            <DatePicker
              className={styles.filterDate}
              onChange={handleDateFromChange}
              label={strings(stringKeys.nationalSociety.dashboard.filters.startDate)}
              value={value.startDate}
            />
          </Grid>

          <Grid item>
            <DatePicker
              className={styles.filterDate}
              onChange={handleDateToChange}
              label={strings(stringKeys.nationalSociety.dashboard.filters.endDate)}
              value={value.endDate}
            />
          </Grid>

          <Grid item>
            <TextField
              select
              label={strings(stringKeys.nationalSociety.dashboard.filters.timeGrouping)}
              onChange={handleGroupingTypeChange}
              value={value.groupingType}
              style={{ width: 130 }}
              InputLabelProps={{ shrink: true }}
            >
              <MenuItem value="Day">{strings(stringKeys.nationalSociety.dashboard.filters.timeGroupingDay)}</MenuItem>
              <MenuItem value="Week">{strings(stringKeys.nationalSociety.dashboard.filters.timeGroupingWeek)}</MenuItem>
            </TextField>
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
              label={strings(stringKeys.nationalSociety.dashboard.filters.healthRisk)}
              onChange={handleHealthRiskChange}
              value={value.healthRiskId || 0}
              className={styles.filterItem}
              InputLabelProps={{ shrink: true }}
            >
              <MenuItem value={0}>{strings(stringKeys.nationalSociety.dashboard.filters.healthRiskAll)}</MenuItem>

              {healthRisks.map(healthRisk => (
                <MenuItem key={`filter_healthRisk_${healthRisk.id}`} value={healthRisk.id}>
                  {healthRisk.name}
                </MenuItem>
              ))}
            </TextField>
          </Grid>

          <Grid item>
            <TextField
              select
              label={strings(stringKeys.nationalSociety.dashboard.filters.reportsType)}
              onChange={handleReportsTypeChange}
              value={value.reportsType || "all"}
              className={styles.filterItem}
              InputLabelProps={{ shrink: true }}
            >
              <MenuItem value="all">
                {strings(stringKeys.nationalSociety.dashboard.filters.allReportsType)}
              </MenuItem>
              <MenuItem value="dataCollector">
                {strings(stringKeys.nationalSociety.dashboard.filters.dataCollectorReportsType)}
              </MenuItem>
              <MenuItem value="dataCollectionPoint">
                {strings(stringKeys.nationalSociety.dashboard.filters.dataCollectionPointReportsType)}
              </MenuItem>
            </TextField>
          </Grid>

          <Grid item>
            <TextField
              select
              label={strings(stringKeys.nationalSociety.dashboard.filters.organization)}
              onChange={handleOrganizationChange}
              value={value.organizationId || 0}
              className={styles.filterItem}
              InputLabelProps={{ shrink: true }}
            >
              <MenuItem value={0}>{strings(stringKeys.nationalSociety.dashboard.filters.organizationsAll)}</MenuItem>

              {organizations.map(organization => (
                <MenuItem key={`filter_organization_${organization.id}`} value={organization.id}>
                  {organization.name}
                </MenuItem>
              ))}
            </TextField>
          </Grid>
        </Grid>
      </CardContent>
    </Card>
  );
}
