import styles from './StringsSwitcher.module.scss';

import React, { Fragment, useState } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Button from '@material-ui/core/Button';
import { SendReportDialog } from '../reports/SendReportDialog'
import * as reportsActions from '../reports/logic/reportsActions'

const ReportSenderComponent = (props) => {
  if (!props.isDemo) {
    return null;
  }

  const [open, setOpen] = useState(false);

  const goToSendReport = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setOpen(true);
  }


  return (
    <Fragment>
      <div className={styles.stringSwitcher}>
        <Button key="sendReportButton" variant="text" color="primary" onClick={goToSendReport}>
          Send report
        </Button>
      </div>

      {open &&
        <SendReportDialog close={(e) => { setOpen(false); }} props={props}/>
      }
    </Fragment>
  );
}

ReportSenderComponent.propTypes = {
  appReady: PropTypes.bool,
  sideMenu: PropTypes.array
};

const mapStateToProps = state => ({
  isDemo: state.appData.isDemo,
  isSaving: state.reports.formSaving,
  data: state.reports.formData,
  error: state.reports.formError
});

const mapDispatchToProps = {
  sendReport: reportsActions.sendReport.invoke
};

export const ReportSender = connect(mapStateToProps, mapDispatchToProps)(ReportSenderComponent);
