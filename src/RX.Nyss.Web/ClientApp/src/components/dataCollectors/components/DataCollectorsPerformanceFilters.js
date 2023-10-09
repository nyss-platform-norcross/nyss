import styles from './DataCollectorsPerformanceFilters.module.scss';
import { useEffect, useReducer, useState } from 'react';
import {
  Card,
  CardContent,
  Button,
  TextField,
  MenuItem,
  Grid,
  Radio,
  RadioGroup,
  FormControlLabel,
  InputLabel
} from "@material-ui/core";
import { useSelector } from "react-redux";
import { strings, stringKeys } from '../../../strings';
import useDebounce from '../../../utils/debounce';
import * as roles from '../../../authentication/roles';
import {trainingStatus, trainingStatusAll, trainingStatusTrained} from "../logic/dataCollectorsConstants";
import LocationFilter from '../../common/filters/LocationFilter';
import { renderFilterLabel } from '../../common/filters/logic/locationFilterService';

export const DataCollectorsPerformanceFilters = ({ onChange, filters, rtl }) => {
  const locations = useSelector(state => state.dataCollectors.filtersData.locations);
  const supervisors = useSelector(state => state.dataCollectors.filtersData.supervisors);
  const callingUserRoles = useSelector(state => state.appData.user.roles);

  const [locationsFilterLabel, setLocationsFilterLabel] = useState(strings(stringKeys.filters.area.all));

  const [name, setName] = useReducer((state, action) => {
    if (state.value !== action) {
      return { changed: true, value: action };
    } else {
      return state;
    }
  }, { value: '', changed: false });

  const debouncedName = useDebounce(name, 500);

  const handleAreaChange = (newValue) =>
    onChange({ type: 'updateLocations', locations: newValue, pageNumber: 1 });

  const handleNameChange = event =>
    setName(event.target.value);

  const handleSupervisorChange = event =>
    onChange({ type: 'updateSupervisor', supervisorId: event.target.value === 0 ? null : event.target.value });

  const handleTrainingStatusChange = event =>
    onChange({ type: 'updateTrainingStatus', trainingStatus: event.target.value });

  useEffect(() => {
    debouncedName.changed && onChange({ type: 'updateName', name: debouncedName.value });
  }, [debouncedName, onChange]);

  useEffect(() =>
    setLocationsFilterLabel(!filters || !locations ? strings(stringKeys.filters.area.all) : renderFilterLabel(filters.locations, locations.regions, false))
  , [filters, locations]);

  const filterIsSet = filters && (
    filters.locations !== null ||
    (filters.name !== null && filters.name !== '') ||
    filters.trainingStatus !== trainingStatusTrained ||
    Object.values(filters).slice(4).some(f =>
      Object.values(f).some(week =>
        Object.values(week).slice(1).some(v => !v)))
  );

  const resetFilters = () => {
    setName('');
    onChange({ type: 'reset' });
  }

  if (!filters) {
    return null;
  }

  return (
    <Card>
      <CardContent>
        <Grid container spacing={2}>
          <Grid item>
            <TextField
              label={strings(stringKeys.common.name)}
              onChange={handleNameChange}
              value={name.value}
              className={styles.filterItem}
              InputLabelProps={{ shrink: true }}
            />
          </Grid>
          <Grid item>
            <LocationFilter
              allLocations={locations}
              filteredLocations={filters.locations}
              filterLabel={locationsFilterLabel}
              onChange={handleAreaChange}
              rtl={rtl}
            />
          </Grid>

          {(!callingUserRoles.some(r => r === roles.Supervisor) &&
            <Grid item>
              <TextField
                select
                label={strings(stringKeys.dataCollectors.filters.supervisors)}
                onChange={handleSupervisorChange}
                value={filters.supervisorId || 0}
                className={styles.filterItem}
                InputLabelProps={{ shrink: true }}
              >
                <MenuItem value={0}>{strings(stringKeys.dataCollectors.filters.supervisorsAll)}</MenuItem>

                {supervisors.map(supervisor => (
                  <MenuItem key={`filter_supervisor_${supervisor.id}`} value={supervisor.id}>
                    {supervisor.name}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
          )}

          <Grid item>
            <InputLabel>{strings(stringKeys.dataCollectors.filters.trainingStatus)}</InputLabel>
            <RadioGroup
              value={filters.trainingStatus}
              onChange={handleTrainingStatusChange}
              className={styles.filterRadioGroup}>
              {trainingStatus
                .filter(status => status !== trainingStatusAll)
                .map(status => (
                  <FormControlLabel key={`trainingStatus_filter_${status}`} control={<Radio />} label={strings(stringKeys.dataCollectors.constants.trainingStatus[status])} value={status} />
                ))}
            </RadioGroup>
          </Grid>

          {filterIsSet && (
            <Grid item className={styles.resetButton}>
              <Button onClick={resetFilters}>
                {strings(stringKeys.dataCollectors.filters.resetAll)}
              </Button>
            </Grid>
          )}
        </Grid>
      </CardContent>
    </Card>
  );
}
