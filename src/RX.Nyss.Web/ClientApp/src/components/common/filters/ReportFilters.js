import styles from "./ReportFilters.module.scss";

import { useEffect, useState } from "react";
import {
  Grid,
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
} from "@material-ui/core";
import { strings, stringKeys } from "../../../strings";
import {
  reportErrorFilterTypes,
  DataCollectorType,
  correctedStateTypes,
} from "./logic/reportFilterConstsants";
import { Fragment } from "react";
import { ReportStatusFilter } from "./ReportStatusFilter";
import LocationFilter from "./LocationFilter";
import { renderFilterLabel } from "./logic/locationFilterService";
import { HealthRiskFilter } from "../../common/filters/HealthRiskFilter";

export const ReportFilters = ({
  filters,
  healthRisks,
  locations,
  onChange,
  showCorrectReportFilters,
  hideTrainingStatusFilter,
  hideCorrectedFilter,
  rtl,
}) => {
  const [value, setValue] = useState(filters);

  const [locationsFilterLabel, setLocationsFilterLabel] = useState(
    strings(stringKeys.filters.area.all)
  );
  const updateValue = (change) => {
    setValue((prev) => ({ ...prev, ...change }));
    return value;
  };

  useEffect(() => {
    const label =
      !value || !locations || !value.locations || value.locations.regionIds.length === 0
        ? strings(stringKeys.filters.area.all)
        : renderFilterLabel(value.locations, locations.regions, true);
    setLocationsFilterLabel(label);
  }, [value.locations]);


  useEffect(() => {
    if (!locations) return;

    const regionIds = locations.regions.map((region) => region.id);

    const districtIds = [];
    locations.regions.forEach((region) =>
      region.districts.forEach((district) => districtIds.push(district.id))
    );

    const villageIds = [];
    locations.regions.forEach((region) =>
      region.districts.forEach((district) =>
        district.villages.forEach((village) => villageIds.push(village.id))
      )
    );

    const zoneIds = [];
    locations.regions.forEach((region) =>
      region.districts.forEach((district) =>
        district.villages.forEach((village) =>
          village.zones.forEach((zone) => zoneIds.push(zone.id))
        )
      )
    );

    const filterValue = {
      regionIds: regionIds,
      districtIds: districtIds,
      villageIds: villageIds,
      zoneIds: zoneIds,
      includeUnknownLocation: false,
    };

    updateValue({ locations: filterValue });
  }, [locations]);

  const handleLocationChange = (newValue) => {
    onChange(
      updateValue({
        locations: newValue,
      })
    );
  };

  const handleHealthRiskChange = (filteredHealthRisks) =>
    onChange(updateValue({ healthRisks: filteredHealthRisks }));

  const handleDataCollectorTypeChange = (event) =>
    onChange(updateValue({ dataCollectorType: event.target.value }));

  const handleErrorTypeChange = (event) =>
    onChange(updateValue({ errorType: event.target.value }));

  const handleCorrectedStateChange = (event) =>
    onChange(updateValue({ correctedState: event.target.value }));

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

  if (!value) {
    return null;
  }

  return (
    <Card>
      <CardContent>
        <Grid container spacing={2}>
          <Grid item>
            <LocationFilter
              filteredLocations={value.locations}
              allLocations={locations}
              onChange={handleLocationChange}
              showUnknownLocation
              filterLabel={locationsFilterLabel}
              rtl={rtl}
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
                <HealthRiskFilter
                  allHealthRisks={healthRisks}
                  filteredHealthRisks={value.healthRisks}
                  onChange={handleHealthRiskChange}
                  updateValue={updateValue}
                />
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

          {!showCorrectReportFilters && !hideCorrectedFilter && (
            <Fragment>
              <Grid item>
                <FormControl className={styles.filterItem}>
                  <InputLabel>
                    {strings(stringKeys.filters.report.isCorrected)}
                  </InputLabel>
                  <Select
                    onChange={handleCorrectedStateChange}
                    value={filters.correctedState}
                  >
                    {correctedStateTypes.map((state) => (
                      <MenuItem value={state} key={`correctedState_${state}`}>
                        {strings(
                          stringKeys.filters.report.correctedStates[state]
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
                    {strings(stringKeys.dashboard.filters.trainingStatus)}
                  </FormLabel>
                  <RadioGroup
                    value={value.trainingStatus}
                    onChange={handleTrainingStatusChange}
                    className={styles.radioGroup}
                  >
                    <FormControlLabel
                      className={styles.radio}
                      label={strings(
                        stringKeys.dataCollectors.constants.trainingStatus
                          .Trained
                      )}
                      value={"Trained"}
                      control={<Radio color="primary" />}
                    />
                    <FormControlLabel
                      className={styles.radio}
                      label={strings(
                        stringKeys.dataCollectors.constants.trainingStatus
                          .InTraining
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
