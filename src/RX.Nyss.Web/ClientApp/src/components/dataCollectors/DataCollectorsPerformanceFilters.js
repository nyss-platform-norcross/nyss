import React from 'react';
import Grid from '@material-ui/core/Grid';
import { strings, stringKeys } from "../../strings";
import { AreaFilter } from "../common/filters/AreaFilter";
import { performanceStatus } from "./logic/dataCollectorsConstants";
import { FormGroup, Card, CardContent } from "@material-ui/core";
import { getIconFromStatus } from "./logic/dataCollectorsService";
import CheckboxWithIcon from "../common/filters/CheckboxWithIcon";
import { useSelector } from "react-redux";
import { DataCollectorStatusIcon } from '../common/icon/DataCollectorStatusIcon';

export const DataCollectorsPerformanceFilters = ({ onChange }) => {
  const filtersValue = useSelector(state => state.dataCollectors.performanceListFilters);
  const nationalSocietyId = useSelector(state => state.dataCollectors.filtersData.nationalSocietyId);

  const handleAreaChange = (item) => {
    onChange({ ...filtersValue, area: item ? { type: item.type, id: item.id, name: item.name } : null });
  }
  const handleReportingCorrectlyChange = (change) => {
    onChange({ ...filtersValue, reportingCorrectly: change.target.checked });
  }

  const handleReportingWithErrorsChange = (change) => {
    onChange({ ...filtersValue, reportingWithErrors: change.target.checked });
  }

  const handleNotReportingChange = (change) => {
    onChange({ ...filtersValue, notReporting: change.target.checked });
  }

  if (filtersValue == null || nationalSocietyId == null) {
    return null;
  }

  return (
    <Card>
      <CardContent>
        <Grid container spacing={3}>
          <Grid item>
            <FormGroup>
              <CheckboxWithIcon
                label={strings(stringKeys.dataCollector.performanceList.legend[performanceStatus.reportingCorrectly])}
                name={performanceStatus.reportingCorrectly}
                value={filtersValue.reportingCorrectly}
                onChange={handleReportingCorrectlyChange}
                icon={
                  <DataCollectorStatusIcon icon={getIconFromStatus(performanceStatus.reportingCorrectly)} status={performanceStatus.reportingCorrectly} />
                }
              />

              <CheckboxWithIcon
                label={strings(stringKeys.dataCollector.performanceList.legend[performanceStatus.reportingWithErrors])}
                name={performanceStatus.reportingWithErrors}
                value={filtersValue.reportingWithErrors}
                onChange={handleReportingWithErrorsChange}
                icon={
                  <DataCollectorStatusIcon icon={getIconFromStatus(performanceStatus.reportingWithErrors)} status={performanceStatus.reportingWithErrors} />
                }
              />

              <CheckboxWithIcon
                label={strings(stringKeys.dataCollector.performanceList.legend[performanceStatus.notReporting])}
                name={performanceStatus.notReporting}
                value={filtersValue.notReporting}
                onChange={handleNotReportingChange}
                icon={
                  <DataCollectorStatusIcon icon={getIconFromStatus(performanceStatus.notReporting)} status={performanceStatus.notReporting} />
                }
              />
            </FormGroup>
          </Grid>

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
