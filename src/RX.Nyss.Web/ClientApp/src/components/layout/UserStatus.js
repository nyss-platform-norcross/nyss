import styles from './UserStatus.module.scss';

import React, { useState } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Menu from "@material-ui/core/Menu";
import MenuItem from "@material-ui/core/MenuItem";
import List from "@material-ui/core/List";
import ListItem from "@material-ui/core/ListItem";
import ListItemText from "@material-ui/core/ListItemText";
import Icon from "@material-ui/core/Icon";
import * as authActions from '../../authentication/authActions';
import ArrowDropDownIcon from '@material-ui/icons/ArrowDropDown';

export const UserStatusComponent = ({ user, logout }) => {
  const [anchorEl, setAnchorEl] = useState();

  const handleClick = (e) => setAnchorEl(e.currentTarget);

  const handleClose = () => setAnchorEl(null);

  if (!user) {
    return null;
  }

  return (
    <div>
      <div className={styles.userStatus} onClick={handleClick}>
        <div className={styles.userName}>{user.name}</div>
        <ArrowDropDownIcon color="primary" className={styles.arrow} />
      </div>

      <Menu
        anchorEl={anchorEl}
        open={!!anchorEl}
        onClose={handleClose}>
        <List className={styles.popover}>
          <ListItem
            onClick={handleClose}
            disabled
            className={styles.authCaption}>
            <ListItemText primary={user.name} secondary={user.name} />
          </ListItem>
          <MenuItem onClick={logout} className={styles.authButton}>
            <Icon className={styles.logoutIcon}>exit_to_app</Icon>
            Log out
          </MenuItem>
        </List>
      </Menu>
    </div>
  );
}

const mapStateToProps = state => ({
  user: state.appData.user
});

const mapDispatchToProps = {
  logout: authActions.logout.invoke
};

UserStatusComponent.propTypes = {
  logout: PropTypes.func
};

export const UserStatus = connect(mapStateToProps, mapDispatchToProps)(UserStatusComponent);
