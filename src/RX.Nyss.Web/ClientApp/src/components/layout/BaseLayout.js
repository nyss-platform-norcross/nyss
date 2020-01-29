import styles from './Layout.module.scss';

import React, { useEffect } from 'react';
import {useMount } from 'react-use';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Loading } from '../common/loading/Loading';
import Typography from '@material-ui/core/Typography';
import Button from "@material-ui/core/Button";
import { push } from 'connected-react-router';
import { StringsSwitcher } from './StringsSwitcher';
import { pageFocused } from '../app/logic/appActions';
import { checkIsIOS, addMaximumScaleToMetaViewport } from '../../utils/disableFormZoom';

const BaseLayoutComponent = ({ appReady, children, moduleError, push, pageFocused }) => {
  useEffect(() => {
    window.addEventListener("focus", handleWindowFocus);
    window.addEventListener("storage", handleWindowStorageChange); // IE fix
    return () => {
      window.removeEventListener("focus", handleWindowFocus)
      window.removeEventListener("storage", handleWindowStorageChange)
    };
  });

  useMount(() => {
    if (checkIsIOS()) {
      addMaximumScaleToMetaViewport();
    }
  });


  const handleWindowFocus = () => pageFocused();
  const handleWindowStorageChange = () => { };

  if (!appReady) {
    return (
      <div className={styles.loader}>
        <Loading />
      </div>);
  }

  return (
    <div className={styles.layout}>
      {moduleError && (
        <div className={styles.centeredLayoutMessage}>
          <Typography variant="h2">
            We're sorry, but there was a problem with accessing the page.
          </Typography>
          <Typography variant="subtitle1">
            {moduleError}
          </Typography>
          <br />
          <Button variant="outlined" color="primary" onClick={() => push("/")}>
            Go back to the main page
          </Button>
        </div>
      )}
      {!moduleError && children}
      <StringsSwitcher />
    </div>
  );
}

const mapStateToProps = (state, ownProps) => ({
  appReady: state.appData.appReady,
  isDevelopment: state.appData.appReady,
  moduleError: state.appData.moduleError || ownProps.authError
});

const mapDispatchToProps = {
  push: push,
  pageFocused: pageFocused
};

BaseLayoutComponent.propTypes = {
  appReady: PropTypes.bool
};

export const BaseLayout = connect(mapStateToProps, mapDispatchToProps)(BaseLayoutComponent);
