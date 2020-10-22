import styles from './DataCollectorsPerformanceFilters.module.scss';
import React, { useEffect, useReducer } from 'react';
import Grid from '@material-ui/core/Grid';
import { AreaFilter } from "../common/filters/AreaFilter";
import { Card, CardContent, Button, TextField } from "@material-ui/core";
import { useSelector } from "react-redux";
import { strings, stringKeys } from '../../strings';
import useDebounce from '../../utils/debounce';

export const DataCollectorsPerformanceFilters = ({ onChange, filters }) => {
  const nationalSocietyId = useSelector(state => state.dataCollectors.filtersData.nationalSocietyId);

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
