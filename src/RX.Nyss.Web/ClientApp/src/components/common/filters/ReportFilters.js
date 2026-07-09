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
  useMediaQuery,
  LinearProgress,
  makeStyles,
} from "@material-ui/core";
import { strings, stringKeys } from "../../../strings";
import {
  reportErrorFilterTypes,
  DataCollectorType,
  correctedStateTypes,
} from "./logic/reportFilterConstsants";
import { ReportStatusFilter } from "./ReportStatusFilter";
import LocationFilter from "./LocationFilter";
import { HealthRiskFilter } from "../../common/filters/HealthRiskFilter";
import useLocalFilters from "../../common/filters/useLocalFilters";
import useLocationFilter from "../../common/filters/useLocationFilter";
import { DrawerFilter } from "./DrawerFilter";
import { DatePicker } from "../../forms/DatePicker";
import { convertToLocalDate, convertToUtc } from "../../../utils/date";

const useStyles = makeStyles((theme) => ({
  selectFilterItem: {
    width: "150px",
    [theme.breakpoints.down("xs")]: {
      width: "100%",
      maxWidth: "220px",
    },
  },
  radio: {
    height: "23px",
  },
  radioGroup: {
    paddingTop: "5px",
  },
}));

const Filter = ({
  localFilters,
  handleFiltersChange,
  updateLocalFilters,
  healthRisks,
  locations,
  locationsFilterLabel,
  showCorrectReportFilters,
  hideCorrectedFilter,
  hideTrainingStatusFilter,
  rtl,
}) => {
  const classes = useStyles();
  const isSmallScreen = useMediaQuery((theme) => theme.breakpoints.down("xs"));

  const handleLocationChange = (newValue) => {
    handleFiltersChange({
      locations: newValue,
    });
  };

  const handleHealthRiskChange = (filteredHealthRisks) =>
    handleFiltersChange({ healthRisks: filteredHealthRisks });

  const handleDataCollectorTypeChange = (event) =>
    handleFiltersChange({ dataCollectorType: event.target.value });

  const handleErrorTypeChange = (event) =>
    handleFiltersChange({ errorType: event.target.value });

  const handleCorrectedStateChange = (event) =>
    handleFiltersChange({ correctedState: event.target.value });

  const handleReportStatusChange = (event) =>
    handleFiltersChange({
      reportStatus: {
        ...localFilters.reportStatus,
        [event.target.name]: event.target.checked,
      },
    });

  const handleTrainingStatusChange = (event) =>
    handleFiltersChange({
      ...localFilters,
      trainingStatus: event.target.value,
    });

  const handleDateFromChange = (date) =>
    handleFiltersChange({ startDate: convertToUtc(date) });

  const handleDateToChange = (date) =>
    handleFiltersChange({ endDate: convertToUtc(date) });

  return (
    <Grid
      container
      direction={"row"}
      alignItems={"flex-start"}
      justifyContent="flex-start"
      spacing={2}
    >
      <Grid item xs={isSmallScreen ? 6 : null}>
        <DatePicker
          select
          label={strings(stringKeys.dashboard.filters.startDate)}
          value={convertToLocalDate(localFilters.startDate)}
          onChange={handleDateFromChange}
          className={classes.selectFilterItem}
          InputLabelProps={{ shrink: true }}
        ></DatePicker>
      </Grid>
      <Grid item xs={isSmallScreen ? 6 : null}>
        <DatePicker
          select
          label={strings(stringKeys.dashboard.filters.endDate)}
          value={convertToLocalDate(localFilters.endDate)}
          onChange={handleDateToChange}
          className={classes.selectFilterItem}
          InputLabelProps={{ shrink: true }}
        ></DatePicker>
      </Grid>
      {/* Material UI v4 lacks some key features for Grids and breakpoints, so the reordering of components in the grid must be done with css order properties and ternaries using isSmallScreen */}
      <Grid item style={{ order: 1 }} xs={isSmallScreen ? 6 : null}>
        <LocationFilter
          filteredLocations={localFilters.locations}
          allLocations={locations}
          onChange={handleLocationChange}
          showUnknownLocation
          filterLabel={locationsFilterLabel}
          rtl={rtl}
        />
      </Grid>

      <Grid
        item
        style={{ order: isSmallScreen ? 6 : 2 }}
        xs={isSmallScreen ? 6 : null}
      >
        {/* Data Collector Type filter */}
        <FormControl className={classes.selectFilterItem}>
          <InputLabel>
            {strings(stringKeys.filters.report.selectReportListType)}
          </InputLabel>
          <Select
            onChange={handleDataCollectorTypeChange}
            value={localFilters.dataCollectorType}
          >
            <MenuItem value={DataCollectorType.unknownSender}>
              {strings(stringKeys.filters.report.unknownSenderReportListType)}
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
        <Grid item style={{ order: 3 }} xs={isSmallScreen ? 6 : null}>
          <HealthRiskFilter
            allHealthRisks={healthRisks}
            filteredHealthRisks={localFilters.healthRisks}
            onChange={handleHealthRiskChange}
            updateValue={updateLocalFilters}
            rtl={rtl}
          />
        </Grid>
      )}

      {!showCorrectReportFilters && (
        <Grid
          item
          style={{ order: isSmallScreen ? 7 : 4 }}
          xs={isSmallScreen ? 6 : null}
        >
          {/* Error Type filter */}
          <FormControl className={classes.selectFilterItem}>
            <InputLabel>
              {strings(stringKeys.filters.report.selectErrorType)}
            </InputLabel>
            <Select
              onChange={handleErrorTypeChange}
              value={localFilters.errorType}
            >
              {reportErrorFilterTypes.map((errorType) => (
                <MenuItem value={errorType} key={`errorfilter_${errorType}`}>
                  {strings(stringKeys.filters.report.errorTypes[errorType])}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Grid>
      )}

      {!showCorrectReportFilters && !hideCorrectedFilter && (
        <Grid item style={{ order: 5 }} xs={isSmallScreen ? 6 : null}>
          {/* Is Corrected filter */}
          <FormControl className={classes.selectFilterItem}>
            <InputLabel>
              {strings(stringKeys.filters.report.isCorrected)}
            </InputLabel>
            <Select
              onChange={handleCorrectedStateChange}
              value={localFilters.correctedState}
            >
              {correctedStateTypes.map((state) => (
                <MenuItem value={state} key={`correctedState_${state}`}>
                  {strings(stringKeys.filters.report.correctedStates[state])}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Grid>
      )}

      {!hideTrainingStatusFilter && (
        <Grid
          item
          style={{ order: isSmallScreen ? 2 : 6 }}
          xs={isSmallScreen ? 6 : null}
        >
          {/* Training status filter */}
          <FormControl>
            <FormLabel component="legend">
              {strings(stringKeys.dashboard.filters.trainingStatus)}
            </FormLabel>
            <RadioGroup
              value={localFilters.trainingStatus}
              onChange={handleTrainingStatusChange}
              className={classes.radioGroup}
            >
              <FormControlLabel
                className={classes.radio}
                label={strings(
                  stringKeys.dataCollectors.constants.trainingStatus.Trained,
                )}
                value={"Trained"}
                control={<Radio color="primary" />}
              />
              <FormControlLabel
                className={classes.radio}
                label={strings(
                  stringKeys.dataCollectors.constants.trainingStatus.InTraining,
                )}
                value={"InTraining"}
                control={<Radio color="primary" />}
              />
            </RadioGroup>
          </FormControl>
        </Grid>
      )}

      {showCorrectReportFilters && (
        <Grid
          item
          style={{ order: isSmallScreen ? 4 : 7 }}
          xs={isSmallScreen ? 6 : null}
        >
          <ReportStatusFilter
            filter={localFilters.reportStatus}
            onChange={handleReportStatusChange}
            correctReports={showCorrectReportFilters}
            showDismissedFilter
            doNotWrap
          />
        </Grid>
      )}
    </Grid>
  );
};

export const ReportFilters = ({
  filters,
  healthRisks,
  locations,
  onChange,
  showCorrectReportFilters,
  hideTrainingStatusFilter,
  hideCorrectedFilter,
  rtl,
  isListFetching,
}) => {
  //Reducer for local filters state
  const [localFilters, updateLocalFilters] = useLocalFilters(filters);
  const showUnknownLocation = true;

  //Fetches new data based on changes in filters
  const handleFiltersChange = (filters) => {
    onChange(updateLocalFilters(filters));
  };

  //Syncs locations from redux store with filter state and sets label for location filter to 'All' or "Region (+n)"
  //Neccecary if locations are added, edited or removed, to make all filters checked
  const [locationsFilterLabel] = useLocationFilter(
    locations,
    localFilters,
    updateLocalFilters,
    showUnknownLocation,
  );

  const isMediumScreen = useMediaQuery((theme) => theme.breakpoints.down("sm"));

  if (!localFilters) {
    return null;
  }

  if (isMediumScreen) {
    if (isListFetching) {
      return <LinearProgress color="primary" />;
    }
    return (
      <Grid container justifyContent="center" style={{ marginBottom: 20 }}>
        <DrawerFilter
          title={strings(stringKeys.reports.title)}
          children={
            <Filter
              localFilters={localFilters}
              handleFiltersChange={handleFiltersChange}
              updateLocalFilters={updateLocalFilters}
              healthRisks={healthRisks}
              locations={locations}
              locationsFilterLabel={locationsFilterLabel}
              showCorrectReportFilters={showCorrectReportFilters}
              hideCorrectedFilter={hideCorrectedFilter}
              hideTrainingStatusFilter={hideTrainingStatusFilter}
              rtl={rtl}
            />
          }
          showResults={handleFiltersChange}
        />
      </Grid>
    );
  }

  return (
    <Card>
      <CardContent>
        <Filter
          localFilters={localFilters}
          handleFiltersChange={handleFiltersChange}
          updateLocalFilters={updateLocalFilters}
          healthRisks={healthRisks}
          locations={locations}
          locationsFilterLabel={locationsFilterLabel}
          showCorrectReportFilters={showCorrectReportFilters}
          hideCorrectedFilter={hideCorrectedFilter}
          hideTrainingStatusFilter={hideTrainingStatusFilter}
          rtl={rtl}
        />
      </CardContent>
    </Card>
  );
};
