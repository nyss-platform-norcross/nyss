import styles from './StringsSwitcher.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as appActions from '../app/logic/appActions';
import Button from '@material-ui/core/Button';

const ReportSenderComponent = ({ isDemo, goToSendReport }) => {
  if (!isDemo) {
    return null;
  }

  return (
    <div className={styles.stringSwitcher}>
      <Button key="sendReportButton" variant="text" color="primary" onClick={() => goToSendReport()}>
        Send report
      </Button>
    </div>
  );
}

ReportSenderComponent.propTypes = {
  appReady: PropTypes.bool,
  sideMenu: PropTypes.array
};

const mapStateToProps = state => ({
  isDemo: state.appData.isDemo
});

const mapDispatchToProps = {
  goToSendReport: appActions.goToSendReport
};

export const ReportSender = connect(mapStateToProps, mapDispatchToProps)(ReportSenderComponent);
