import { useState } from "react";
import { makeStyles } from "@material-ui/core/styles";
import ListSubheader from "@material-ui/core/ListSubheader";
import List from "@material-ui/core/List";
import { NationalSocietyLocationListItem } from "./NationalSocietyLocationListItem";
import { Typography, Button} from "@material-ui/core";
import AddIcon from '@material-ui/icons/Add';

export const NationalSocietyLocationList = (props) => {
  const [activeIndex, setActiveIndex] = useState("");

  const headerHeight = 48;
  const borderStyle = props.locations.length > 0 ? "1px solid black" : "1px dashed black";

  const lowerCaseLocationType = props.locationType.toLowerCase();

  const useStyles = makeStyles((theme) => ({
    root: {
      width: "100%",
      maxWidth: 300,
      backgroundColor: theme.palette.background.paper,
    },
    nested: {
      position: "absolute",
      marginTop: -headerHeight - 1,
    },
    header: {
      height: headerHeight,
      minWidth: 200,
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
    }
  }));

  const classes = useStyles();

  let nextLocationType = "";
  switch (props.locationType) {
    case "Regions":
      nextLocationType = "Districts";
      break;
    case "Districts":
      nextLocationType = "Villages";
      break;
    case "Villages":
      nextLocationType = "Zones";
      break;
    default:
      nextLocationType = null;
      break;
  }

  return (
    <List
      className={
        props.locationType === "Regions" ? classes.root : classes.nested
      }
      component="nav"
      aria-labelledby={`${props.locationType}_list`}
      subheader={
        <ListSubheader
          className={classes.header}
          component="div"
          id={`${props.locationType}_list`}
        >
          <div className={classes.background}>{props.locationType}</div>
        </ListSubheader>
      }
    >
      {props.locations.length === 0 && (
        <div className={classes.noLocationsContainer}>
          <div className={classes.noLocationsTextContainer}>
            <Typography>{`No ${lowerCaseLocationType} added`}</Typography>
          </div>
          <Button startIcon={<AddIcon />} className={classes.button} variant="outlined" color="primary">{`Add ${lowerCaseLocationType}`}</Button>
        </div>
      )}
      {props.locations.length > 0 && (
        <div className={classes.listContainer}>
          {props.locations.map((location) => (
            <NationalSocietyLocationListItem
              key={`${props.locationType}_${location.id}`}
              location={location}
              locationType={props.locationType}
              nextLocationType={nextLocationType}
              regions={props.regions}
              districts={props.districts}
              villages={props.villages}
              zones={props.zones}
              activeIndex={activeIndex}
              setActiveIndex={setActiveIndex}
            />
          ))}
        </div>
      )}
    </List>
  );
};
