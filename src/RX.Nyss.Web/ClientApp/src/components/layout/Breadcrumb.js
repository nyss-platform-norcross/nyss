import styles from './Breadcrumb.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Breadcrumbs from '@material-ui/core/Breadcrumbs';
import NavigateNextIcon from '@material-ui/icons/NavigateNext';
import { getBreadcrumb } from '../../siteMap';

const BreadcrumbComponent = ({ siteMap }) => {
  if (!siteMap.path) {
    return null;
  }

  const breakcrumb = getBreadcrumb(siteMap.path, siteMap.parameters);

  return (
    <Breadcrumbs
      className={styles.container}
      separator={<NavigateNextIcon fontSize="small" color="primary" />}>
      {breakcrumb.map((item, index) => (
        <div key={`breadcrumbItem${index}`} className={`${styles.item} ${siteMap.path === item.path ? styles.selected : ""}`}>
          <div className={styles.title}>{item.title}</div>
        </div>
      ))}
    </Breadcrumbs>
  );
}

BreadcrumbComponent.propTypes = {
  appReady: PropTypes.bool,
  siteMap: PropTypes.object
};

const mapStateToProps = state => ({
  siteMap: state.appData.siteMap
});

export const Breadcrumb = connect(mapStateToProps)(BreadcrumbComponent);
