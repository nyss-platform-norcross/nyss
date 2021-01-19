import styles from './Layout.module.scss';

import React, { useEffect } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Loading } from '../common/loading/Loading';
import Typography from '@material-ui/core/Typography';
import Button from "@material-ui/core/Button";
import { push } from 'connected-react-router';
import { StringsSwitcher } from './StringsSwitcher';
import { pageFocused } from '../app/logic/appActions';
import { checkIsIOS, addMaximumScaleToMetaViewport } from '../../utils/disableFormZoom';
import { useMount } from '../../utils/lifecycle';
import { stringKeys, strings } from '../../strings';

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
            {strings(stringKeys.error.errorPage.message)}
          </Typography>
          <Typography variant="subtitle1">
            {strings(moduleError)}
          </Typography>
          <br />
          <Button variant="outlined" color="primary" onClick={() => push("/")}>
            {strings(stringKeys.error.errorPage.goHome)}
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
