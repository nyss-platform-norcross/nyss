import React, { useState, useRef } from "react";
import Drawer from "@material-ui/core/Drawer";
import Button from "@material-ui/core/Button";
import Backdrop from "@material-ui/core/Backdrop";
import { Typography, makeStyles, Grid } from "@material-ui/core";
import FilterListIcon from "@material-ui/icons/FilterList";
import { strings, stringKeys } from "../../../strings";

const useStyles = makeStyles((theme) => ({
  pullTabContainer: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    borderBottom: `1px solid ${theme.palette.grey[300]}`,
    boxShadow: `0px 4px 4px -6px ${theme.palette.grey[500]}`,
    borderRadius: theme.shape.borderRadius,
  },
  pullTab: {
    height: 5,
    borderRadius: 8,
    width: 50,
    backgroundColor: theme.palette.grey[400],
    margin: "8px 0px",
  },
  title: {
    marginBottom: 10,
    marginRight: 3,
  },
  contentContainer: {
    position: "relative",
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    padding: "30px 10px",
    overflowX: "hidden",
    maxHeight: 350,
    overflowY: "auto",
    [theme.breakpoints.down("xs")]: {
      padding: "20px 10px",
    },
  },
  resultsContainer: {
    display: "flex",
    flexDirection: "row",
    justifyContent: "center",
    padding: 15,
    height: 40,
    borderRadius: theme.shape.borderRadius,
    borderTop: `1px solid ${theme.palette.grey[300]}`,
    boxShadow: `0px 4px 4px 2px ${theme.palette.grey[500]}`,
  },
  backdrop: {
    zIndex: theme.zIndex.drawer - 1,
    backgroundColor: "rgba(0, 0, 0, 0.5)",
  },
  backdropClass: {
    backgroundColor: "transparent",
  },
}));

export const DrawerFilter = ({ title, children, showResults }) => {
  const [isOpen, setIsOpen] = useState(false);
  const classes = useStyles();
  const [startY, setStartY] = useState(null);
  const drawerRef = useRef(null);

  const onShowResults = () => {
    setIsOpen(false);
    showResults();
  };

  const handleTouchStart = (event) => {
    setStartY(event.touches[0].clientY);
  };

  const handleTouchMove = (event) => {
    if (startY !== null) {
      const currentY = event.touches[0].clientY;
      const deltaY = currentY - startY;

      if (deltaY > 0 && drawerRef.current && drawerRef.current.style) {
        drawerRef.current.style.transform = `translateY(${deltaY}px)`;
      }
    }
  };
  const handleDrawerTransitionEnd = () => {
    if (!isOpen && drawerRef.current && drawerRef.current.style) {
      drawerRef.current.style.transition = ""; // Reset transition property
      drawerRef.current.style.transform = ""; // Reset transform property
    }
  };

  const handleTouchEnd = (event) => {
    if (startY !== null) {
      // You can adjust the threshold for closing the drawer as needed
      const threshold = 200;
      const currentY = event.changedTouches[0].clientY;
      const deltaY = currentY - startY;

      if (deltaY > threshold) {
        // Close the drawer when dragging down
        onShowResults();
      } else {
        // Reset styles if not closing
        if (drawerRef.current && drawerRef.current.style) {
          drawerRef.current.style.transform = "";
        }
      }
      setStartY(null);
    }
  };

  const toggleDrawer = (open) => {
    setIsOpen(open);
  };

  return (
    <>
      <Button
        startIcon={<FilterListIcon />}
        color="primary"
        variant="outlined"
        onClick={() => toggleDrawer(true)}
      >
        {strings(stringKeys.common.filters)}
      </Button>
      <Drawer
        ref={drawerRef}
        anchor={"bottom"}
        open={isOpen}
        onTransitionEnd={handleDrawerTransitionEnd}
        onClose={onShowResults}
        BackdropProps={{
          classes: {
            root: classes.backdropClass,
          },
        }}
        ModalProps={{
          keepMounted: true,
        }}
      >
        <div
          className={classes.pullTabContainer}
          onTouchStart={handleTouchStart}
          onTouchMove={handleTouchMove}
          onTouchEnd={handleTouchEnd}
        >
          <div className={classes.pullTab} />
          <Grid container justifyContent="center">
            <Typography variant="h6" className={classes.title}>
              {strings(stringKeys.common.filter)}
            </Typography>
            <Typography variant="h6" style={{ textTransform: "lowercase" }}>
              {title}
            </Typography>
          </Grid>
        </div>
        <div role="presentation" className={classes.contentContainer}>
          {children}
        </div>
        <div className={classes.resultsContainer}>
          <Button variant="contained" color="primary" onClick={onShowResults}>
            {strings(stringKeys.common.buttons.showResults)}
          </Button>
        </div>
      </Drawer>
      <Backdrop
        className={classes.backdrop}
        open={isOpen}
        onClick={onShowResults}
      />
    </>
  );
};
