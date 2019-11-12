import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Snackbar from "@material-ui/core/Snackbar";
import Icon from "@material-ui/core/Icon";
import * as appActions from '../app/logic/appActions';

const MessagePopupComponent = ({ message, closeMessage }) => {
  return (
    <Snackbar
      action={<Icon>close</Icon>}
      open={!!message}
      message={message}
      autoHideDuration={5000}
      onClick={closeMessage}
      onClose={closeMessage}
    />
  );
}

MessagePopupComponent.propTypes = {
  appReady: PropTypes.bool,
  sideMenu: PropTypes.array
};

const mapStateToProps = state => ({
  message: state.appData.message
});

const mapDispatchToProps = {
  closeMessage: appActions.closeMessage
};

export const MessagePopup = connect(mapStateToProps, mapDispatchToProps)(MessagePopupComponent);
