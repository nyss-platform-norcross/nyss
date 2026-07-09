import styles from "./StringsSwitcher.module.scss";

import React, { useState } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as appActions from "../app/logic/appActions";
import { Fab, Menu, MenuItem, Tooltip } from "@material-ui/core";
import CodeIcon from "@material-ui/icons/Code";

const StringsSwitcherComponent = ({
  isDevelopment,
  switchStrings,
  showStringsKeys,
  goToTranslations,
}) => {
  const [anchorEl, setAnchorEl] = useState(null);

  if (!isDevelopment) {
    return null;
  }

  const handleClick = (event) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleSwitchKeys = () => {
    switchStrings();
    handleClose();
  };

  const handleGoToTranslations = () => {
    goToTranslations();
    handleClose();
  };

  return (
    <div className={styles.stringSwitcher}>
      <Tooltip title="Developer Tools" placement="left">
        <Fab
          color="primary"
          size="small"
          onClick={handleClick}
          aria-label="developer tools"
          className={styles.fab}
        >
          <CodeIcon />
        </Fab>
      </Tooltip>
      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleClose}
        anchorOrigin={{
          vertical: "top",
          horizontal: "left",
        }}
        transformOrigin={{
          vertical: "bottom",
          horizontal: "right",
        }}
      >
        <MenuItem onClick={handleSwitchKeys}>Switch Keys</MenuItem>
        <MenuItem onClick={handleGoToTranslations}>Translations</MenuItem>
      </Menu>
    </div>
  );
};

StringsSwitcherComponent.propTypes = {
  appReady: PropTypes.bool,
  sideMenu: PropTypes.array,
};

const mapStateToProps = (state) => ({
  isDevelopment: state.appData.isDevelopment,
});

const mapDispatchToProps = {
  switchStrings: appActions.switchStrings,
  goToTranslations: appActions.goToTranslations,
};

export const StringsSwitcher = connect(
  mapStateToProps,
  mapDispatchToProps,
)(StringsSwitcherComponent);