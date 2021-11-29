import styles from "./ReportFilters.module.scss";

import { useEffect, useState } from "react";
import {
  Grid,
  TextField,
  MenuItem,
  Card,
  CardContent,
  FormControl,
  InputLabel,
  Select,
  FormLabel,
  RadioGroup,
  FormControlLabel,
  Radio,
  Checkbox,
} from "@material-ui/core";
import { strings, stringKeys } from "../../../strings";
import {
  reportErrorFilterTypes,
  DataCollectorType,
} from "./logic/reportFilterConstsants";
import { Fragment } from "react";
import { ReportStatusFilter } from "./ReportStatusFilter";
import MultiSelectField from "../../forms/MultiSelectField";
import LocationFilter from "./LocationFilter";
import { renderFilterLabel } from "./logic/locationFilterService";

export const ReportFilters = ({
  filters,
  healthRisks,
  locations,
  onChange,
  showCorrectReportFilters,
  hideTrainingStatusFilter,
}) => {
  const [value, setValue] = useState(filters);

  const [locationsFilterLabel, setLocationsFilterLabel] = useState(strings(stringKeys.filters.area.all));

  useEffect(() => {
    const label = !value || !locations ? strings(stringKeys.filters.area.all) : renderFilterLabel(value.locations, locations.regions, true);
    setLocationsFilterLabel(label);
  }, [value, locations]);

  const updateValue = (change) => {
    const newValue = {
      ...value,
      ...change,
    };

    setValue(newValue);
    return newValue;
  };

  const handleLocationChange = (newValue) => {
    onChange(
      updateValue({
        locations: newValue,
      })
    );
  };

  const handleHealthRiskChange = (event) =>
    onChange(
      updateValue({
        healthRisks: typeof event.target.value === 'string' ? event.target.value.split(',') : event.target.value,
      })
    );

  const handleDataCollectorTypeChange = (event) =>
    onChange(updateValue({ dataCollectorType: event.target.value }));

  const handleErrorTypeChange = (event) =>
    onChange(updateValue({ errorType: event.target.value }));

  const handleReportStatusChange = (event) =>
    onChange(
      updateValue({
        reportStatus: {
          ...value.reportStatus,
          [event.target.name]: event.target.checked,
        },
      })
    );

  const handleTrainingStatusChange = (event) =>
    onChange(
      updateValue({
        ...value,
        trainingStatus: event.target.value,
      })
    );

  const renderHealthRiskValues = (selectedIds) => 
    selectedIds.length < 1 || selectedIds.length === healthRisks.length
      ? strings(stringKeys.filters.report.healthRiskAll)
      : selectedIds.map(id => healthRisks.find(hr => hr.id === id).name).join(',');

  if (!value) {
    return null;
  }


  return (
    <Card>
      <CardContent>
        <Grid container spacing={2}>
          <Grid item>
            <LocationFilter
              value={value.locations}
              locations={locations}
              onChange={handleLocationChange}
              showUnknownLocation
              filterLabel={locationsFilterLabel}
            />
          </Grid>

          <Grid item>
            <FormControl className={styles.filterItem}>
              <InputLabel>
                {strings(stringKeys.filters.report.selectReportListType)}
              </InputLabel>
              <Select
                onChange={handleDataCollectorTypeChange}
                value={filters.dataCollectorType}
              >
                <MenuItem value={DataCollectorType.unknownSender}>
                  {strings(
                    stringKeys.filters.report.unknownSenderReportListType
                  )}
                </MenuItem>
                <MenuItem value={DataCollectorType.human}>
                  {strings(stringKeys.filters.report.mainReportsListType)}
                </MenuItem>
                <MenuItem value={DataCollectorType.collectionPoint}>
                  {strings(stringKeys.filters.report.dcpReportListType)}
                </MenuItem>
              </Select>
            </FormControl>
          </Grid>

          {showCorrectReportFilters && (
            <Fragment>
              <Grid item>
                <MultiSelectField
                  name="healthRisks"
                  label={strings(stringKeys.filters.report.healthRisk)}
                  onChange={handleHealthRiskChange}
                  value={value.healthRisks}
                  className={styles.filterItem}
                  renderValues={renderHealthRiskValues}
                >
                  {healthRisks.map((healthRisk) => (
                    <MenuItem
                      key={`filter_healthRisk_${healthRisk.id}`}
                      value={healthRisk.id}>
                      <Checkbox checked={value.healthRisks.indexOf(healthRisk.id) > -1} />
                      <span>{healthRisk.name}</span>
                    </MenuItem>
                  ))}
                </MultiSelectField>
              </Grid>
            </Fragment>
          )}

          {!showCorrectReportFilters && (
            <Fragment>
              <Grid item>
                <FormControl className={styles.filterItem}>
                  <InputLabel>
                    {strings(stringKeys.filters.report.selectErrorType)}
                  </InputLabel>
                  <Select
                    onChange={handleErrorTypeChange}
                    value={filters.errorType}
                  >
                    {reportErrorFilterTypes.map((errorType) => (
                      <MenuItem
                        value={errorType}
                        key={`errorfilter_${errorType}`}
                      >
                        {strings(
                          stringKeys.filters.report.errorTypes[errorType]
                        )}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
            </Fragment>
          )}

          {!hideTrainingStatusFilter && (
            <Fragment>
              <Grid item>
                <FormControl>
                  <FormLabel component="legend">
                    {strings(stringKeys.project.dashboard.filters.trainingStatus)}
                  </FormLabel>
                  <RadioGroup
                    value={value.trainingStatus}
                    onChange={handleTrainingStatusChange}
                    className={styles.radioGroup}
                  >
                    <FormControlLabel
                      className={styles.radio}
                      label={strings(
                        stringKeys.dataCollector.constants.trainingStatus.Trained
                      )}
                      value={"Trained"}
                      control={<Radio color="primary" />}
                    />
                    <FormControlLabel
                      className={styles.radio}
                      label={strings(
                        stringKeys.dataCollector.constants.trainingStatus.InTraining
                      )}
                      value={"InTraining"}
                      control={<Radio color="primary" />}
                    />
                  </RadioGroup>
                </FormControl>
              </Grid>
            </Fragment>
          )}

          {showCorrectReportFilters && (
            <Fragment>
              <Grid item>
                <ReportStatusFilter
                  filter={value.reportStatus}
                  onChange={handleReportStatusChange}
                  correctReports={showCorrectReportFilters}
                  showDismissedFilter
                />
              </Grid>
            </Fragment>
          )}
        </Grid>
      </CardContent>
    </Card>
  );
};
