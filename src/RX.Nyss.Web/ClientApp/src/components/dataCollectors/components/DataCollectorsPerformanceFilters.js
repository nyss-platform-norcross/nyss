import { useEffect, useReducer } from "react";
import {
  Card,
  CardContent,
  TextField,
  MenuItem,
  Grid,
  Radio,
  RadioGroup,
  FormControlLabel,
  InputLabel,
  useTheme,
  useMediaQuery,
  makeStyles,
} from "@material-ui/core";
import { strings, stringKeys } from "../../../strings";
import useDebounce from "../../../utils/debounce";
import * as roles from "../../../authentication/roles";
import { trainingStatus } from "../logic/dataCollectorsConstants";
import LocationFilter from "../../common/filters/LocationFilter";
import { HealthRiskFilter } from "../../common/filters/HealthRiskFilter";
import useLocalFilters from "../../common/filters/useLocalFilters";
import useLocationFilter from "../../common/filters/useLocationFilter";
import { DrawerFilter } from "../../common/filters/DrawerFilter";

const emptyHealthRisks = [];

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
}));

const Filter = ({
  localFilters,
  updateLocalFilters,
  handleFiltersChange,
  locations,
  locationsFilterLabel,
  healthRisks,
  supervisors,
  userRoles,
  rtl,
  isSmallScreen,
  classes,
}) => {
  const [name, setName] = useReducer(
    (state, action) => {
      if (state.value !== action) {
        return { changed: true, value: action };
      } else {
        return state;
      }
    },
    { value: localFilters.name, changed: false },
  );

  const debouncedName = useDebounce(name, 500);

  const handleNameChange = (event) => setName(event.target.value);

  const handleAreaChange = (newValue) =>
    handleFiltersChange({ locations: newValue, pageNumber: 1 });

  const handleSupervisorChange = (event) =>
    handleFiltersChange({
      supervisorId: event.target.value === 0 ? null : event.target.value,
    });

  const handleTrainingStatusChange = (event) =>
    handleFiltersChange({ trainingStatus: event.target.value });

  const handleHealthRiskChange = (healthRiskIds) =>
    handleFiltersChange({ healthRiskIds, pageNumber: 1 });

  useEffect(() => {
    debouncedName.changed &&
      handleFiltersChange({ name: debouncedName.value });
  }, [debouncedName.value]);

  return (
    <Grid container spacing={2} direction={"row"} alignItems={"flex-start"}>
      <Grid item xs={isSmallScreen ? 6 : null}>
        <TextField
          label={strings(stringKeys.common.name)}
          onChange={handleNameChange}
          value={name.value}
          className={classes.selectFilterItem}
          InputLabelProps={{ shrink: true }}
        />
      </Grid>
      <Grid item xs={isSmallScreen ? 6 : null}>
        <LocationFilter
          allLocations={locations}
          filteredLocations={localFilters.locations}
          filterLabel={locationsFilterLabel}
          onChange={handleAreaChange}
          rtl={rtl}
        />
      </Grid>

      {(healthRisks ?? emptyHealthRisks).length > 0 && (
        <Grid item xs={isSmallScreen ? 6 : null}>
          <HealthRiskFilter
            allHealthRisks={healthRisks ?? emptyHealthRisks}
            filteredHealthRisks={localFilters.healthRiskIds || []}
            onChange={handleHealthRiskChange}
            updateValue={(value) =>
              updateLocalFilters({ healthRiskIds: value.healthRisks || [] })
            }
            rtl={rtl}
          />
        </Grid>
      )}

      {!userRoles.some((r) => r === roles.Supervisor) && (
        <Grid item xs={isSmallScreen ? 6 : null}>
          <TextField
            select
            label={strings(stringKeys.dataCollectors.filters.supervisors)}
            onChange={handleSupervisorChange}
            value={localFilters.supervisorId || 0}
            className={classes.selectFilterItem}
            InputLabelProps={{ shrink: true }}
            SelectProps={{
              MenuProps: {
                PaperProps: {
                  style: {
                    maxWidth: "90%",
                  },
                },
              },
            }}
          >
            <MenuItem value={0}>
              {strings(stringKeys.dataCollectors.filters.supervisorsAll)}
            </MenuItem>

            {supervisors.map((supervisor) => (
              <MenuItem
                key={`filter_supervisor_${supervisor.id}`}
                value={supervisor.id}
              >
                {supervisor.name}
              </MenuItem>
            ))}
          </TextField>
        </Grid>
      )}

      <Grid item xs={isSmallScreen ? 6 : null}>
        <InputLabel>
          {strings(stringKeys.dataCollectors.filters.trainingStatus)}
        </InputLabel>
        <RadioGroup
          value={localFilters.trainingStatus || "All"}
          onChange={handleTrainingStatusChange}
          className={classes.filterRadioGroup}
        >
          {trainingStatus.map((status) => (
            <FormControlLabel
              key={`trainingStatus_filter_${status}`}
              control={<Radio />}
              className={classes.radio}
              label={strings(
                stringKeys.dataCollectors.constants.trainingStatus[status],
              )}
              value={status}
            />
          ))}
        </RadioGroup>
      </Grid>
    </Grid>
  );
};

export const DataCollectorsPerformanceFilters = ({
  onChange,
  filters,
  rtl,
  locations,
  supervisors,
  healthRisks,
  userRoles,
}) => {
  const [localFilters, updateLocalFilters] = useLocalFilters(filters);

  useEffect(() => {
    updateLocalFilters(filters);
  }, [filters]);

  const handleFiltersChange = (filters) =>
    onChange(updateLocalFilters(filters));

  const [locationsFilterLabel] = useLocationFilter(
    locations,
    localFilters,
    updateLocalFilters,
  );

  const theme = useTheme();
  const classes = useStyles();
  const isSmallScreen = useMediaQuery(theme.breakpoints.down("xs"));
  const isMediumScreen = useMediaQuery(theme.breakpoints.down("sm"));

  if (!filters) {
    return null;
  }

  const filterProps = {
    localFilters,
    updateLocalFilters,
    handleFiltersChange,
    locations,
    locationsFilterLabel,
    healthRisks,
    supervisors,
    userRoles,
    rtl,
    isSmallScreen,
    classes,
  };

  if (isMediumScreen) {
    return (
      <Grid container justifyContent="center" style={{ marginBottom: 20 }}>
        <DrawerFilter
          title={strings(stringKeys.dataCollectors.title)}
          children={<Filter {...filterProps} />}
          showResults={handleFiltersChange}
        />
      </Grid>
    );
  }

  return (
    <Card>
      <CardContent>
        <Filter {...filterProps} />
      </CardContent>
    </Card>
  );
};
