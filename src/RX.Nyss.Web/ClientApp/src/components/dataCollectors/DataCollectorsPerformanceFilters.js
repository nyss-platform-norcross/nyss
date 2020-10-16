import styles from './DataCollectorsPerformanceFilters.module.scss';
import React, { useEffect, useReducer } from 'react';
import Grid from '@material-ui/core/Grid';
import { AreaFilter } from "../common/filters/AreaFilter";
import { Card, CardContent, Button, TextField } from "@material-ui/core";
import { useSelector, shallowEqual } from "react-redux";
import { strings, stringKeys } from '../../strings';
import useDebounce from '../../utils/debounce';
import { initialState } from '../../initialState';

export const DataCollectorsPerformanceFilters = ({ onChange }) => {
  const nationalSocietyId = useSelector(state => state.dataCollectors.filtersData.nationalSocietyId);
  
  const [filter, setFilter] = useReducer((state, action) => {
    const newState = { ...state.value, ...action };
    if (!shallowEqual(newState, state.value)) {
      return { ...state, changed: true, value: newState }
    } else {
      return state
    }
  }, { value: initialState.dataCollectors.performanceListFilters, changed: false });

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

  const updateValue = (newVal) =>
    setFilter(newVal);

  const handleAreaChange = (item) =>
    updateValue({ area: item ? { type: item.type, id: item.id, name: item.name } : null, pageNumber: 1 });

  const handleNameChange = event =>
    setName(event.target.value);

  useEffect(() => {
    debouncedName.changed && setFilter({ name: debouncedName.value });
  }, [debouncedName]);

  const filterIsSet = filter.value && (
    filter.value.area !== null ||
    (filter.value.name !== null && filter.value.name !== '') ||
    Object.values(filter.value).slice(3).some(f => Object.values(f).some(v => !v))
  );

  const resetFilters = () => {
    setName('');
    updateValue(initialState.dataCollectors.performanceListFilters);
  }

  if (!filter.value || !nationalSocietyId) {
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
              selectedItem={filter.value.area}
              onChange={handleAreaChange}
            />
          </Grid>
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
