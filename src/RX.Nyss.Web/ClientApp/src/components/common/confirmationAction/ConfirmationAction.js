import styles from "./ConfirmationAction.module.scss";

import React, { Fragment, useState } from "react";
import PropTypes from "prop-types";
import { Menu, MenuItem, ListItemIcon } from "@material-ui/core";

export const ConfirmationAction = ({
  children,
  icon,
  confirmationText,
  onClick,
}) => {
  const [anchorEl, setAnchorEl] = useState(null);

  const handleDropdownClick = (e) => {
    e.stopPropagation();
    setAnchorEl(e.currentTarget);
  };

  const handleDropdownClose = (e) => {
    e && e.stopPropagation();
    setAnchorEl(null);
  };

  const handleMenuClick = (e) => {
    e.stopPropagation();
    onClick();
    handleDropdownClose();
  };

  return (
    <Fragment>
      <div onClick={handleDropdownClick} className={styles.confirmationAction}>
        {children}
      </div>
      <Menu
        anchorEl={anchorEl}
        onClose={handleDropdownClose}
        open={Boolean(anchorEl)}
      >
        <MenuItem
          onClick={handleMenuClick}
          data-automationid="confirmationButton_confirm"
        >
          {icon && <ListItemIcon>{icon}</ListItemIcon>}
          {confirmationText}
        </MenuItem>
      </Menu>
    </Fragment>
  );
};

ConfirmationAction.propTypes = {
  automationid: PropTypes.string,
  canEdit: PropTypes.bool,
  isIcon: PropTypes.bool,
  onClick: PropTypes.func,
  children: PropTypes.any,
};

export default ConfirmationAction;
