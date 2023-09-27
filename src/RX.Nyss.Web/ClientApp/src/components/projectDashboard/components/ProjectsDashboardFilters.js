import styles from "./ProjectsDashboardFilters.module.scss";
import { useEffect, useState } from "react";
import { DatePicker } from "../../forms/DatePicker";
import { strings, stringKeys } from "../../../strings";
import { ConditionalCollapse } from "../../common/conditionalCollapse/ConditionalCollapse";
import { convertToLocalDate, convertToUtc } from "../../../utils/date";
import ExpandMore from "@material-ui/icons/ExpandMore";
import DateRange from "@material-ui/icons/DateRange";
import {
  LinearProgress,
  Chip,
  IconButton,
  Grid,
  TextField,
  MenuItem,
  Card,
  useMediaQuery,
  CardContent,
  CardHeader,
  FormControl,
  FormControlLabel,
  FormLabel,
  RadioGroup,
  Radio,
} from "@material-ui/core";
import { ReportStatusFilter } from "../../common/filters/ReportStatusFilter";
import { Fragment } from "react";
import { DataConsumer } from "../../../authentication/roles";
import LocationFilter from "../../common/filters/LocationFilter";
import { renderFilterLabel } from "../../common/filters/logic/locationFilterService";
import { HealthRiskFilter } from "../../common/filters/HealthRiskFilter";

