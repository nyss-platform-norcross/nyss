import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Snackbar from "@material-ui/core/Snackbar";
import Icon from "@material-ui/core/Icon";
import * as appActions from '../app/logic/appActions';
import { strings, areStringKeysDisplayed } from '../../strings';

const MessagePopupComponent = ({ message, messageStringKey, messageTime, closeMessage }) => {
  const messageContent = messageStringKey ? strings(messageStringKey) : message;

  return (
    <Snackbar
      action={<Icon>close</Icon>}
      open={!!messageContent}
      message={messageContent}
      autoHideDuration={areStringKeysDisplayed() ? null : (messageTime || 5000)}
      onClick={areStringKeysDisplayed() ? null : closeMessage}
      onClose={areStringKeysDisplayed() ? null : closeMessage}
    />
  );
}

MessagePopupComponent.propTypes = {
  appReady: PropTypes.bool,
  sideMenu: PropTypes.array
};

const mapStateToProps = state => ({
  message: state.appData.message,
  messageStringKey: state.appData.messageStringKey,
  messageTime: state.appData.messageTime
});

const mapDispatchToProps = {
  closeMessage: appActions.closeMessage
};

export const MessagePopup = connect(mapStateToProps, mapDispatchToProps)(MessagePopupComponent);
