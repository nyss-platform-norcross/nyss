import styles from './UserStatus.module.scss';

import React, { useState } from 'react';
import PropTypes from "prop-types";
import { useSelector } from "react-redux";
import { Menu, MenuItem, ListItemText, Icon } from "@material-ui/core";
import { logout } from '../../authentication/authActions';
import { sendFeedback } from '../app/logic/appActions';
import ArrowDropDownIcon from '@material-ui/icons/ArrowDropDown';
import { FeedbackDialog } from '../feedback/FeedbackDialog';
import { strings, stringKeys } from '../../strings';

export const UserStatusComponent = () => {
  const [anchorEl, setAnchorEl] = useState();
  const [feedbackDialogOpened, setFeedbackDialogOpened] = useState(false);
  const isSendingFeedback = useSelector(state => state.appData.feedback.isSending);
  const sendFeedbackResult = useSelector(state => state.appData.feedback.result);
  const user = useSelector(state => state.appData.user);

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
      <div className={user.languageCode !== 'ar' ? styles.userStatus : `${styles.userStatus} ${styles.rightToLeft}`} onClick={handleClick}>
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
        <MenuItem onClick={logout.invoke} className={styles.authButton}>
          <Icon className={styles.fontIcon}>exit_to_app</Icon>
          {strings(stringKeys.user.logout)}
        </MenuItem>
      </Menu>
      <FeedbackDialog
        isOpened={feedbackDialogOpened}
        close={handleFeedbackDialogClose}
        sendFeedback={sendFeedback.invoke}
        isSending={isSendingFeedback}
        result={sendFeedbackResult} />      
    </div >
  );
}

UserStatusComponent.propTypes = {
  logout: PropTypes.func,
  sendFeedback: PropTypes.func,
  isSendingFeedback: PropTypes.bool,
  sendFeedbackResult: PropTypes.string,  
};

export const UserStatus = UserStatusComponent;
