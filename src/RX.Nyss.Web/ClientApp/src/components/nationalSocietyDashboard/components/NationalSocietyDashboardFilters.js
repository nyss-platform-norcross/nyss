import React from "react";
import { DatePicker } from "../../forms/DatePicker";
import { strings, stringKeys } from "../../../strings";
import {
  useMediaQuery,
  LinearProgress,
  Chip,
  IconButton,
  Grid,
  TextField,
  MenuItem,
  Card,
  CardContent,
  CardHeader,
  FormControl,
  FormLabel,
  RadioGroup,
  FormControlLabel,
  Radio,
  Typography,
  makeStyles,
} from "@material-ui/core";
import DateRange from "@material-ui/icons/DateRange";
import ExpandMore from "@material-ui/icons/ExpandMore";
import { ConditionalCollapse } from "../../common/conditionalCollapse/ConditionalCollapse";
import { convertToLocalDate, convertToUtc } from "../../../utils/date";
import { Fragment } from "react";
import { ReportStatusFilter } from "../../common/filters/ReportStatusFilter";
import { DataConsumer } from "../../../authentication/roles";
import LocationFilter from "../../common/filters/LocationFilter";
import { HealthRiskFilter } from "../../common/filters/HealthRiskFilter";
import useLocalFilters from "../../common/filters/useLocalFilters";
import useLocationFilter from "../../common/filters/useLocationFilter";
import { isAllLocationsSelected } from "../../common/filters/logic/locationFilterService";
import { useEffect } from "react";
import { trackEvent } from "../../../utils/appInsightsHelper";
import { DrawerFilter } from "../../common/filters/DrawerFilter";

const useStyles = makeStyles((theme) => ({
  selectFilterItem: {
    width: "150px",
    [theme.breakpoints.down("xs")]: {
      width: "100%",
      maxWidth: "220px",
    },
  },
  filterRadioGroup: {
    paddingTop: "5px",
  },
  radio: {
    height: "23px",
  },
  filters: {
    boxShadow: "#fff 0px 5px 5px 5px",
    padding: "0px",
    paddingBottom: "0px",
  },
  filterTitle: {
    paddingBottom: "1px",
  },
  collapsedFilterBar: {
    padding: "0px 5px",
    "&:last-child": {
      paddingBottom: "0px",
    },
  },
  expandFilterButton: {
    marginLeft: "auto",
    "> button": {
      transition: "transform 150ms",
    },
  },
  rtl: {
    marginLeft: 0,
    marginRight: "auto",
  },
}));

