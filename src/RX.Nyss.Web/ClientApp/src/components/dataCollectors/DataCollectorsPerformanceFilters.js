import styles from './DataCollectorsPerformanceFilters.module.scss';
import React from 'react';
import Grid from '@material-ui/core/Grid';
import { AreaFilter } from "../common/filters/AreaFilter";
import { Card, CardContent, Button } from "@material-ui/core";
import { useSelector } from "react-redux";
import { strings, stringKeys } from '../../strings';

export const DataCollectorsPerformanceFilters = ({ onChange }) => {
  const filtersValue = useSelector(state => state.dataCollectors.performanceListFilters);
  const nationalSocietyId = useSelector(state => state.dataCollectors.filtersData.nationalSocietyId);

  const handleAreaChange = (item) => {
    onChange({ ...filtersValue, area: item ? { type: item.type, id: item.id, name: item.name } : null });
  }

  const filterIsSet = filtersValue && (filtersValue.area !== null || Object.values(filtersValue).slice(1).some(f => Object.values(f).some(v => !v)));

  const resetFilters = () => {
    onChange({
      area: null,
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

  if (filtersValue == null || nationalSocietyId == null) {
    return null;
  }

  return (
    <Card>
      <CardContent>
        <Grid container spacing={3} className={styles.filterContainer}>
          <Grid item>
            <AreaFilter
              nationalSocietyId={nationalSocietyId}
              selectedItem={filtersValue.area}
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
    </Card >
  );
}
