import React from 'react';
import Grid from '@material-ui/core/Grid';
import { AreaFilter } from "../common/filters/AreaFilter";
import { Card, CardContent, InputLabel } from "@material-ui/core";
import { useSelector } from "react-redux";

export const DataCollectorsPerformanceFilters = ({ onChange }) => {
  const filtersValue = useSelector(state => state.dataCollectors.performanceListFilters);
  const nationalSocietyId = useSelector(state => state.dataCollectors.filtersData.nationalSocietyId);

  const handleAreaChange = (item) => {
    onChange({ ...filtersValue, area: item ? { type: item.type, id: item.id, name: item.name } : null });
  }

  if (filtersValue == null || nationalSocietyId == null) {
    return null;
  }

  return (
    <Card>
      <CardContent>
        <Grid container spacing={3}>
          <Grid item>
            <AreaFilter
              nationalSocietyId={nationalSocietyId}
              selectedItem={filtersValue.area}
              onChange={handleAreaChange}
            />
          </Grid>
        </Grid>
      </CardContent>
    </Card>
  );
}
