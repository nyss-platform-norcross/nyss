import { Grid, Typography, makeStyles, useMediaQuery } from "@material-ui/core";
import { strings, stringKeys } from "../../strings";
import { RcIcon } from "../icons/RcIcon";

const useStyles = makeStyles((theme) => ({
  mapExpanded: {
    transition: "0.3s",
  },
  mapCollapsed: {
    transition: "0.3s",
  },
  mapButton: {
    fontSize: "2rem",
    position: "absolute",
    zIndex: 1000,
    right: "4%",
    top: -15,
    "&:hover": {
      cursor: "pointer",
    },
  },
}));

export const MapAndDashboardNumbers = ({
  DashboardNumbers,
  DashboardReportsMap,
  isMapExpanded,
  setIsMapExpanded,
}) => {
  const classes = useStyles();
  const isSmallScreen = useMediaQuery((theme) => theme.breakpoints.down("sm"));

  if (isSmallScreen) {
    return (
      <>
        <Grid item xs={12}>
          {DashboardNumbers}
        </Grid>
        <Grid item style={{ marginBottom: -20 }}>
          <Typography variant="h5">
            {strings(stringKeys.dashboard.map.title)}
          </Typography>
        </Grid>
        <Grid item xs={12}>
          <div
            style={{ height: "100%", width: isSmallScreen ? "100%" : "95%" }}
          >
            {DashboardReportsMap}
          </div>
        </Grid>
      </>
    );
  }
  return (
    <>
      <Grid item style={{ marginBottom: -20 }}>
        <Typography variant="h5">
          {strings(stringKeys.dashboard.map.title)}
        </Typography>
      </Grid>
      <Grid container item xs={12}>
        <Grid
          item
          xs={12}
          md={isMapExpanded ? 8 : 5}
          style={{ position: "relative" }}
          className={isMapExpanded ? classes.mapExpanded : classes.mapCollapsed}
        >
          <RcIcon
            className={classes.mapButton}
            onClick={() => setIsMapExpanded((prev) => !prev)}
            icon={isMapExpanded ? "Collapse" : "Expand"}
          />
          <div
            style={{ height: "100%", width: isSmallScreen ? "100%" : "95%" }}
          >
            {DashboardReportsMap}
          </div>
        </Grid>
        <Grid
          item
          xs={12}
          md={isMapExpanded ? 4 : 7}
          className={isMapExpanded ? classes.mapExpanded : classes.mapCollapsed}
        >
          {DashboardNumbers}
        </Grid>
      </Grid>
    </>
  );
};
