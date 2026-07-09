import React from "react";
import {
  IconButton,
  SvgIcon,
  makeStyles,
  useTheme,
  useMediaQuery,
} from "@material-ui/core";
import { ReactComponent as ExpandLeftSVG } from "../../../../assets/icons/Expand-left.svg";
import { useSelector } from "react-redux";

export const ExpandButton = ({ onClick, isExpanded }) => {
  const theme = useTheme();
  const isSmallScreen = useMediaQuery(theme.breakpoints.down("md"));
  const useRtlDirection = useSelector(
    (state) => state.appData.direction === "rtl",
  );

  const useStyles = makeStyles((theme) => ({
    triangleBackground: {
      position: "absolute",
      zIndex: 1201,
      right: useRtlDirection ? undefined : -10,
      left: useRtlDirection ? -10 : undefined,
      top: 5,
      height: 35,
      width: 35,
      transform: useRtlDirection ? "rotate(225deg)" : "rotate(45deg)",
      backgroundImage: useRtlDirection
        ? `linear-gradient(-315deg, rgba(255,0,0,0) 50%, ${theme.palette.backgroundDark.main} 50%)`
        : `linear-gradient(45deg, rgba(255,0,0,0) 50%, ${theme.palette.backgroundDark.main} 50%)`,
      borderRadius: "10px",
      display: "flex",
      justifyContent: "center",
    },
    button: {
      padding: 0,
      transform: useRtlDirection
        ? "rotate(-225deg) translateX(-5px)"
        : "rotate(135deg) translateX(-5px)",
      color: theme.palette.primary.main,
      "&:hover": {
        backgroundColor: "transparent",
      },
    },
    invertedButton: {
      transform: "rotate(-45deg)",
    },
  }));
  const classes = useStyles();

  // The expanded button should only appear for larger screens
  if (isSmallScreen) return null;
  return (
    <div className={classes.triangleBackground}>
      <IconButton
        className={`${classes.button} ${isExpanded && classes.invertedButton}`}
        onClick={onClick}
      >
        <SvgIcon
          style={{ fontSize: 18 }}
          component={ExpandLeftSVG}
          viewBox="0 0 48 49"
        />
      </IconButton>
    </div>
  );
};

export default ExpandButton;
