import { useState } from "react";
import { makeStyles } from "@material-ui/core/styles";
import ListSubheader from "@material-ui/core/ListSubheader";
import List from "@material-ui/core/List";
import { NationalSocietyLocationListItem } from "./NationalSocietyLocationListItem";
import { Typography, Button, Grid } from "@material-ui/core";
import AddIcon from '@material-ui/icons/Add';
import { InlineTextEditor } from "../common/InlineTextEditor/InlineTextEditor";
import * as roles from "../../authentication/roles";
import EditIcon from '@material-ui/icons/Edit';

export const NationalSocietyLocationList = (props) => {
  const [activeIndex, setActiveIndex] = useState("");
  const [isCreatingLocation, setIsCreatingLocation] = useState(false)
  const [isEditingLocations, setIsEditingLocations] = useState(false)

  const headerHeight = 48;
  const hasLocations = props.locations.length > 0
  const borderStyle = hasLocations ? "1px solid black" : "1px dashed black";

  const lowerCaseLocationType = props.locationType.toLowerCase();
  const createLocation = props.manageLocation[props.locationType].create;

  const canModify =
  !props.nationalSocietyIsArchived &&
  (!props.nationalSocietyHasCoordinator ||
    props.callingUserRoles.some(
      (r) => r === roles.Coordinator || r === roles.Administrator
    ));

  const useStyles = makeStyles((theme) => ({
    root: {
      width: "100%",
      maxWidth: 300,
      backgroundColor: theme.palette.background.paper,
    },
    nested: {
      width: "100%",
      position: "absolute",
      marginTop: -headerHeight - 1,
    },
    header: {
      height: headerHeight,
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      fontSize: 20,
      color: theme.palette.text.primary,
    },
    background: {
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      backgroundColor: "#F1F1F1",
      width: "100%",
      borderRadius: "8px",
    },
    listContainer: {
      borderLeft: borderStyle,
      borderRight: borderStyle,
      borderTop: borderStyle,
    },
    noLocationsContainer: {
      display: "flex",
      flexDirection: "column",
    },
    noLocationsTextContainer: {
      border: borderStyle,
      minHeight: 54,
      display: "flex",
      justifyContent: "center",
      alignItems: "center",
      fontSize: 16
    },
    button: {
      marginTop: 10,
      alignSelf: "center",
    },
    addLocationField: {
      marginTop: 10,
      marginRight: 5,
      marginLeft: 5,
      alignSelf: "center",
    }
  }));

  const classes = useStyles();

  return (
    <List
      className={
        props.locationType === "region" ? classes.root : classes.nested
      }
      component="nav"
      aria-labelledby={`${props.locationType}_list`}
      subheader={
        <ListSubheader
          className={classes.header}
          component="div"
          id={`${props.locationType}_list`}
        >
          <div className={classes.background}>{`${props.locationType.charAt(0).toUpperCase() + props.locationType.slice(1)}s`}</div>
        </ListSubheader>
      }
    >
      {hasLocations && (
        <div className={classes.listContainer}>
          {props.locations.map((location) => (
            <NationalSocietyLocationListItem
              key={`${props.locationType}_${location.id}`}
              activeIndex={activeIndex}
              setActiveIndex={setActiveIndex}
              isEditingLocations={isEditingLocations}
              setIsEditingLocations={setIsEditingLocations}
              location={location}
              locationType={props.locationType}
              manageLocation={props.manageLocation}
            />
          ))}
        </div>
      )}
      <Grid container direction="column">
        {!hasLocations && (
          <div className={classes.noLocationsTextContainer}>
            <Typography>{`No ${lowerCaseLocationType} added`}</Typography>
          </div>
        )}
        <Grid container direction="row" justifyContent="space-evenly">
          {canModify && !isEditingLocations && !isCreatingLocation && (
            <>
              {hasLocations && (
                <Grid item>
                  <Button startIcon={<EditIcon />} className={classes.button} variant="outlined" color="primary" onClick={() => setIsEditingLocations(!isEditingLocations)}>{`Edit ${lowerCaseLocationType}`}</Button>
                </Grid>
              )}
              <Grid item>
                <Button startIcon={<AddIcon />} className={classes.button} variant="contained" color="primary" onClick={() => setIsCreatingLocation(!isCreatingLocation)}>{`Add ${lowerCaseLocationType}`}</Button>
              </Grid>
            </>
          )}
          {canModify && isEditingLocations && (
            <Grid item>
              <Button className={classes.button} variant="outlined" color="primary" onClick={() => setIsEditingLocations(!isEditingLocations)}>Cancel</Button>
            </Grid>
          )}
        </Grid>
        {isCreatingLocation && (
          <div className={classes.addLocationField}>
            <InlineTextEditor placeholder={`Add ${lowerCaseLocationType}`} onSave={(name) => createLocation(props.activeParentLocationId, name)} autoFocus setIsModifying={setIsCreatingLocation} />
          </div>
        )}
      </Grid>
    </List>
  );
};
