import { makeStyles } from "@material-ui/core/styles";
import ListSubheader from "@material-ui/core/ListSubheader";
import List from "@material-ui/core/List";
import { NationalSocietyLocationListItem } from "./NationalSocietyLocationListItem";

export const NationalSocietyLocationList = (props) => {
  const headerHeight = 48;
  const useStyles = makeStyles((theme) => ({
    root: {
      width: "100%",
      maxWidth: 360,
      backgroundColor: theme.palette.background.paper,
    },
    nested: {
      position: "absolute",
      // marginTop: -headerHeight,
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
      border: props.locations.length > 0 ? "1px solid black" : "none",
    },
  }));

  const classes = useStyles();

  let nextLocationType = "";
  switch (props.locationType) {
    case "Regions":
      nextLocationType = "Districs";
      break;
    case "Districs":
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
      <div className={classes.listContainer}>
        {props.locations.map((location, index) => (
          <NationalSocietyLocationListItem
            key={`${props.locationType}_${location.id}`}
            location={location}
            locationType={props.locationType}
            nextLocationType={nextLocationType}
            regions={props.regions}
            districts={props.districts}
            villages={props.villages}
            zones={props.zones}
            hasNextLocation={props.locations[index + 1]}
          />
        ))}
      </div>
    </List>
  );
};
