import styles from "./AlertsFilters.module.scss";
import React, { useState, useEffect } from 'react';
import Grid from '@material-ui/core/Grid';
import TextField from "@material-ui/core/TextField";
import MenuItem from "@material-ui/core/MenuItem";
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import { AreaFilter } from "../../common/filters/AreaFilter";
import { strings, stringKeys } from "../../../strings";
import { alertStatusFilters } from "../logic/alertsConstants";
import { useSelector } from "react-redux";

export const AlertsFilters = ({ filters, filtersData, onChange }) => {
  const nationalSocietyId = useSelector(state => state.appData.siteMap.parameters.nationalSocietyId);
  const [value, setValue] = useState(null);
  const [selectedArea, setSelectedArea] = useState(null);
  const [healthRisks, setHealthRisks] = useState(null);

  useEffect(() => {
    filters && setValue(filters);
    filters && setSelectedArea(filters.area);
  }, [filters]);

  useEffect(() => {
    filtersData && setHealthRisks(filtersData.healthRisks);
  }, [filtersData]);

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
            <AreaFilter
              nationalSocietyId={nationalSocietyId}
              selectedItem={selectedArea}
              onChange={handleAreaChange}
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
