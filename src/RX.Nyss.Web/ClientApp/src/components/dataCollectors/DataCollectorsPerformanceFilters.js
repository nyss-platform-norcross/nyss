import styles from './DataCollectorsPerformanceFilters.module.scss';
import React, { useState, useEffect } from 'react';
import Grid from '@material-ui/core/Grid';
import { AreaFilter } from "../common/filters/AreaFilter";
import { Card, CardContent, Button, TextField } from "@material-ui/core";
import { useSelector } from "react-redux";
import { strings, stringKeys } from '../../strings';
import useDebounce from '../../utils/debounce';

export const DataCollectorsPerformanceFilters = ({ onChange, filters }) => {
  const [filterValue, setFilterValue] = useState(null);
  const nationalSocietyId = useSelector(state => state.dataCollectors.filtersData.nationalSocietyId);
  const [name, setName] = useState('');
  const debouncedName = useDebounce(name, 500);

  useEffect(() => {
    filters && setFilterValue(filters);
  }, [filters]);

  useEffect(() => {
    filterValue && Object.keys(filterValue).length > 1 && onChange(filterValue);
  }, [filterValue, onChange]);

  const updateValue = (newVal) =>
    setFilterValue((v) => { return { ...v, ...newVal } });

  const handleAreaChange = (item) =>
    updateValue({ area: item ? { type: item.type, id: item.id, name: item.name } : null, pageNumber: 1 });

  const handleNameChange = event =>
    setName(event.target.value);

  useEffect(() => {
    updateValue({ name: debouncedName });
  }, [debouncedName]);

  const filterIsSet = filterValue && (
    filterValue.area !== null ||
    (filterValue.name !== null && filterValue.name !== '') ||
    Object.values(filterValue).slice(3).some(f => Object.values(f).some(v => !v))
  );

  const resetFilters = () => {
    setName('');
    updateValue({
      area: null,
      pageNumber: 1,
      lastWeek: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      },
      twoWeeksAgo: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      },
      threeWeeksAgo: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      },
      fourWeeksAgo: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      },
      fiveWeeksAgo: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      },
      sixWeeksAgo: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      },
      sevenWeeksAgo: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      },
      eightWeeksAgo: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      }
    });
  }

  if (!filterValue || !nationalSocietyId) {
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
              value={name}
              className={styles.filterItem}
              InputLabelProps={{ shrink: true }}
            />
          </Grid>
          <Grid item>
            <AreaFilter
              nationalSocietyId={nationalSocietyId}
              selectedItem={filterValue.area}
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
