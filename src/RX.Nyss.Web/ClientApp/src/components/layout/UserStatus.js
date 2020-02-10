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
import { FeedbackDialog } from '../feedback/FeedbackDialog';

export const UserStatusComponent = ({ user, logout, sendFeedback, isSendingFeedback }) => {
  const [anchorEl, setAnchorEl] = useState();
  const [feedbackDialogOpened, setFeedbackDialogOpened] = useState(false);

  const handleClick = (e) => setAnchorEl(e.currentTarget);
  const handleClose = () => setAnchorEl(null);
  const handleFeedbackDialogClose = () => {
    setFeedbackDialogOpened(false);
    handleClose();
  }

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
        <MenuItem onClick={() => setFeedbackDialogOpened(true)} className={styles.authButton}>
          <Icon className={styles.fontIcon}>feedback</Icon>
          {strings(stringKeys.feedback.send)}
        </MenuItem>
        <MenuItem onClick={logout} className={styles.authButton}>
          <Icon className={styles.fontIcon}>exit_to_app</Icon>
          {strings(stringKeys.user.logout)}
        </MenuItem>
      </Menu>
      <FeedbackDialog
        isOpened={feedbackDialogOpened}
        close={handleFeedbackDialogClose}
        sendFeedback={sendFeedback}
        isSending={isSendingFeedback} />
    </div >
  );
}

UserStatusComponent.propTypes = {
  logout: PropTypes.func,
  sendFeedback: PropTypes.func,
  isSendingFeedback: PropTypes.bool,
};

const mapStateToProps = state => ({
  user: state.appData.user,
  isSendingFeedback: state.appData.feedback.isSending
});

const mapDispatchToProps = {
  logout: authActions.logout.invoke,
  sendFeedback: appActions.sendFeedback.invoke
};


export const UserStatus = connect(mapStateToProps, mapDispatchToProps)(UserStatusComponent);
