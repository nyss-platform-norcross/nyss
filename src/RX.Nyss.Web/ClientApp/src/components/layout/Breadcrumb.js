import styles from './Breadcrumb.module.scss';

import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import { push } from "connected-react-router";
import { Breadcrumbs, Hidden } from '@material-ui/core';
import NavigateNextIcon from '@material-ui/icons/NavigateNext';
import NavigateBeforeIcon from '@material-ui/icons/NavigateBefore';

const BreadcrumbComponent = ({ breadcrumb, push }) => {
  const directionIsRtl = useSelector(state => state.appData.direction === 'rtl');
  const breadcrumbVisibleItems = breadcrumb.filter(b => !b.hidden);

  const renderBreadcrumb = (items, disableLastItem) => (
    <Breadcrumbs
      className={styles.container}
      separator={directionIsRtl ? <NavigateBeforeIcon fontSize="small" color="primary" /> : <NavigateNextIcon fontSize="small" color="primary" />}>
      {items.map((item, index) => (
        <div key={`breadcrumbItem${item.url}`} className={`${styles.item} ${item.isActive ? styles.selected : ""}`}>
          <div
            onClick={() => (!disableLastItem || index !== items.length - 1) ? push(item.url) : null}
            className={styles.title}>
            {item.title}
          </div>
        </div>
      ))}
    </Breadcrumbs>
  );

  if (!breadcrumbVisibleItems.length) {
    return null;
  }

  return (
    <Fragment>
      <Hidden smDown> {/* full view */}
        {renderBreadcrumb(breadcrumbVisibleItems, true)}
      </Hidden>
      <Hidden mdUp> {/* smaller view */}
        {renderBreadcrumb(breadcrumbVisibleItems.slice(breadcrumbVisibleItems.length - 3, breadcrumbVisibleItems.length - 1), false)}
      </Hidden>
    </Fragment>
  );
};

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
