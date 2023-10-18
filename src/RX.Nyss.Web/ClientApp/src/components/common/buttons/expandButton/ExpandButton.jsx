import React from "react";
import { IconButton, SvgIcon, makeStyles, useTheme, useMediaQuery } from '@material-ui/core';
import { ReactComponent as ExpandLeftSVG } from "../../../../assets/icons/Expand-left.svg"

const useStyles = makeStyles({
  triangleBackground: {
    position: 'relative',
    zIndex: 2,
    right: 20,
    top: 5,
    height: 35,
    width: 35,
    transform: 'rotate(45deg)',
    backgroundImage: 'linear-gradient(45deg, rgba(255,0,0,0) 50%, #F4F4F4 50%)',
    borderRadius: '10px',
    display: 'flex',
    justifyContent: 'center',
  },
  button: {
    padding: 0,
    transform: 'rotate(135deg)',
    color: "#D52B1E",
  },
  invertedButton: {
    transform: 'rotate(-45deg)',
  },
})

export const ExpandButton = ({ onClick, isExpanded }) => {
  const classes = useStyles();
  const theme = useTheme();
  const isSmallScreen = useMediaQuery(theme.breakpoints.down('md'));
  
  // The expanded button should only appear for larger screens
  if (isSmallScreen) return null;
  return (
    <div className={classes.triangleBackground}>
    <IconButton
      className={`${classes.button} ${isExpanded && classes.invertedButton}`}
      onClick={onClick}
    >
       <SvgIcon style={{fontSize: 18}} component={ExpandLeftSVG} viewBox="0 0 48 49"/>
    </IconButton>
    </div>
  );
}

export default ExpandButton;