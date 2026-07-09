import styles from "./LocationFilter.module.scss";

import { Fragment, useState } from "react";
import { Popover, TextField, useMediaQuery } from "@material-ui/core";
import ArrowDropDown from "@material-ui/icons/ArrowDropDown";
import { makeStyles } from "@material-ui/core/styles";

const useStyles = makeStyles({
  popoverPaper: {
    "&.MuiPopover-paper": {
      width: "100%",
      maxWidth: 400,
    },
  },
});

export const DropdownPopover = ({
  children,
  label,
  filterLabel,
  showResults,
  rtl,
  dialogOpen,
  setDialogOpen,
}) => {
  const classes = useStyles();
  const [anchorEl, setAnchorEl] = useState(null);
  const isSmallScreen = useMediaQuery((theme) => theme.breakpoints.down("sm"));

  const handleDropdownClick = (event) => {
    event.preventDefault();
    setAnchorEl(event.currentTarget);
    setDialogOpen(true);
  };

  const handleKeyUp = (event) => {
    if (event.key === "Enter" || event.key === " ") handleDropdownClick(event);
  };

  return (
    <Fragment>
      <TextField
        label={label}
        InputProps={{
          readOnly: true,
          endAdornment: <ArrowDropDown className={styles.arrow} />,
        }}
        value={filterLabel}
        inputProps={{
          className: styles.clickable,
        }}
        onClick={handleDropdownClick}
        onKeyUp={handleKeyUp}
      />
      <Popover
        open={dialogOpen}
        onClose={() => {
          setDialogOpen(false);
          showResults();
        }}
        anchorEl={anchorEl}
        anchorOrigin={{
          vertical: "bottom",
          horizontal: rtl ? "right" : "left",
        }}
        transformOrigin={{
          vertical: "top",
          horizontal: rtl ? "right" : "left",
        }}
        PaperProps={{
          className: styles.filterContainer,
        }}
        classes={{ paper: classes.popoverPaper }}
        style={{ maxHeight: 400, maxWidth: "90%" }}
      >
        {children}
      </Popover>
    </Fragment>
  );
};