export const ProjectsDashboardFilters = ({
  filters,
  healthRisks,
  locations,
  organizations,
  onChange,
  isFetching,
  isGeneratingPdf,
  isFilterExpanded,
  setIsFilterExpanded,
  userRoles,
  rtl,
}) => {
  const [value, setValue] = useState(filters);
  const [locationsFilterLabel, setLocationsFilterLabel] = useState(
    strings(stringKeys.filters.area.all)
  );
  const isSmallScreen = useMediaQuery((theme) => theme.breakpoints.down("lg"));

  const updateValue = (change) => {
    const newValue = {
      ...value,
      ...change,
    };

    setValue((prev) => ({ ...prev, ...change }));
    return newValue;
  };

  // useEffect which runs on mount and when locations are added, edited or removed. Updates locations in the filter state in order to avoid mismatch between locations and filtered locations
  useEffect(() => {
    if (!locations) return;

    const filterValue = {
      regionIds: locations.regions.map((region) => region.id),
      districtIds: locations.regions.map((region) => region.districts.map((district) => district.id)).flat(),
      villageIds: locations.regions.map((region) => region.districts.map((district) => district.villages.map((village) => village.id))).flat(2),
      zoneIds: locations.regions.map((region) => region.districts.map((district) => district.villages.map((village) => village.zones.map((zone) => zone.id)))).flat(3),
      includeUnknownLocation: false,
    }

    updateValue({ locations: filterValue });
  }, [locations]);

  // Sets label for location filter to 'All' or "Region (+n)"
  useEffect(() => {
    const label =
      !value || !locations || !value.locations || value.locations.regionIds.length === 0
        ? strings(stringKeys.filters.area.all)
        : renderFilterLabel(value.locations, locations.regions, false);
    setLocationsFilterLabel(label);
  }, [value.locations]);

  const handleLocationChange = (newValue) => {
    onChange(
      updateValue({
        locations: newValue,
      })
    );
  };

  const handleHealthRiskChange = (filteredHealthRisks) =>
    onChange(updateValue({ healthRisks: filteredHealthRisks }));

  const handleOrganizationChange = (event) =>
    onChange(
      updateValue({
        organizationId: event.target.value === 0 ? null : event.target.value,
      })
    );

  const handleDateFromChange = (date) =>
    onChange(updateValue({ startDate: convertToUtc(date) }));

  const handleDateToChange = (date) =>
    onChange(updateValue({ endDate: convertToUtc(date) }));

  const handleGroupingTypeChange = (event) =>
    onChange(updateValue({ groupingType: event.target.value }));

  const handleDataCollectorTypeChange = (event) =>
    onChange(updateValue({ dataCollectorType: event.target.value }));

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
    onChange(updateValue({ trainingStatus: event.target.value }));

  const collectionsTypes = {
    all: strings(stringKeys.dashboard.filters.allReportsType),
    dataCollector: strings(
      stringKeys.dashboard.filters.dataCollectorReportsType
    ),
    dataCollectionPoint: strings(
      stringKeys.dashboard.filters.dataCollectionPointReportsType
    ),
  };

  const allLocationsSelected = () =>
    !value.locations ||
    value.locations.regionIds.length === locations.regions.length;

  if (!value) {
    return null;
  }

  return (
    <Card className={styles.filters}>
      {isFetching && <LinearProgress color="primary" />}
      {isSmallScreen && (
        <CardContent className={styles.collapsedFilterBar}>
          <Grid container spacing={2} alignItems="center">
            <Grid item>
              <CardHeader title={strings(stringKeys.dashboard.filters.title)} />
            </Grid>
            {!isFilterExpanded && (
              <Fragment>
                <Grid item>
                  <Chip
                    icon={<DateRange />}
                    label={`${convertToLocalDate(value.startDate).format(
                      "YYYY-MM-DD"
                    )} - ${convertToLocalDate(value.endDate).format(
                      "YYYY-MM-DD"
                    )}`}
                    onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                  />
                </Grid>
                <Grid item>
                  <Chip
                    onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                    label={
                      value.groupingType === "Day"
                        ? strings(stringKeys.dashboard.filters.timeGroupingDay)
                        : strings(stringKeys.dashboard.filters.timeGroupingWeek)
                    }
                  />
                </Grid>
              </Fragment>
            )}
            {!isFilterExpanded && !allLocationsSelected() && (
              <Grid item>
                <Chip
                  label={locationsFilterLabel}
                  onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                />
              </Grid>
            )}
            {!isFilterExpanded && value.healthRiskId && (
              <Grid item>
                <Chip
                  label={
                    healthRisks.filter((hr) => hr.id === value.healthRiskId)[0]
                      .name
                  }
                  onDelete={() => onChange(updateValue({ healthRiskId: null }))}
                  onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                />
              </Grid>
            )}
            {!isFilterExpanded && value.dataCollectorType !== "all" && (
              <Grid item>
                <Chip
                  label={collectionsTypes[value.dataCollectorType]}
                  onDelete={() =>
                    onChange(updateValue({ dataCollectorType: "all" }))
                  }
                  onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                />
              </Grid>
            )}
            {!isFilterExpanded && value.organizationId && (
              <Grid item>
                <Chip
                  label={
                    organizations.filter(
                      (o) => o.id === value.organizationId
                    )[0].name
                  }
                  onDelete={() =>
                    onChange(updateValue({ organizationId: null }))
                  }
                  onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                />
              </Grid>
            )}
            {!isFilterExpanded &&
              !userRoles.some((r) => r === DataConsumer) &&
              value.reportStatus.kept && (
                <Grid item>
                  <Chip
                    label={strings(stringKeys.filters.report.kept)}
                    onDelete={() =>
                      onChange(
                        updateValue({
                          reportStatus: {
                            ...value.reportStatus,
                            kept: false,
                          },
                        })
                      )
                    }
                    onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                  />
                </Grid>
              )}
            {!isFilterExpanded &&
              !userRoles.some((r) => r === DataConsumer) &&
              value.reportStatus.dismissed && (
                <Grid item>
                  <Chip
                    label={strings(stringKeys.filters.report.dismissed)}
                    onDelete={() =>
                      onChange(
                        updateValue({
                          reportStatus: {
                            ...value.reportStatus,
                            dismissed: false,
                          },
                        })
                      )
                    }
                    onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                  />
                </Grid>
              )}
            {!isFilterExpanded &&
              !userRoles.some((r) => r === DataConsumer) &&
              value.reportStatus.notCrossChecked && (
                <Grid item>
                  <Chip
                    label={strings(stringKeys.filters.report.notCrossChecked)}
                    onDelete={() =>
                      onChange(
                        updateValue({
                          reportStatus: {
                            ...value.reportStatus,
                            notCrossChecked: false,
                          },
                        })
                      )
                    }
                    onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                  />
                </Grid>
              )}
            {!isFilterExpanded &&
              !userRoles.some((r) => r === DataConsumer) &&
              value.trainingStatus !== "Trained" && (
                <Grid item>
                  <Chip
                    label={strings(
                      stringKeys.dataCollectors.constants.trainingStatus
                        .InTraining
                    )}
                    onDelete={() =>
                      onChange(
                        updateValue({
                          ...value,
                          trainingStatus: "Trained",
                        })
                      )
                    }
                    onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                  />
                </Grid>
              )}
            <Grid
              item
              className={`${styles.expandFilterButton} ${rtl ? styles.rtl : ""
                }`}
            >
              <IconButton
                data-expanded={isFilterExpanded}
                onClick={() => setIsFilterExpanded(!isFilterExpanded)}
              >
                <ExpandMore />
              </IconButton>
            </Grid>
          </Grid>
        </CardContent>
      )}

      <ConditionalCollapse
        collapsible={isSmallScreen && !isGeneratingPdf}
        expanded={isFilterExpanded}
      >
        {!isSmallScreen && (
          <Grid container spacing={2}>
            <CardHeader
              title={strings(stringKeys.dashboard.filters.title)}
              className={styles.filterTitle}
            />
          </Grid>
        )}
        <CardContent data-printable={true}>
          <Grid container spacing={2}>
            <Grid item>
              <DatePicker
                className={styles.filterDate}
                onChange={handleDateFromChange}
                label={strings(stringKeys.dashboard.filters.startDate)}
                value={convertToLocalDate(value.startDate)}
              />
            </Grid>

            <Grid item>
              <DatePicker
                className={styles.filterDate}
                onChange={handleDateToChange}
                label={strings(stringKeys.dashboard.filters.endDate)}
                value={convertToLocalDate(value.endDate)}
              />
            </Grid>

            <Grid item>
              <FormControl>
                <FormLabel component="legend">
                  {strings(stringKeys.dashboard.filters.timeGrouping)}
                </FormLabel>
                <RadioGroup
                  value={value.groupingType}
                  onChange={handleGroupingTypeChange}
                  className={styles.radioGroup}
                >
                  <FormControlLabel
                    className={styles.radio}
                    label={strings(
                      stringKeys.dashboard.filters.timeGroupingDay
                    )}
                    value={"Day"}
                    control={<Radio color="primary" />}
                  />
                  <FormControlLabel
                    className={styles.radio}
                    label={strings(
                      stringKeys.dashboard.filters.timeGroupingWeek
                    )}
                    value={"Week"}
                    control={<Radio color="primary" />}
                  />
                </RadioGroup>
              </FormControl>
            </Grid>

            <Grid item>
              <LocationFilter
                allLocations={locations}
                filteredLocations={value.locations}
                filterLabel={locationsFilterLabel}
                onChange={handleLocationChange}
                rtl={rtl}
              />
            </Grid>

            <Grid item>
              <HealthRiskFilter
                allHealthRisks={healthRisks}
                filteredHealthRisks={value.healthRisks}
                onChange={handleHealthRiskChange}
                updateValue={updateValue}
              />
            </Grid>

            <Grid item>
              <TextField
                select
                label={strings(stringKeys.dashboard.filters.reportsType)}
                onChange={handleDataCollectorTypeChange}
                value={value.dataCollectorType || "all"}
                className={styles.filterItem}
                InputLabelProps={{ shrink: true }}
              >
                <MenuItem value="all">{collectionsTypes["all"]}</MenuItem>
                <MenuItem value="dataCollector">
                  {collectionsTypes["dataCollector"]}
                </MenuItem>
                <MenuItem value="dataCollectionPoint">
                  {collectionsTypes["dataCollectionPoint"]}
                </MenuItem>
              </TextField>
            </Grid>

            {organizations.length > 1 && (
              <Grid item>
                <TextField
                  select
                  label={strings(stringKeys.dashboard.filters.organization)}
                  onChange={handleOrganizationChange}
                  value={value.organizationId || 0}
                  className={styles.filterItem}
                  InputLabelProps={{ shrink: true }}
                >
                  <MenuItem value={0}>
                    {strings(stringKeys.dashboard.filters.organizationsAll)}
                  </MenuItem>

                  {organizations.map((organization) => (
                    <MenuItem
                      key={`filter_organization_${organization.id}`}
                      value={organization.id}
                    >
                      {organization.name}
                    </MenuItem>
                  ))}
                </TextField>
              </Grid>
            )}

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
                      stringKeys.dataCollectors.constants.trainingStatus.Trained
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

            {!userRoles.some((r) => r === DataConsumer) && (
              <Grid item>
                <ReportStatusFilter
                  filter={value.reportStatus}
                  correctReports
                  showDismissedFilter
                  onChange={handleReportStatusChange}
                />
              </Grid>
            )}
          </Grid>
        </CardContent>
      </ConditionalCollapse>
    </Card>
  );
};
