import React, { Fragment, useState } from "react";
import PropTypes from "prop-types";
import Menu from "@material-ui/core/Menu";
import MenuItem from "@material-ui/core/MenuItem";
import ListItemIcon from "@material-ui/core/ListItemIcon";

export const ConfirmationAction = ({ children, icon, confirmationText, onClick }) => {
  const [anchorEl, setAnchorEl] = useState(null);

  const handleDropdownClick = (e) => {
    e.stopPropagation();
    setAnchorEl(e.currentTarget);
  }

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
      <div onClick={handleDropdownClick}>
        {children}
      </div>
      <Menu
        anchorEl={anchorEl}
        onClose={handleDropdownClose}
        open={Boolean(anchorEl)}
      >
        <MenuItem onClick={handleMenuClick} data-automationid="confirmationButton_confirm">
          {icon && <ListItemIcon>{icon}</ListItemIcon>}
          {confirmationText}
        </MenuItem>
      </Menu>
    </Fragment>
  );
};

ConfirmationAction.propTypes = {
  automationid: PropTypes.string,
  confirmationText: PropTypes.string,
  canEdit: PropTypes.bool,
  isIcon: PropTypes.bool,
  onClick: PropTypes.func,
  children: PropTypes.any
};

export default ConfirmationAction;