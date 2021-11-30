import styles from "./AlertsFilters.module.scss";
import { useState, useEffect } from 'react';
import { Grid, TextField, MenuItem, Card, CardContent } from '@material-ui/core';
import LocationFilter from "../../common/filters/LocationFilter";
import { strings, stringKeys } from "../../../strings";
import { alertStatusFilters } from "../logic/alertsConstants";
import { renderFilterLabel } from "../../common/filters/logic/locationFilterService";

export const AlertsFilters = ({ filters, filtersData, onChange }) => {
  const [value, setValue] = useState(null);
  const [locationsFilterLabel, setLocationsFilterLabel] = useState(strings(stringKeys.filters.area.all));
  const [healthRisks, setHealthRisks] = useState(null);
  const [locations, setLocations] = useState([]);

  useEffect(() => {
    filters && setValue(filters);
  }, [filters]);

  useEffect(() => {
    filtersData && setHealthRisks(filtersData.healthRisks);
    filtersData && setLocations(filtersData.locations);
  }, [filtersData]);

  useEffect(() => {
    const label = !value || !locations ? strings(stringKeys.filters.area.all) : renderFilterLabel(value.locations, locations.regions, false);
    setLocationsFilterLabel(label);
  }, [value, locations]);

  const updateValue = (change) => {
    const newValue = {
      ...value,
      ...change
    }

    setValue(newValue);
    return newValue;
  };

  const handleLocationChange = (newValue) => {
    onChange(updateValue({ locations: newValue }));
  }

  const handleHealthRiskChange = (event) => {
    onChange(updateValue({ healthRiskId: event.target.value > 0 ? event.target.value : null }));
  }

  const handleStatusChange = (event) => {
    onChange(updateValue({ status: event.target.value }));
  }

  if (!value || !healthRisks) {
    return null;
  }

  return (
    <Card className={styles.filters}>
      <CardContent>
        <Grid container spacing={2}>
          <Grid item>
            <LocationFilter
              value={value.locations}
              locations={locations}
              filterLabel={locationsFilterLabel}
              onChange={handleLocationChange}
            />
          </Grid>

          <Grid item>
            <TextField
              select
              label={strings(stringKeys.alerts.filters.healthRisks)}
              onChange={handleHealthRiskChange}
              value={value.healthRiskId || 0}
              className={styles.filterItem}
              InputLabelProps={{ shrink: true }}
            >
              <MenuItem value={0}>{strings(stringKeys.alerts.filters.healthRisksAll)}</MenuItem>

              {healthRisks.map(hr => (
                <MenuItem key={`filter_healthRisk_${hr.id}`} value={hr.id}>
                  {hr.name}
                </MenuItem>
              ))}
            </TextField>
          </Grid>

          <Grid item>
            <TextField
              select
              label={strings(stringKeys.alerts.filters.status)}
              value={value.status || 'All'}
              onChange={handleStatusChange}
              className={styles.filterItem}
              InputLabelProps={{ shrink: true }}
            >
              {Object.values(alertStatusFilters).map(status => (
                <MenuItem key={`filter_status_${status}`} value={status}>
                  {status}
                </MenuItem>
              ))}
            </TextField>
          </Grid>
        </Grid>
      </CardContent>
    </Card>
  );
}
