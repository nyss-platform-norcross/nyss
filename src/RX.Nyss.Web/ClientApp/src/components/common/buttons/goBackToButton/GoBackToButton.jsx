import React from "react";
import { Button, makeStyles } from "@material-ui/core";
import ArrowBackIcon from "@material-ui/icons/ArrowBack";
import ArrowForwardIcon from "@material-ui/icons/ArrowForward";
import { useSelector } from "react-redux";

export const GoBackToButton = ({ onClick, variant, children, size }) => {
  const useRtlDirection = useSelector(
    (state) => state.appData.direction === "rtl",
  );
  const useStyles = makeStyles(() => ({
    icon: {
      marginLeft: useRtlDirection ? 8 : "initial",
      marginRight: useRtlDirection ? -4 : "initial",
    },
  }));
  const classes = useStyles();

  return (
    <Button
      onClick={onClick}
      startIcon={useRtlDirection ? <ArrowForwardIcon /> : <ArrowBackIcon />}
      classes={{
        startIcon: useRtlDirection && classes.icon,
      }}
      variant={variant}
      color="primary"
      size={size}
    >
      {children}
    </Button>
  );
};

export default GoBackToButton;
