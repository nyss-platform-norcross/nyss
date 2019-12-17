import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Snackbar from "@material-ui/core/Snackbar";
import Icon from "@material-ui/core/Icon";
import * as appActions from '../app/logic/appActions';
import { areStringKeysDisplayed, extractString } from '../../strings';

const MessagePopupComponent = ({ messageKey, requestErrorMessageKey, messageTime, closeMessage }) => {
  const messageContent = requestErrorMessageKey
    ? extractString(requestErrorMessageKey)
    : messageKey
      ? extractString(messageKey)
      : null;

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
  requestErrorMessageKey: state.requests.errorMessage,
  messageKey: state.appData.messageKey,
  messageTime: state.appData.messageTime
});

const mapDispatchToProps = {
  closeMessage: appActions.closeMessage
};

export const MessagePopup = connect(mapStateToProps, mapDispatchToProps)(MessagePopupComponent);
