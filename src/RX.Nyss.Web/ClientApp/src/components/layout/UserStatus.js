import styles from './UserStatus.module.scss';

import React, { useState, Fragment } from 'react';
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
import { strings, stringKeys } from '../../strings';
import { FeedbackDialog } from '../feedback/FeedbackDialog';

export const UserStatusComponent = ({ user, logout }) => {
  const [anchorEl, setAnchorEl] = useState();
  const [feedbackDialogOpened, setfeedbackDialogOpened] = useState(false);

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
      <FeedbackDialog isOpened={feedbackDialogOpened} close={() => setfeedbackDialogOpened(false)} />
      <Menu
        anchorEl={anchorEl}
        open={!!anchorEl}
        onClose={handleClose}>
        <List className={styles.popover}>
          <ListItem
            onClick={handleClose}
            disabled
            className={styles.authCaption}>
            <ListItemText primary={user.name} secondary={strings(stringKeys.roles[user.roles[0]])} />
          </ListItem>
          <ListItem
            onClick={handleClose}
            disabled
            className={styles.authCaption}>
            <ListItemText secondary={user.email} />
          </ListItem>
          <MenuItem onClick={() => setfeedbackDialogOpened(true)} className={styles.authButton}>
            <Icon className={styles.fontIcon}>feedback</Icon>
            {strings(stringKeys.feedback.send)}
          </MenuItem>
          <MenuItem onClick={logout} className={styles.authButton}>
            <Icon className={styles.fontIcon}>exit_to_app</Icon>
            {strings(stringKeys.user.logout)}
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
