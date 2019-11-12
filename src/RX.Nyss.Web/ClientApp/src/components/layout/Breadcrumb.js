import styles from './Breadcrumb.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { push } from "connected-react-router";
import Breadcrumbs from '@material-ui/core/Breadcrumbs';
import NavigateNextIcon from '@material-ui/icons/NavigateNext';

const BreadcrumbComponent = ({ breadcrumb, push }) => (
  <Breadcrumbs
    className={styles.container}
    separator={<NavigateNextIcon fontSize="small" color="primary" />}>
    {breadcrumb.map((item, index) => (
      <div key={`breadcrumbItem${item.url}`} className={`${styles.item} ${item.isActive ? styles.selected : ""}`}>
        <div
          onClick={() => index !== breadcrumb.length - 1 ? push(item.url) : null}
          className={styles.title}>
          {item.title}
        </div>
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

const mapDispatchToProps = {
  push: push
};

export const Breadcrumb = connect(mapStateToProps, mapDispatchToProps)(BreadcrumbComponent);
