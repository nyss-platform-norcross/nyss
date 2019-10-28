import styles from './Breadcrumb.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Breadcrumbs from '@material-ui/core/Breadcrumbs';
import NavigateNextIcon from '@material-ui/icons/NavigateNext';

const BreadcrumbComponent = ({ breadcrumb }) => (
  <Breadcrumbs
    className={styles.container}
    separator={<NavigateNextIcon fontSize="small" color="primary" />}>
    {breadcrumb.map((item, index) => (
      <div key={`breadcrumbItem${index}`} className={`${styles.item} ${item.isActive ? styles.selected : ""}`}>
        <div className={styles.title}>{item.title}</div>
      </div>
    ))}
  </Breadcrumbs>
);

BreadcrumbComponent.propTypes = {
  appReady: PropTypes.bool,
  siteMap: PropTypes.object
};

const mapStateToProps = state => ({
  breadcrumb: state.appData.siteMap.breadcrumb
});

export const Breadcrumb = connect(mapStateToProps)(BreadcrumbComponent);