const Filter = ({
  localFilters,
  handleFiltersChange,
  updateLocalFilters,
  healthRisks,
  locations,
  locationsFilterLabel,
  organizations,
  userRoles,
  rtl,
  collectionsTypes,
}) => {
  const classes = useStyles();
  const isSmallScreen = useMediaQuery((theme) => theme.breakpoints.down("xs"));

  const handleLocationChange = (locations) => {
    handleFiltersChange({ locations: locations });
  };
  const handleHealthRiskChange = (filteredHealthRisks) =>
    handleFiltersChange({ healthRisks: filteredHealthRisks });

  {
    /* See comment below, (organization filter comment), explaining commented out code */
  }
  // const handleOrganizationChange = (event) =>
  //   handleFiltersChange({
  //     organizationId: event.target.value === 0 ? null : event.target.value,
  //   });

  const handleDateFromChange = (date) =>
    handleFiltersChange({ startDate: convertToUtc(date) });

  const handleDateToChange = (date) =>
    handleFiltersChange({ endDate: convertToUtc(date) });

  const handleGroupingTypeChange = (event) =>
    handleFiltersChange({ groupingType: event.target.value });

  const handleDataCollectorTypeChange = (event) =>
    handleFiltersChange({ dataCollectorType: event.target.value });

  const handleReportStatusChange = (event) =>
    handleFiltersChange({
      reportStatus: {
        ...localFilters.reportStatus,
        [event.target.name]: event.target.checked,
      },
    });

  return (
    <Grid container spacing={2} direction={"row"} alignItems={"flex-start"}>
      <Grid item xs={isSmallScreen ? 6 : null}>
        <DatePicker
          className={classes.selectFilterItem}
          onChange={handleDateFromChange}
          label={strings(stringKeys.dashboard.filters.startDate)}
          value={convertToLocalDate(localFilters.startDate)}
        />
      </Grid>

      <Grid item xs={isSmallScreen ? 6 : null}>
        <DatePicker
          className={classes.selectFilterItem}
          onChange={handleDateToChange}
          label={strings(stringKeys.dashboard.filters.endDate)}
          value={convertToLocalDate(localFilters.endDate)}
        />
      </Grid>

      <Grid item xs={isSmallScreen ? 6 : null}>
        <FormControl>
          <FormLabel component="legend">
            {strings(stringKeys.dashboard.filters.timeGrouping)}
          </FormLabel>
          <RadioGroup
            value={localFilters.groupingType}
            onChange={handleGroupingTypeChange}
            className={classes.filterRadioGroup}
          >
            <FormControlLabel
              className={classes.radio}
              label={strings(stringKeys.dashboard.filters.timeGroupingDay)}
              value={"Day"}
              control={<Radio color="primary" />}
            />
            <FormControlLabel
              className={classes.radio}
              label={strings(stringKeys.dashboard.filters.timeGroupingWeek)}
              value={"Week"}
              control={<Radio color="primary" />}
            />
          </RadioGroup>
        </FormControl>
      </Grid>

      {!userRoles.some((r) => r === DataConsumer) && (
        <Grid item xs={isSmallScreen ? 6 : null}>
          <ReportStatusFilter
            filter={localFilters.reportStatus}
            correctReports
            showDismissedFilter
            doNotWrap
            onChange={handleReportStatusChange}
          />
        </Grid>
      )}

      <Grid item xs={isSmallScreen ? 6 : null}>
        <LocationFilter
          filteredLocations={localFilters.locations}
          filterLabel={locationsFilterLabel}
          allLocations={locations}
          onChange={handleLocationChange}
          rtl={rtl}
        />
      </Grid>

      <Grid item xs={isSmallScreen ? 6 : null}>
        <HealthRiskFilter
          allHealthRisks={healthRisks}
          filteredHealthRisks={localFilters.healthRisks}
          onChange={handleHealthRiskChange}
          updateValue={updateLocalFilters}
          rtl={rtl}
        />
      </Grid>

      {/* Data collector type filter */}
      <Grid item xs={isSmallScreen ? 6 : null}>
        <TextField
          select
          label={strings(stringKeys.dashboard.filters.reportsType)}
          onChange={handleDataCollectorTypeChange}
          value={localFilters.dataCollectorType}
          className={classes.selectFilterItem}
          InputLabelProps={{ shrink: true }}
        >
          <MenuItem value="All">{collectionsTypes["All"]}</MenuItem>
          <MenuItem value="DataCollector">
            {collectionsTypes["DataCollector"]}
          </MenuItem>
          <MenuItem value="DataCollectionPoint">
            {collectionsTypes["DataCollectionPoint"]}
          </MenuItem>
        </TextField>
      </Grid>

      {/* NOTE: The organization filter is commented out since the overall multiple organization implementation in Nyss is currently deprecated/unclear of how it should be used. Don't delete commented code before clarification. Commented code causes an error. */}
      {/* {organizations.length > 1 && (
        <Grid item>
          <TextField
            select
            label={strings(stringKeys.dashboard.filters.organization)}
            onChange={handleOrganizationChange}
            value={localFilters.organizationId || 0}
            className={classes.selectFilterItem}
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
      )} */}
    </Grid>
  );
};

