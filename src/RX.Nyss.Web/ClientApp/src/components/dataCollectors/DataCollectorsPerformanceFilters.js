import styles from "./DataCollectorsPerformanceFilters.module.scss"

import React, { useState } from 'react';
import Grid from '@material-ui/core/Grid';
import { strings, stringKeys } from "../../strings";
import { AreaFilter } from "../common/filters/AreaFilter";
import CheckboxField from "../forms/CheckboxField";
import { performanceStatus } from "./logic/dataCollectorsConstants";
import { FormGroup, Card, CardContent, FormControlLabel, Checkbox } from "@material-ui/core";
import { createForm } from "../../utils/forms";
import { getIconFromStatus } from "./logic/dataCollectorsService";
import CheckboxWithIconField from "../forms/CheckboxWithIconField";
import { useSelector } from "react-redux";

export const DataCollectorsPerformanceFilters = ({ filters, nationalSocietyId, onChange }) => {
  const filtersValue = useSelector(state => state.dataCollectors.performanceListFilters);

  const handleAreaChange = (item) => {
    onChange({ ...filtersValue, area: item ? { type: item.type, id: item.id, name: item.name } : null });
  }
  const handleReportingCorrectlyChange = (change) => {
    onChange({ ...filtersValue, reportingCorrectly: change.target.checked });
  }

  const handleReportingWithErrorsChange = (change) => {
    onChange({ ...filtersValue, reportingWithErrors: change.target.checked});
  }

  const handleNotReportingChange = (change) => {
    onChange({ ...filtersValue, notReporting: change.target.checked});
  }

  if (filtersValue == null) {
    return null;
  }

  return (
    <Card className={styles.filters}>
      <CardContent>
        <Grid container spacing={3} className={styles.filters}>
          <Grid item>
            <AreaFilter
              nationalSocietyId={nationalSocietyId}
              selectedItem={filtersValue.area}
              onChange={handleAreaChange}
            />
          </Grid>

          <Grid item>
            <FormGroup>
              <CheckboxWithIconField
                label={strings(stringKeys.dataCollector.performanceList.legend[performanceStatus.reportingCorrectly])}
                name={performanceStatus.reportingCorrectly}
                value={filtersValue.reportingCorrectly}
                onChange={handleReportingCorrectlyChange}
                color="primary"
              />

              <CheckboxWithIconField
                label={strings(stringKeys.dataCollector.performanceList.legend[performanceStatus.reportingWithErrors])}
                name={performanceStatus.reportingWithErrors}
                value={filtersValue.reportingWithErrors}
                onChange={handleReportingWithErrorsChange}
                color="primary"
              />

              <CheckboxWithIconField
                label={strings(stringKeys.dataCollector.performanceList.legend[performanceStatus.notReporting])}
                name={performanceStatus.notReporting}
                value={filtersValue.notReporting}
                onChange={handleNotReportingChange}
                color="primary"
              />
            </FormGroup>
          </Grid>
        </Grid>
      </CardContent>
    </Card>
  );
}
