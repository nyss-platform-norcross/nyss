import { useEffect, useReducer } from "react";
import { strings, stringKeys } from "../../../strings";
import {
  sexValues,
  trainingStatus,
  deployedMode,
  dataCollectorType,
} from "../logic/dataCollectorsConstants";
import {
  InputLabel,
  RadioGroup,
  FormControlLabel,
  Radio,
  Grid,
  TextField,
  MenuItem,
  Card,
  CardContent,
  useTheme,
  useMediaQuery,
  makeStyles,
} from "@material-ui/core";
import * as roles from "../../../authentication/roles";
import useDebounce from "../../../utils/debounce";
import LocationFilter from "../../common/filters/LocationFilter";
import useLocalFilters from "../../common/filters/useLocalFilters";
import useLocationFilter from "../../common/filters/useLocationFilter";
import { DrawerFilter } from "../../common/filters/DrawerFilter";

const useStyles = makeStyles((theme) => ({
  filterItem: {
    width: "150px",
    [theme.breakpoints.down("xs")]: {
      width: "100%",
      maxWidth: "220px",
    },
  },
  filters: {
    boxShadow: "#fff 0px 5px 5px 5px",
  },
  filterRadioGroup: {
    paddingTop: "5px",
  },
  radio: {
    height: "23px",
  },
  filterLabel: {
    fontWeight: 400,
  },
}));

export const DataCollectorsFilters = ({
  supervisors,
  locations,
  onChange,
  callingUserRoles,
  filters,
  rtl,
}) => {
  //Reducer for local filters state
  const [localFilters, updateLocalFilters] = useLocalFilters(filters);

  const classes = useStyles();

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
  );

  const theme = useTheme();
  const isSmallScreen = useMediaQuery(theme.breakpoints.down("xs"));
  const isMediumScreen = useMediaQuery(theme.breakpoints.down("sm"));

  const updateValue = (change) => {
    handleFiltersChange(change);
  };

  const handleLocationChange = (newValue) => {
    updateValue({ locations: newValue });
  };

  const handleSupervisorChange = (event) =>
    updateValue({
      supervisorId: event.target.value === 0 ? null : event.target.value,
    });

  const handleSexChange = (event) => updateValue({ sex: event.target.value });

  const handleTrainingStatusChange = (event) =>
    updateValue({ trainingStatus: event.target.value });

  const handleDeployedModeChange = (event) =>
    updateValue({ deployedMode: event.target.value });

  const handleDataCollectorTypeChange = (event) =>
    updateValue({
      dataCollectorType: event.target.value === "all" ? null : event.target.value,
    });

  const Filter = () => {
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

    useEffect(() => {
      debouncedName.changed &&
        handleFiltersChange({ name: debouncedName.value });
    }, [debouncedName.value]);

    useEffect(() => {
      debouncedName.changed && updateValue({ name: debouncedName.value });
    }, [debouncedName]);

    return (
      <Grid container spacing={2} direction={"row"} alignItems={"flex-start"}>
        <Grid item xs={isSmallScreen ? 6 : null}>
          <TextField
            label={strings(stringKeys.dataCollectors.filters.name)}
            value={name.value}
            onChange={handleNameChange}
            className={classes.filterItem}
            InputLabelProps={{ shrink: true }}
          ></TextField>
        </Grid>
        <Grid item xs={isSmallScreen ? 6 : null}>
          <LocationFilter
            filteredLocations={localFilters.locations}
            allLocations={locations}
            filterLabel={locationsFilterLabel}
            onChange={handleLocationChange}
            rtl={rtl}
          />
        </Grid>

        {!callingUserRoles.some((r) => r === roles.Supervisor) && (
          <Grid item xs={isSmallScreen ? 6 : null}>
            <TextField
              select
              label={strings(stringKeys.dataCollectors.filters.supervisors)}
              onChange={handleSupervisorChange}
              value={localFilters.supervisorId || 0}
              className={classes.filterItem}
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

        {/* DC sex filter */}
        <Grid item xs={isSmallScreen ? 6 : null}>
          <TextField
            select
            label={strings(stringKeys.dataCollectors.filters.sex)}
            onChange={handleSexChange}
            value={localFilters.sex || "all"}
            className={classes.filterItem}
            InputLabelProps={{ shrink: true }}
          >
            <MenuItem value="all">
              {strings(stringKeys.dataCollectors.filters.sexAll)}
            </MenuItem>

            {sexValues.map((sex) => (
              <MenuItem key={`datacollector_filter_${sex}`} value={sex}>
                {strings(
                  stringKeys.dataCollectors.constants.sex[sex.toLowerCase()],
                )}
              </MenuItem>
            ))}
          </TextField>
        </Grid>

        <Grid item xs={isSmallScreen ? 6 : null}>
          <TextField
            select
            label={strings(stringKeys.dataCollectors.list.dataCollectorType)}
            onChange={handleDataCollectorTypeChange}
            value={localFilters.dataCollectorType || "all"}
            className={classes.filterItem}
            InputLabelProps={{ shrink: true }}
          >
            <MenuItem value="all">
              {strings(stringKeys.dataCollectors.constants.trainingStatus.All)}
            </MenuItem>
            <MenuItem value={dataCollectorType.human}>
              {strings(
                stringKeys.dataCollectors.constants.dataCollectorType.Human,
              )}
            </MenuItem>
            <MenuItem value={dataCollectorType.collectionPoint}>
              {strings(
                stringKeys.dataCollectors.constants.dataCollectorType
                  .CollectionPoint,
              )}
            </MenuItem>
          </TextField>
        </Grid>

        {/* Training status filter */}
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

        <Grid item xs={isSmallScreen ? 6 : null}>
          <InputLabel>
            {strings(stringKeys.dataCollectors.filters.deployedMode)}
          </InputLabel>
          <RadioGroup
            value={localFilters.deployedMode}
            onChange={handleDeployedModeChange}
            className={classes.filterRadioGroup}
          >
            {deployedMode.map((status) => (
              <FormControlLabel
                key={`deployedMode_filter_${status}`}
                control={<Radio />}
                className={classes.radio}
                label={strings(
                  stringKeys.dataCollectors.constants.deployedMode[status],
                )}
                value={status}
              />
            ))}
          </RadioGroup>
        </Grid>
      </Grid>
    );
  };

  if (isMediumScreen) {
    return (
      !!localFilters && (
        <Grid container justifyContent="center" style={{ marginBottom: 20 }}>
          <DrawerFilter
            title={strings(stringKeys.dataCollectors.title)}
            children={<Filter />}
            showResults={handleFiltersChange}
          />
        </Grid>
      )
    );
  }

  return (
    !!localFilters && (
      <Card className={classes.filters}>
        <CardContent>
          <Filter />
        </CardContent>
      </Card>
    )
  );
};
