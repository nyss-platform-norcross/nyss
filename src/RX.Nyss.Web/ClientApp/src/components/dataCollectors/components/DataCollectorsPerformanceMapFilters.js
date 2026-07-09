import styles from "./DataCollectorsPerformanceMapFilters.module.scss";
import { Grid } from "@material-ui/core";
import { strings, stringKeys } from "../../../strings";
import { DatePicker } from "../../forms/DatePicker";
import { convertToLocalDate, convertToUtc } from "../../../utils/date";
import useLocalFilters from "../../common/filters/useLocalFilters";
import { useEffect } from "react";
import { HealthRiskFilter } from "../../common/filters/HealthRiskFilter";

export const DataCollectorsPerformanceMapFilters = ({
  filters,
  onChange,
  healthRisks,
  rtl,
}) => {
  //Reducer for local filters state
  const [localFilters, updateLocalFilters] = useLocalFilters(filters);

  useEffect(() => {  
    updateLocalFilters(filters);
  }, [filters]);


  //Fetches new data based on changes in filters
  const handleFiltersChange = (filterChanges) => {
    const updatedFilters = updateLocalFilters(filterChanges);
    onChange(updatedFilters);
  };

  const handleDateFromChange = (date) =>
    handleFiltersChange({ startDate: convertToUtc(date) });

  const handleDateToChange = (date) =>
    handleFiltersChange({ endDate: convertToUtc(date) });

  const handleHealthRiskChange = (healthRiskIds) =>
    handleFiltersChange({ healthRiskIds });

  return (
    <Grid container spacing={2} className={styles.filters}>
      <Grid item>
        <DatePicker
          onChange={handleDateFromChange}
          label={strings(stringKeys.dashboard.filters.startDate)}
          value={convertToLocalDate(localFilters.startDate)}
        />
      </Grid>

      <Grid item>
        <DatePicker
          onChange={handleDateToChange}
          label={strings(stringKeys.dashboard.filters.endDate)}
          value={convertToLocalDate(localFilters.endDate)}
        />
      </Grid>

      <Grid item>
        <HealthRiskFilter
          allHealthRisks={healthRisks || []}
          filteredHealthRisks={localFilters.healthRiskIds || []}
          onChange={handleHealthRiskChange}
          updateValue={(value) =>
            updateLocalFilters({ healthRiskIds: value.healthRisks || [] })
          }
          rtl={rtl}
        />
      </Grid>
    </Grid>
  );
};
