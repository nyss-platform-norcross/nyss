import { useState } from "react";
import { makeStyles } from "@material-ui/core/styles";
import ListSubheader from "@material-ui/core/ListSubheader";
import List from "@material-ui/core/List";
import { NationalSocietyLocationListItem } from "./NationalSocietyLocationListItem";
import { Typography, Button, Grid } from "@material-ui/core";
import AddIcon from "@material-ui/icons/Add";
import { InlineTextEditor } from "../common/InlineTextEditor/InlineTextEditor";
import EditIcon from "@material-ui/icons/Edit";
import { strings, stringKeys } from "../../strings";

export const NationalSocietyLocationList = (props) => {
  const [activeIndex, setActiveIndex] = useState("");
  const [isCreatingLocation, setIsCreatingLocation] = useState(false);
  const [isEditingLocations, setIsEditingLocations] = useState(false);

  const headerHeight = 48;
  const hasLocations = props.locations?.length > 0;
  const borderStyle = hasLocations ? "1px solid black" : "1px dashed black";

  const lowerCaseLocationType = props.locationType.toLowerCase();
  const createLocation = props.manageLocation[props.locationType].create;

  const useStyles = makeStyles((theme) => ({
    root: {
      width: "100%",
      maxWidth: 250,
      marginBottom: 50,
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
      fontSize: "18px",
      color: theme.palette.text.primary,
    },
    background: {
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      backgroundColor: theme.palette.background.default,
      width: "100%",
      borderRadius: 2,
    },
    listContainer: {
      borderLeft: borderStyle,
      borderRight: borderStyle,
      borderTop: borderStyle,
    },
    noLocationsTextContainer: {
      border: borderStyle,
      minHeight: 54,
      display: "flex",
      justifyContent: "center",
      alignItems: "center",
      fontSize: "16px",
    },
    button: {
      marginTop: 10,
    },
    icon: {
      marginLeft: 8,
      marginRight: -8,
    },
    addLocationField: {
      marginTop: 10,
      marginRight: 5,
      marginLeft: 5,
    },
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
          disableSticky
          className={classes.header}
          component="div"
          id={`${props.locationType}_list`}
        >
          <div className={classes.background}>
            {strings(
              stringKeys.nationalSociety.structure.locationHeader[
                props.locationType
              ],
            )}
          </div>
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
              canModify={props.canModify}
              rtl={props.rtl}
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
          {props.canModify && !isEditingLocations && !isCreatingLocation && (
            <>
              {hasLocations && (
                <Grid item>
                  <Button
                    startIcon={
                      <EditIcon className={`${props.rtl && classes.icon}`} />
                    }
                    className={classes.button}
                    variant="outlined"
                    color="primary"
                    onClick={() => setIsEditingLocations(!isEditingLocations)}
                  >
                    {strings(stringKeys.common.buttons.edit)}
                  </Button>
                </Grid>
              )}
              <Grid item>
                <Button
                  startIcon={
                    <AddIcon className={`${props.rtl && classes.icon}`} />
                  }
                  className={classes.button}
                  variant="contained"
                  color="primary"
                  onClick={() => setIsCreatingLocation(!isCreatingLocation)}
                >
                  {props.manageLocation[props.locationType].addLocationLabel}
                </Button>
              </Grid>
            </>
          )}
          {props.canModify && isEditingLocations && (
            <Grid item>
              <Button
                className={classes.button}
                variant="outlined"
                color="primary"
                onClick={() => setIsEditingLocations(!isEditingLocations)}
              >
                {strings(stringKeys.form.cancel)}
              </Button>
            </Grid>
          )}
        </Grid>
        {isCreatingLocation && (
          <div className={classes.addLocationField}>
            <InlineTextEditor
              placeholder={
                props.manageLocation[props.locationType].addLocationLabel
              }
              onSave={(name) =>
                createLocation(props.activeParentLocationId, name)
              }
              autoFocus
              setIsModifying={setIsCreatingLocation}
            />
          </div>
        )}
      </Grid>
    </List>
  );
};
