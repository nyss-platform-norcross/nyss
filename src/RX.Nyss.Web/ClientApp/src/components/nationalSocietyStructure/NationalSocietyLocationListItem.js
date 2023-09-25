import React, { Fragment, useState } from "react";

import { makeStyles } from "@material-ui/core/styles";
import ListItem from "@material-ui/core/ListItem";
import ListItemText from "@material-ui/core/ListItemText";
import Collapse from "@material-ui/core/Collapse";
import ExpandLess from "@material-ui/icons/ExpandLess";
import ExpandMore from "@material-ui/icons/ExpandMore";
import { NationalSocietyLocationList } from "./NationalSocietyLocationList";
import EditIcon from '@material-ui/icons/Edit';
import DeleteIcon from '@material-ui/icons/Delete';
import { IconButton } from "@material-ui/core";
import ListItemSecondaryAction from '@material-ui/core/ListItemSecondaryAction';
import ConfirmationAction from "../common/confirmationAction/ConfirmationAction";
import { strings, stringKeys } from "../../strings";
import { InlineTextEditor } from "../common/InlineTextEditor/InlineTextEditor";

export const NationalSocietyLocationListItem = (props) => {
  const [isEditing, setIsEditing] = useState(false);
  const isCurrentOpen =
    props.activeIndex === `${props.locationType}_${props.location.id}`;
  const isZone = props.locationType === "zone";
  const activeParentLocationId = props.location.id
  const removeLocation = props.manageLocation[props.locationType].remove
  const editLocation = props.manageLocation[props.locationType].edit
  const nextLocation = props.manageLocation[props.locationType].nextLocationType
  const nextLocations = props.manageLocation[props.locationType].nextLocations(props.location)

  const useStyles = makeStyles((theme) => ({
    container: {
      maxHeight: 55,
      display: "flex",
      flexDirection: "row",
      width: "100%",
    },
    row: {
      maxHeight: "100%",
      borderBottom: "1px solid black",
      "&:hover": {
        backgroundColor: nextLocations ? "#FEF1F1" : "none",
      },
      "&:focus": {
        backgroundColor: "#FEF1F1",
      },
    },
    expanded: {
      backgroundColor: "#FEF1F1",
      fontWeight: "bold",
    },
    iconExpanded: {
      transform: "rotate(90deg)",
    },
    text: {
      fontSize: 16,
      color: theme.palette.text.primary,
    },
    icon: {
      fontSize: 36,
      color: "#D52B1E",
    },
    editContainer: {
      display: "flex"
    }
  }));
  const classes = useStyles();

  const handleClick = () => {
    if (isCurrentOpen) {
      props.setActiveIndex("");
    } else {
      props.setActiveIndex(`${props.locationType}_${props.location.id}`);
    }
  };

  const handleRemove = () => {
    removeLocation(props.location.id)
    props.setIsEditingLocations(false)
  }


  const handleEdit = (e) => {
    e.stopPropagation();
    setIsEditing(true);
  }

  const handleSave = (newName) => {
    editLocation(props.location.id, newName);
    setIsEditing(false);
  }


  return (
    <Fragment>
      <div className={classes.container}>
        <ListItem
          className={
            classes.row + " " + (isCurrentOpen ? classes.expanded : "")
          }
          ContainerProps={{
            className: classes.container
          }}
          button={!!nextLocations}
          onClick={!isZone ? handleClick : () => null}
        >
        {!isEditing && (
            <>
            <ListItemText
              disableTypography
              className={classes.text}
              primary={props.location.name}
            />
            {!isZone && !props.isEditingLocations &&
              (isCurrentOpen ? (
                <ExpandLess
                  className={
                    classes.icon +
                    " " +
                    (isCurrentOpen ? classes.iconExpanded : "")
                  }
                />
              ) : (
                <ExpandMore
                  className={
                    classes.icon +
                    " " +
                    (isCurrentOpen ? classes.iconExpanded : "")
                  }
                />
            ))}
            {props.isEditingLocations && (
              <ListItemSecondaryAction className={classes.editContainer}>
                <IconButton size="small" id={`${props.locationType}_${props.location.id}_edit`} onClick={handleEdit}>
                  <EditIcon style={{color: "#D52B1E"}}/>
                </IconButton>
                <ConfirmationAction
                  className={classes.icon}
                  confirmationText={strings(stringKeys.nationalSociety.structure.removalConfirmation)}
                  onClick={handleRemove}>
                    <IconButton size="small" id={`${props.locationType}_${props.location.id}_delete`}>
                      <DeleteIcon style={{color: "#D52B1E"}}/>
                    </IconButton>
                </ConfirmationAction>
              </ListItemSecondaryAction>
            )}
          </>
        )}
        {isEditing && (
          <InlineTextEditor
            initialValue={props.location.name}
            onSave={handleSave}
            autoFocus
            onClose={() => setIsEditing(false)} />
        )}
        </ListItem>
        <Collapse
          in={
            props.activeIndex === `${props.locationType}_${props.location.id}`
          }
          timeout="auto"
          unmountOnExit
        >
          <NationalSocietyLocationList
            locationType={nextLocation}
            locations={nextLocations}
            manageLocation={props.manageLocation}
            activeParentLocationId={activeParentLocationId}
          />
        </Collapse>
      </div>
    </Fragment>
  );
};
