import styles from './DataCollectorsPerformanceFilters.module.scss';
import React, { useEffect, useReducer } from 'react';
import Grid from '@material-ui/core/Grid';
import { AreaFilter } from "../common/filters/AreaFilter";
import { Card, CardContent, Button, TextField, MenuItem } from "@material-ui/core";
import { useSelector } from "react-redux";
import { strings, stringKeys } from '../../strings';
import useDebounce from '../../utils/debounce';
import * as roles from '../../authentication/roles';

export const DataCollectorsPerformanceFilters = ({ onChange, filters }) => {
  const nationalSocietyId = useSelector(state => state.dataCollectors.filtersData.nationalSocietyId);
  const supervisors = useSelector(state => state.dataCollectors.filtersData.supervisors);
  const callingUserRoles = useSelector(state => state.appData.user.roles);

  const [name, setName] = useReducer((state, action) => {
    if (state.value !== action) {
      return { changed: true, value: action };
    } else {
      return state;
    }
  }, { value: '', changed: false });

  const debouncedName = useDebounce(name, 500);

  const handleAreaChange = (item) =>
    onChange({ type: 'updateArea', area: item ? { type: item.type, id: item.id, name: item.name } : null, pageNumber: 1 });

  const handleNameChange = event =>
    setName(event.target.value);

  const handleSupervisorChange = event => 
    onChange({ type: 'updateSupervisor', supervisorId: event.target.value === 0 ? null : event.target.value });

  useEffect(() => {
    debouncedName.changed && onChange({ type: 'updateName', name: debouncedName.value });
  }, [debouncedName, onChange]);

  const filterIsSet = filters && (
    filters.area !== null ||
    (filters.name !== null && filters.name !== '') ||
    Object.values(filters).slice(3).some(f => Object.values(f).some(v => !v))
  );

  const resetFilters = () => {
    setName('');
    onChange({ type: 'reset' });
  }

  if (!filters || !nationalSocietyId) {
    return null;
  }

  return (
    <Card>
      <CardContent>
        <Grid container spacing={2}>
          <Grid item>
            <TextField
              label={strings(stringKeys.dataCollector.performanceListFilters.name)}
              onChange={handleNameChange}
              value={name.value}
              className={styles.filterItem}
              InputLabelProps={{ shrink: true }}
            />
          </Grid>
          <Grid item>
            <AreaFilter
              nationalSocietyId={nationalSocietyId}
              selectedItem={filters.area}
              onChange={handleAreaChange}
            />
          </Grid>

          {(!callingUserRoles.some(r => r === roles.Supervisor) &&
            <Grid item>
              <TextField
                select
                label={strings(stringKeys.dataCollector.filters.supervisors)}
                onChange={handleSupervisorChange}
                value={filters.supervisorId || 0}
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

          {filterIsSet && (
            <Grid item className={styles.resetButton}>
              <Button onClick={resetFilters}>
                {strings(stringKeys.dataCollector.filters.resetAll)}
              </Button>
            </Grid>
          )}
        </Grid>
      </CardContent>
    </Card>
  );
}
