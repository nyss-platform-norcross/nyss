import React, { Fragment, useState } from "react";

import { makeStyles } from "@material-ui/core/styles";
import ListItem from "@material-ui/core/ListItem";
import ListItemText from "@material-ui/core/ListItemText";
import Collapse from "@material-ui/core/Collapse";
import ExpandLess from "@material-ui/icons/ExpandLess";
import ExpandMore from "@material-ui/icons/ExpandMore";
import { NationalSocietyLocationList } from "./NationalSocietyLocationList";

export const NationalSocietyLocationListItem = (props) => {
  const useStyles = makeStyles((theme) => ({
    container: {
      maxHeight: 55,
      display: "flex",
      flexDirection: "row",
    },
    row: {
      border: "1px solid black",
      "&:hover": {
        backgroundColor: "#FEF1F1",
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
  }));
  const classes = useStyles();
  const [open, setOpen] = useState(false);

  const handleClick = () => {
    setOpen(!open);
  };

  let nextLocations = [];
  switch (props.nextLocationType) {
    case "Districs":
      nextLocations = props.districts.filter(
        (district) => district.regionId === props.location.id
      );
      break;
    case "Villages":
      nextLocations = props.villages.filter(
        (village) => village.districtId === props.location.id
      );
      break;
    case "Zones":
      nextLocations = props.zones.filter(
        (zone) => zone.villageId === props.location.id
      );
      break;
    default:
      nextLocations = null;
      break;
  }

  const isZones = props.locationType === "Zones";

  return (
    <Fragment>
      <div className={classes.container}>
        <ListItem
          className={classes.row + " " + (open ? classes.expanded : "")}
          button={!!props.nextLocationType && !!nextLocations}
          onClick={!isZones ? handleClick : () => null}
        >
          <ListItemText
            disableTypography
            className={classes.text}
            primary={props.location.name}
          />
          {!isZones &&
            (open ? (
              <ExpandLess
                className={
                  classes.icon + " " + (open ? classes.iconExpanded : "")
                }
                fontSize="large"
              />
            ) : (
              <ExpandMore
                className={
                  classes.icon + " " + (open ? classes.iconExpanded : "")
                }
                fontSize="large"
              />
            ))}
        </ListItem>
        <Collapse in={open} timeout="auto" unmountOnExit>
          <NationalSocietyLocationList
            regions={props.regions}
            districts={props.districts}
            villages={props.villages}
            zones={props.zones}
            locations={nextLocations}
            locationType={props.nextLocationType}
          />
        </Collapse>
      </div>
    </Fragment>
  );
};
