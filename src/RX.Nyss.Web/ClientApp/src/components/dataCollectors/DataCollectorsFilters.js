import styles from "./DataCollectorsFilters.module.scss";
import { useState, useEffect, useReducer } from 'react';
import { strings, stringKeys } from "../../strings";
import { sexValues, trainingStatus, deployedMode } from './logic/dataCollectorsConstants';
import {
  InputLabel,
  RadioGroup,
  FormControlLabel,
  Radio,
  Grid,
  TextField,
  MenuItem,
  Card,
  CardContent,
} from "@material-ui/core";
import * as roles from '../../authentication/roles';
import useDebounce from "../../utils/debounce";
import { shallowEqual } from "react-redux";
import LocationFilter from "../common/filters/LocationFilter";
import { renderFilterLabel } from "../common/filters/logic/locationFilterService";

export const DataCollectorsFilters = ({ supervisors, locations, onChange, callingUserRoles, filters }) => {
  
  const [locationsFilterLabel, setLocationsFilterLabel] = useState(strings(stringKeys.filters.area.all));

  const [filter, setFilter] = useReducer((state, action) => {
    const newState = { ...state.value, ...action };
    if (!shallowEqual(newState, state.value)) {
      return { ...state, changed: true, value: newState }
    } else {
      return state
    }
  }, { value: filters, changed: false });

  const [name, setName] = useReducer((state, action) => {
    if (state.value !== action) {
      return { changed: true, value: action };
    } else {
      return state;
    }
  }, { value: '', changed: false });
  const debouncedName = useDebounce(name, 500);

  useEffect(() => {
    filter.changed && onChange(filter.value);
  }, [filter, onChange]);

  useEffect(() => {
    const label = !filter.value || !locations ? strings(stringKeys.filters.area.all) : renderFilterLabel(filter.value.locations, locations.regions, false);
    setLocationsFilterLabel(label);
  }, [filter, locations]);

  const updateValue = (change) => {
    setFilter(change);
  };

  const handleLocationChange = (newValue) => {
    updateValue({ locations: newValue });
  }

  const handleSupervisorChange = event =>
    updateValue({ supervisorId: event.target.value === 0 ? null : event.target.value });

  const handleSexChange = event =>
    updateValue({ sex: event.target.value });

  const handleTrainingStatusChange = event =>
    updateValue({ trainingStatus: event.target.value });

  const handleDeployedModeChange = event =>
    updateValue({ deployedMode: event.target.value });

  const handleNameChange = (event) => setName(event.target.value);

  useEffect(() => {
    debouncedName.changed && updateValue({ name: debouncedName.value });
  }, [debouncedName]);

  return !!filter && (
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
            <LocationFilter
              value={filter.value.locations}
              locations={locations}
              filterLabel={locationsFilterLabel}
              onChange={handleLocationChange}
            />
          </Grid>

          {(!callingUserRoles.some(r => r === roles.Supervisor) &&
            <Grid item>
              <TextField
                select
                label={strings(stringKeys.dataCollector.filters.supervisors)}
                onChange={handleSupervisorChange}
                value={filter.value.supervisorId || 0}
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
              value={filter.value.sex || "all"}
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
              value={filter.value.trainingStatus || 'All'}
              onChange={handleTrainingStatusChange}
              className={styles.filterRadioGroup}>
              {trainingStatus.map(status => (
                <FormControlLabel key={`trainingStatus_filter_${status}`} control={<Radio />} label={strings(stringKeys.dataCollector.constants.trainingStatus[status])} value={status} />
              ))}
            </RadioGroup>
          </Grid>

          <Grid item>
            <InputLabel>{strings(stringKeys.dataCollector.filters.deployedMode)}</InputLabel>
            <RadioGroup
              value={filter.value.deployedMode}
              onChange={handleDeployedModeChange}
              className={styles.filterRadioGroup}>
              {deployedMode.map(status => (
                <FormControlLabel key={`deployedMode_filter_${status}`} control={<Radio />} label={strings(stringKeys.dataCollector.constants.deployedMode[status])} value={status} />
              ))}
            </RadioGroup>
          </Grid>

        </Grid>
      </CardContent>
    </Card>
  );
}