//Filters components should probably fetch data from redux store themselves
export const NationalSocietyDashboardFilters = ({
  filters,
  healthRisks,
  organizations,
  locations,
  onChange,
  isFetching,
  userRoles,
  isFilterExpanded,
  setIsFilterExpanded,
  rtl,
  isGeneratingPdf,
}) => {
  //Reducer for local filters state
  const [localFilters, updateLocalFilters] = useLocalFilters(filters);

  useEffect(() => {
    updateLocalFilters(filters);
  }, [filters]);

  //Fetches new data based on changes in filters
  const handleFiltersChange = (filters) => {
    trackEvent("NsDashboardFilterChange", { filters });

    const nextFilters = updateLocalFilters(filters);

    // Guard against invalid date ranges (end before start) to avoid crashes
    if (
      nextFilters.startDate &&
      nextFilters.endDate &&
      typeof nextFilters.endDate.isBefore === "function" &&
      nextFilters.endDate.isBefore(nextFilters.startDate)
    ) {
      // Do not propagate invalid filters to the backend
      return;
    }

    if (!isMediumScreen) {
      onChange(nextFilters);
    }
  };

  const showResults = () => {
    // When using the drawer on small screens, validate date range before fetching
    const nextFilters = localFilters;

    if (
      nextFilters.startDate &&
      nextFilters.endDate &&
      typeof nextFilters.endDate.isBefore === "function" &&
      nextFilters.endDate.isBefore(nextFilters.startDate)
    ) {
      // Do not propagate invalid filters to the backend
      return;
    }

    onChange(updateLocalFilters(nextFilters));
  };

  //Syncs locations from redux store with filter state and sets label for location filter to 'All' or "Region (+n)"
  //Neccecary if locations are added, edited or removed, to make all filters checked
  const [locationsFilterLabel] = useLocationFilter(
    locations,
    localFilters,
    updateLocalFilters,
  );

  const classes = useStyles();
  const isMediumScreen = useMediaQuery((theme) => theme.breakpoints.down("sm"));

  // Keys must match server enum casing (All, DataCollector, DataCollectionPoint)
  const collectionsTypes = {
    All: strings(stringKeys.dashboard.filters.allReportsType),
    DataCollector: strings(
      stringKeys.dashboard.filters.dataCollectorReportsType,
    ),
    DataCollectionPoint: strings(
      stringKeys.dashboard.filters.dataCollectionPointReportsType,
    ),
  };

  // Guard against undefined locations; default empty structure
  const safeLocations =
    locations || {
      regions: [],
      districts: [],
      villages: [],
      zones: [],
    };

  const safeFilterLocations = localFilters.locations || {
    regionIds: [],
    districtIds: [],
    villageIds: [],
    zoneIds: [],
    includeUnknownLocation: true,
  };

  const allLocationsSelected = () =>
    isAllLocationsSelected(
      safeFilterLocations,
      safeLocations.regions,
      false,
    );

  if (!localFilters) {
    return null;
  }

  if (isMediumScreen) {
    if (isFetching) {
      return <LinearProgress color="primary" />;
    }
    return (
      <Grid container justifyContent="center" style={{ marginBottom: 20 }}>
        <DrawerFilter
          title={null}
          children={
            <Filter
              localFilters={localFilters}
              handleFiltersChange={handleFiltersChange}
              updateLocalFilters={updateLocalFilters}
              healthRisks={healthRisks}
              locations={locations}
              locationsFilterLabel={locationsFilterLabel}
              organizations={organizations}
              userRoles={userRoles}
              rtl={rtl}
              collectionsTypes={collectionsTypes}
            />
          }
          showResults={showResults}
        />
      </Grid>
    );
  }

  return (
    <Card className={classes.filters}>
      {isFetching && <LinearProgress color="primary" />}
      <CardContent className={classes.collapsedFilterBar}>
        <Grid container spacing={2} alignItems="center">
          <Grid item>
            <CardHeader
              title={
                <Typography variant="h5">
                  {strings(stringKeys.dashboard.filters.title)}
                </Typography>
              }
              className={isFilterExpanded ? classes.filterTitle : null}
            />
          </Grid>
          {!isGeneratingPdf && !isFilterExpanded && (
            <Fragment>
              <Grid item>
                <Chip
                  icon={<DateRange />}
                  label={`${convertToLocalDate(localFilters.startDate).format(
                    "YYYY-MM-DD",
                  )} - ${convertToLocalDate(localFilters.endDate).format(
                    "YYYY-MM-DD",
                  )}`}
                  onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                />
              </Grid>
              <Grid item>
                <Chip
                  label={
                    localFilters.groupingType === "Day"
                      ? strings(stringKeys.dashboard.filters.timeGroupingDay)
                      : strings(stringKeys.dashboard.filters.timeGroupingWeek)
                  }
                  onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                />
              </Grid>
            </Fragment>
          )}
          {!isGeneratingPdf && !isFilterExpanded && !allLocationsSelected() && (
            <Grid item>
              <Chip
                label={locationsFilterLabel}
                onClick={() => setIsFilterExpanded(!isFilterExpanded)}
              />
            </Grid>
          )}
          {!isGeneratingPdf &&
            !isFilterExpanded &&
            localFilters.healthRiskId && (
              <Grid item>
                <Chip
                  label={
                    healthRisks.filter(
                      (hr) => hr.id === localFilters.healthRiskId,
                    )[0].name
                  }
                  onDelete={() => handleFiltersChange({ healthRiskId: null })}
                  onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                />
              </Grid>
            )}

          {!isGeneratingPdf &&
            !isFilterExpanded &&
            localFilters.dataCollectorType !== "All" && (
              <Grid item>
                <Chip
                  label={collectionsTypes[localFilters.dataCollectorType]}
                  onDelete={() =>
                    handleFiltersChange({ dataCollectorType: "All" })
                  }
                  onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                />
              </Grid>
            )}

          {/* See comment below, (organization filter comment), explaining commented out code */}
          {/* {!isFilterExpanded && localFilters.organizationId && (
              <Grid item>
                <Chip
                  label={
                    organizations.filter(
                      (o) => o.id === localFilters.organizationId,
                    )[0].name
                  }
                  onDelete={() => handleFiltersChange({ organizationId: null })}
                  onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                />
              </Grid>
            )} */}
          {!isGeneratingPdf &&
            !isFilterExpanded &&
            !userRoles.some((r) => r === DataConsumer) &&
            localFilters.reportStatus.kept && (
              <Grid item>
                <Chip
                  label={strings(stringKeys.filters.report.kept)}
                  onDelete={() =>
                    handleFiltersChange({
                      reportStatus: {
                        ...localFilters.reportStatus,
                        kept: false,
                      },
                    })
                  }
                  onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                />
              </Grid>
            )}
          {!isGeneratingPdf &&
            !isFilterExpanded &&
            !userRoles.some((r) => r === DataConsumer) &&
            localFilters.reportStatus.notCrossChecked && (
              <Grid item>
                <Chip
                  label={strings(stringKeys.filters.report.notCrossChecked)}
                  onDelete={() =>
                    handleFiltersChange({
                      reportStatus: {
                        ...localFilters.reportStatus,
                        notCrossChecked: false,
                      },
                    })
                  }
                  onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                />
              </Grid>
            )}
          <Grid
            item
            className={`${classes.expandFilterButton} ${
              rtl ? classes.rtl : ""
            }`}
          >
            <IconButton
              style={{
                transform: isFilterExpanded ? "rotate(180deg)" : "rotate(0deg)",
              }}
              onClick={() => setIsFilterExpanded(!isFilterExpanded)}
            >
              <ExpandMore />
            </IconButton>
          </Grid>
        </Grid>
      </CardContent>
      <ConditionalCollapse
        collapsible={!isGeneratingPdf}
        expanded={isFilterExpanded}
      >
        <CardContent data-printable={true}>
          <Filter
            localFilters={localFilters}
            handleFiltersChange={handleFiltersChange}
            updateLocalFilters={updateLocalFilters}
            healthRisks={healthRisks}
            locations={locations}
            locationsFilterLabel={locationsFilterLabel}
            organizations={organizations}
            userRoles={userRoles}
            rtl={rtl}
            collectionsTypes={collectionsTypes}
          />
        </CardContent>
      </ConditionalCollapse>
    </Card>
  );
};