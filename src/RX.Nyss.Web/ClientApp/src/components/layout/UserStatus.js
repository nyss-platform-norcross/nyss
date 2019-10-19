import styles from './UserStatus.module.scss';

import React, { useState } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Menu from "@material-ui/core/Menu";
import MenuItem from "@material-ui/core/MenuItem";
import List from "@material-ui/core/List";
import ListItem from "@material-ui/core/ListItem";
import ListItemText from "@material-ui/core/ListItemText";
import * as authActions from '../../authentication/authActions';

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
        {user.name}
      </div>

      <Menu
        anchorEl={anchorEl}
        open={!!anchorEl}
        onClose={handleClose}
        className={styles.authButton}>
        <List>
          <ListItem
            onClick={handleClose}
            disabled
            className={styles.authCaption}>
            <ListItemText primary={user.name} secondary={user.unique_name} />
          </ListItem>
          <MenuItem onClick={logout}>
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
  logout: authActions.logout
};

UserStatusComponent.propTypes = {
  logout: PropTypes.func
};

export const UserStatus = connect(mapStateToProps, mapDispatchToProps)(UserStatusComponent);
