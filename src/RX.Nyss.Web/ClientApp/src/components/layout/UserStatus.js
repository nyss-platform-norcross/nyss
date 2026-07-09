import styles from "./UserStatus.module.scss";
import React, { useState } from "react";
import PropTypes from "prop-types";
import { useDispatch, useSelector, connect } from "react-redux";
import { Menu, MenuItem, ListItemText, Icon } from "@material-ui/core";
import { logout } from "../../authentication/authActions";
import ArrowDropDownIcon from "@material-ui/icons/ArrowDropDown";
import { strings, stringKeys } from "../../strings";
import { push } from "connected-react-router";

export const UserStatusComponent = ({ push }) => {
  const dispatch = useDispatch();
  const [anchorEl, setAnchorEl] = useState();
  const user = useSelector((state) => state.appData.user);

  const handleClick = (e) => setAnchorEl(e.currentTarget);

  const handleClose = () => setAnchorEl(null);

  const handleLogout = () => dispatch(logout.invoke());

  if (!user) {
    return null;
  }

  return (
    <div className={styles.userStatusContainer}>
      <div
        className={`${styles.userStatus} ${
          user.languageCode === "ar" ? styles.rtl : ""
        }`}
        onClick={handleClick}
      >
        <div className={styles.userName}>{user.name}</div>
        <ArrowDropDownIcon color="primary" className={styles.arrow} />
      </div>
      <Menu
        anchorEl={anchorEl}
        open={!!anchorEl}
        onClose={handleClose}
        className={styles.popover}
      >
        <MenuItem onClick={handleClose} disabled className={styles.authCaption}>
          <ListItemText
            primary={user.name}
            secondary={strings(stringKeys.roles[user.roles[0]])}
          />
        </MenuItem>
        <MenuItem onClick={handleClose} disabled className={styles.authCaption}>
          <ListItemText secondary={user.email} />
        </MenuItem>
        <MenuItem
          onClick={() => push("/feedback")}
          className={styles.authButton}
        >
          <Icon
            className={`${styles.fontIcon} ${
              user.languageCode === "ar" ? styles.rtl : ""
            }`}
          >
            feedback
          </Icon>
          {strings(stringKeys.feedback.send)}
        </MenuItem>
        <MenuItem onClick={handleLogout} className={styles.authButton}>
          <Icon
            className={`${styles.fontIcon} ${
              user.languageCode === "ar" ? styles.rtl : ""
            }`}
          >
            exit_to_app
          </Icon>
          {strings(stringKeys.user.logout)}
        </MenuItem>
      </Menu>
    </div>
  );
};

UserStatusComponent.propTypes = {
  logout: PropTypes.func,
  sendFeedback: PropTypes.func,
  isSendingFeedback: PropTypes.bool,
  sendFeedbackResult: PropTypes.string,
};

const mapDispatchToProps = {
  push: push,
};

export const UserStatus = connect(
  null,
  mapDispatchToProps,
)(UserStatusComponent);
