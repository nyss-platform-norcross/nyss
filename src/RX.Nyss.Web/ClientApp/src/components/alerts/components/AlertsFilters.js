import {
  Grid,
  TextField,
  MenuItem,
  Card,
  CardContent,
  useMediaQuery,
  useTheme,
  Typography,
  makeStyles,
} from "@material-ui/core";
import LocationFilter from "../../common/filters/LocationFilter";
import { strings, stringKeys } from "../../../strings";
import { alertStatusFilters } from "../logic/alertsConstants";
import { DatePicker } from "../../forms/DatePicker";
import { convertToLocalDate, convertToUtc } from "../../../utils/date";
import useLocalFilters from "../../common/filters/useLocalFilters";
import useLocationFilter from "../../common/filters/useLocationFilter";
import { DrawerFilter } from "../../common/filters/DrawerFilter";
import { useEffect } from "react";

const useStyles = makeStyles((theme) => ({
  selectFilterItem: {
    width: "150px",
    [theme.breakpoints.down("xs")]: {
      width: "100%",
      maxWidth: "220px",
    },
  },
  filters: {
    boxShadow: "#fff 0px 5px 5px 5px",
  },
}));

export const AlertsFilters = ({
  filters,
  locations,
  healthRisks,
  onChange,
  rtl,
}) => {
  const theme = useTheme();
  const classes = useStyles();
  const isSmallScreen = useMediaQuery(theme.breakpoints.down("xs"));
  const isMediumScreen = useMediaQuery(theme.breakpoints.down("sm"));
  //Reducer for local filters state
  const [localFilters, updateLocalFilters] = useLocalFilters(filters);

  //Fetches new data based on changes in filters
  const handleFiltersChange = (filters) => {
    onChange(updateLocalFilters(filters));
  };

  useEffect(() => {
    updateLocalFilters(filters);
  }, [filters]);

  //Syncs locations from redux store with filter state and sets label for location filter to 'All' or "Region (+n)"
  //Neccecary if locations are added, edited or removed, to make all filters checked
  const [locationsFilterLabel] = useLocationFilter(
    locations,
    localFilters,
    updateLocalFilters,
  );

  const handleLocationChange = (newValue) => {
    handleFiltersChange({ locations: newValue });
  };

  const handleHealthRiskChange = (event) => {
    handleFiltersChange({
      healthRiskId: event.target.value > 0 ? event.target.value : null,
    });
  };

  const handleStatusChange = (event) => {
    handleFiltersChange({ status: event.target.value });
  };

  const handleDateFromChange = (date) =>
    handleFiltersChange({ startDate: convertToUtc(date) });

  const handleDateToChange = (date) =>
    handleFiltersChange({ endDate: convertToUtc(date) });

  if (!localFilters || !healthRisks) {
    return null;
  }

  const Filter = () => {
    return (
      <Grid container spacing={2} direction={"row"} alignItems={"flex-start"}>
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
        <Grid item xs={isSmallScreen ? 6 : null}>
          <LocationFilter
            filteredLocations={localFilters.locations}
            allLocations={locations}
            filterLabel={locationsFilterLabel}
            onChange={handleLocationChange}
            rtl={rtl}
          />
        </Grid>

        <Grid item xs={isSmallScreen ? 6 : null}>
          <TextField
            select
            label={strings(stringKeys.alerts.filters.healthRisks)}
            onChange={handleHealthRiskChange}
            value={localFilters.healthRiskId || 0}
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
              {strings(stringKeys.alerts.filters.healthRisksAll)}
            </MenuItem>

            {healthRisks.map((hr) => (
              <MenuItem key={`filter_healthRisk_${hr.id}`} value={hr.id}>
                <Typography style={{ maxWidth: "100%", whiteSpace: "normal" }}>
                  {hr.name}
                </Typography>
              </MenuItem>
            ))}
          </TextField>
        </Grid>

        <Grid item xs={isSmallScreen ? 6 : null}>
          <TextField
            select
            label={strings(stringKeys.alerts.filters.status)}
            value={localFilters.status || "All"}
            onChange={handleStatusChange}
            className={classes.selectFilterItem}
            InputLabelProps={{ shrink: true }}
          >
            {Object.values(alertStatusFilters).map((status) => (
              <MenuItem key={`filter_status_${status}`} value={status}>
                {strings(stringKeys.alerts.constants.alertStatus[status])}
              </MenuItem>
            ))}
          </TextField>
        </Grid>
      </Grid>
    );
  };

  if (isMediumScreen) {
    return (
      <Grid container justifyContent="center" style={{ marginBottom: 20 }}>
        <DrawerFilter
          title={strings(stringKeys.alerts.title)}
          children={<Filter />}
          showResults={handleFiltersChange}
        />
      </Grid>
    );
  }

  return (
    <Card className={classes.filters}>
      <CardContent>
        <Filter />
      </CardContent>
    </Card>
  );
};
