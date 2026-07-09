import React, { Fragment, useState } from "react";

import { makeStyles } from "@material-ui/core/styles";
import ListItem from "@material-ui/core/ListItem";
import ListItemText from "@material-ui/core/ListItemText";
import Collapse from "@material-ui/core/Collapse";
import ExpandMore from "@material-ui/icons/ExpandMore";
import { NationalSocietyLocationList } from "./NationalSocietyLocationList";
import EditIcon from "@material-ui/icons/Edit";
import DeleteIcon from "@material-ui/icons/Delete";
import { IconButton, useTheme } from "@material-ui/core";
import ListItemSecondaryAction from "@material-ui/core/ListItemSecondaryAction";
import ConfirmationAction from "../common/confirmationAction/ConfirmationAction";
import { strings, stringKeys } from "../../strings";
import { InlineTextEditor } from "../common/InlineTextEditor/InlineTextEditor";

export const NationalSocietyLocationListItem = (props) => {
  const [isEditing, setIsEditing] = useState(false);
  const isCurrentOpen =
    props.activeIndex === `${props.locationType}_${props.location.id}`;
  const isZone = props.locationType === "zone";
  const activeParentLocationId = props.location.id;
  const removeLocation = props.manageLocation[props.locationType].remove;
  const editLocation = props.manageLocation[props.locationType].edit;
  const nextLocationType =
    props.manageLocation[props.locationType].nextLocationType;
  const nextLocations = props.manageLocation[props.locationType].nextLocations(
    props.location,
  );

  const theme = useTheme()

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
        backgroundColor: nextLocationType ? "#FEF1F1" : "none",
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
      transform: props.rtl ? "rotate(90deg)" : "rotate(-90deg)",
    },
    text: {
      fontSize: "16px",
      color: theme.palette.text.primary,
      maxWidth: "70%",
      whiteSpace: "pre",
    },
    icon: {
      fontSize: 36,
      color: theme.palette.primary.main,
    },
    editContainer: {
      display: "flex",
      right: props.rtl ? 180 : "none",
    },
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
    removeLocation(props.location.id);
    props.setIsEditingLocations(false);
  };

  const handleEdit = (e) => {
    e.stopPropagation();
    setIsEditing(true);
  };

  const handleSave = (newName) => {
    editLocation(props.location.id, newName);
    setIsEditing(false);
    props.setIsEditingLocations(false);
    props.setActiveIndex(null);
  };

  return (
    <Fragment>
      <div className={classes.container}>
        <ListItem
          className={
            classes.row + " " + (isCurrentOpen ? classes.expanded : "")
          }
          ContainerProps={{
            className: classes.container,
          }}
          button={!!nextLocationType}
          onClick={!isZone ? handleClick : () => null}
        >
          {!isEditing && (
            <>
              <ListItemText
                disableTypography
                className={classes.text}
                primary={props.location.name}
              />
              {!isZone && !props.isEditingLocations && (
                <ExpandMore
                  className={`${classes.icon} ${
                    isCurrentOpen && classes.iconExpanded
                  }`}
                />
              )}
              {props.isEditingLocations &&
                (props.location.canModify ?? props.canModify) && (
                  <ListItemSecondaryAction className={classes.editContainer}>
                    <IconButton size="small" onClick={handleEdit}>
                      <EditIcon style={{ color: theme.palette.primary.main }} />
                    </IconButton>
                    <ConfirmationAction
                      confirmationText={strings(
                        stringKeys.nationalSociety.structure
                          .removalConfirmation,
                      )}
                      onClick={handleRemove}
                    >
                      <IconButton
                        size="small"
                        id={`${props.locationType}_${props.location.id}_delete`}
                      >
                        <DeleteIcon style={{ color: theme.palette.primary.main }} />
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
              onClose={() => setIsEditing(false)}
            />
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
            locationType={nextLocationType}
            locations={nextLocations}
            manageLocation={props.manageLocation}
            activeParentLocationId={activeParentLocationId}
            canModify={props.canModify}
            rtl={props.rtl}
          />
        </Collapse>
      </div>
    </Fragment>
  );
};
