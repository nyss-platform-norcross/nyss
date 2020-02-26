import styles from './UserStatus.module.scss';

import React, { useState } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Menu from "@material-ui/core/Menu";
import MenuItem from "@material-ui/core/MenuItem";
import ListItemText from "@material-ui/core/ListItemText";
import Icon from "@material-ui/core/Icon";
import * as authActions from '../../authentication/authActions';
import * as appActions from '../app/logic/appActions';
import ArrowDropDownIcon from '@material-ui/icons/ArrowDropDown';
import { strings, stringKeys } from '../../strings';
import * as roles from '../../authentication/roles';

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
        onClose={handleClose}
        className={styles.popover}>
        <MenuItem
          onClick={handleClose}
          disabled
          className={styles.authCaption}>
          <ListItemText primary={user.name} secondary={strings(stringKeys.roles[user.roles[0]])} />
        </MenuItem>
        <MenuItem
          onClick={handleClose}
          disabled
          className={styles.authCaption}>
          <ListItemText secondary={user.email} />
        </MenuItem>
        <MenuItem onClick={logout} className={styles.authButton}>
          <Icon className={styles.fontIcon}>exit_to_app</Icon>
          {strings(stringKeys.user.logout)}
        </MenuItem>
      </Menu>
    </div >
  );
}

UserStatusComponent.propTypes = {
  logout: PropTypes.func
};

const mapStateToProps = state => ({
  user: state.appData.user
});

const mapDispatchToProps = {
  logout: authActions.logout.invoke
};


export const UserStatus = connect(mapStateToProps, mapDispatchToProps)(UserStatusComponent);
