import styles from "./DataCollectorsFilters.module.scss";
import React, { useState, useEffect } from 'react';
import Grid from '@material-ui/core/Grid';
import TextField from "@material-ui/core/TextField";
import MenuItem from "@material-ui/core/MenuItem";
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import { AreaFilter } from "../common/filters/AreaFilter";
import { strings, stringKeys } from "../../strings";
import { sexValues, trainingStatus } from './logic/dataCollectorsConstants';
import { InputLabel, RadioGroup, FormControlLabel, Radio } from "@material-ui/core";
import * as roles from '../../authentication/roles';

export const DataCollectorsFilters = ({ filters, nationalSocietyId, supervisors, onChange, callingUserRoles }) => {
  const [value, setValue] = useState(null);
  const [selectedArea, setSelectedArea] = useState(null);

  useEffect(() => {
    filters && setValue(filters);
    filters && setSelectedArea(filters.area);
  }, [filters]);

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

  const handleSupervisorChange = event =>
    onChange(updateValue({ supervisorId: event.target.value === 0 ? null : event.target.value }));

  const handleSexChange = event =>
    onChange(updateValue({ sex: event.target.value }));

  const handleTrainingStatusChange = event =>
    onChange(updateValue({ trainingStatus: event.target.value }));

  const handleNameChange = event =>
    onChange(updateValue({ name: event.target.value }));

  if (!value) {
    return null;
  }

  return (
    <Card className={styles.filters}>
      <CardContent>
        <Grid container spacing={2}>
          <Grid item>
            <TextField
              label={strings(stringKeys.dataCollector.filters.name)}
              onChange={handleNameChange}
              className={styles.filterItem}
              InputLabelProps={{ shrink: true }}
            >
            </TextField>
          </Grid>
          <Grid item>
            <AreaFilter
              nationalSocietyId={nationalSocietyId}
              selectedItem={selectedArea}
              onChange={handleAreaChange}
            />
          </Grid>

          {(!callingUserRoles.some(r => r === roles.Supervisor) &&
            <Grid item>
              <TextField
                select
                label={strings(stringKeys.dataCollector.filters.supervisors)}
                onChange={handleSupervisorChange}
                value={value.supervisorId || 0}
                className={styles.filterItem}
                InputLabelProps={{ shrink: true }}
              >
                <MenuItem value={0}>{strings(stringKeys.dataCollector.filters.supervisorsAll)}</MenuItem>

                {supervisors.map(supervisor => (
                  <MenuItem key={`filter_supervisor_${supervisor.id}`} value={supervisor.id}>
                    {supervisor.name}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
          )}

          <Grid item>
            <TextField
              select
              label={strings(stringKeys.dataCollector.filters.sex)}
              onChange={handleSexChange}
              value={value.sex || "all"}
              className={styles.filterItem}
              InputLabelProps={{ shrink: true }}
            >
              <MenuItem value="all">
                {strings(stringKeys.dataCollector.filters.sexAll)}
              </MenuItem>

              {sexValues.map(sex => (
                <MenuItem key={`datacollector_filter_${sex}`} value={sex}>
                  {strings(stringKeys.dataCollector.constants.sex[sex.toLowerCase()])}
                </MenuItem>
              ))}
            </TextField>
          </Grid>

          <Grid item>
            <InputLabel>{strings(stringKeys.dataCollector.filters.trainingStatus)}</InputLabel>
            <RadioGroup
              value={value.trainingStatus}
              onChange={handleTrainingStatusChange}
              className={styles.filterRadioGroup}>
              {trainingStatus.map(status => (
                <FormControlLabel key={`trainingStatus_filter_${status}`} control={<Radio />} label={strings(stringKeys.dataCollector.constants.trainingStatus[status])} value={status} />
              ))}
            </RadioGroup>
          </Grid>
        </Grid>
      </CardContent>
    </Card>
  );
}
