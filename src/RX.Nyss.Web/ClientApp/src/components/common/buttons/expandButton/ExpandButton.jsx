import React from "react";
import { IconButton, SvgIcon, makeStyles } from '@material-ui/core';
import { ReactComponent as ExpandLeftSVG } from "../../../../assets/icons/Expand-left.svg"

const useStyles = makeStyles({
  triangleBackground: {
    height: 49,
    width: 48,
    padding: 5,
    transform: 'rotate(45deg)',
    backgroundImage: 'linear-gradient(45deg, rgba(255,0,0,0) 50%, #F4F4F4 50%)',
    borderRadius: '15px',
    display: 'flex',
    justifyContent: 'center',
  },
  button: {
    padding: 0,
    transform: 'rotate(-45deg)',
    color: "#D52B1E",
  },
})
 
export const ExpandButton = () => {
  const classes = useStyles()
  return (
    <div className={classes.triangleBackground}>
    <IconButton
      className={classes.button}
    >
       <SvgIcon component={ExpandLeftSVG} viewBox="0 0 48 49"/>
    </IconButton>
    </div>
  );
}

export default ExpandButton;