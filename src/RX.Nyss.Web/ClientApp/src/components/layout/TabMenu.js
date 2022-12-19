import styles from './TabMenu.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { push } from "connected-react-router";
import { Tabs, Tab } from '@material-ui/core';

const TabMenuComponent = ({ tabMenu, breadcrumb, push, currentUrl, title }) => {
  const onItemClick = (item) => {
    push(item.url);
  };

  if (!breadcrumb.length) {
    return null;
  }

  const breadcrumbVisibleItems = breadcrumb.filter(b => !b.hidden);
  const currentBreadcrumbItem = breadcrumbVisibleItems[breadcrumbVisibleItems.length - 1];

  // http addresses are case insensitive so compare to-lower versions
  const showTabMenu = tabMenu.some(t => t.url.toLowerCase() === currentUrl.toLowerCase());

  return (
    <div className={styles.tabMenu}>
      <div className={styles.header}>{title || currentBreadcrumbItem.title}</div>

      {showTabMenu && (
        <Tabs
          value={tabMenu.indexOf(tabMenu.find(t => t.isActive))}
          className={styles.tabs}
          scrollButtons="auto"
          indicatorColor="primary"
          variant="scrollable">
          {tabMenu.map(item => (
            <Tab key={`tabMenu_${item.url}`} label={item.title} onClick={() => onItemClick(item)} />
          ))}
        </Tabs>
      )}
    </div>
  );
}

TabMenuComponent.propTypes = {
  appReady: PropTypes.bool,
  tabMenu: PropTypes.array
};

const mapStateToProps = state => ({
  tabMenu: state.appData.siteMap.tabMenu,
  breadcrumb: state.appData.siteMap.breadcrumb,
  title: state.appData.siteMap.title,
  currentUrl: state.appData.route.url
});

const mapDispatchToProps = {
  push: push
};

export const TabMenu = connect(mapStateToProps, mapDispatchToProps)(TabMenuComponent);
