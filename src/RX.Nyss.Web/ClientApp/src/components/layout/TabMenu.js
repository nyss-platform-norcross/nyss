import styles from './TabMenu.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { push } from "connected-react-router";
import Tabs from '@material-ui/core/Tabs';
import Tab from '@material-ui/core/Tab';

const TabMenuComponent = ({ tabMenu, breadcrumb, push, currentUrl }) => {
  const onItemClick = (item) => {
    push(item.url);
  };

  if (!breadcrumb.length) {
    return null;
  }

  const breadcrumbVisibleItems = breadcrumb.filter(b => !b.hidden);
  const currentBreadcrumbItem = breadcrumbVisibleItems[breadcrumbVisibleItems.length - 1];

  const showTabMenu = tabMenu.some(t => t.url === currentUrl);

  return (
    <div className={styles.tabMenu}>
      <div className={styles.header}>{currentBreadcrumbItem.title}</div>

      {showTabMenu && (
        <Tabs value={tabMenu.indexOf(tabMenu.find(t => t.isActive))}>
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
  currentUrl: state.appData.route.url
});

const mapDispatchToProps = {
  push: push
};

export const TabMenu = connect(mapStateToProps, mapDispatchToProps)(TabMenuComponent);
