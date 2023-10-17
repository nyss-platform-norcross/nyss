import React from 'react';
import { connect } from "react-redux";
import { Header } from './Header';
import { SideMenu } from './SideMenu';
import { BaseLayout } from './BaseLayout';

import styles from './Layout.module.scss';
import { MessagePopup } from './MessagePopup';
import { TabMenu } from './TabMenu';
import ExpandButton from '../common/buttons/expandButton/ExpandButton';
import { expandSideMenu } from '../app/logic/appActions';

const pageContentId = "pageContent";

export const resetPageContentScroll = () => {
  const element = document.getElementById(pageContentId);
  element && element.scrollTo(0, 0)
}

const LayoutComponent = ({ fillPage, children, isSideMenuExpanded, expandSideMenu }) => {
  const handleExpandClick = () => {
    expandSideMenu(!isSideMenuExpanded);
  }

  return (
    <BaseLayout>
      <div className={styles.sideMenu}>
        <SideMenu isExpanded={isSideMenuExpanded}/>
        <ExpandButton onClick={handleExpandClick} isExpanded={isSideMenuExpanded}/>
      </div>
      <div className={styles.mainContent}>

        <div className={`${styles.header}`}>
          <Header />
        </div>

        <div className={`${styles.pageContentContainer} ${fillPage ? styles.fillPage : null}`} id={pageContentId}>
          <div className={`${styles.pageContent} ${fillPage ? styles.fillPage : null}`}>
            <div className={fillPage ? styles.fillPage : null}>
              <TabMenu />
              {children}
            </div>
          </div>
        </div>
      </div>
      <MessagePopup />
    </BaseLayout>
  );
}

const mapStateToProps = state => ({
  isSideMenuExpanded: state.appData.isSideMenuExpanded
});

const mapDispatchToProps = {
  expandSideMenu: expandSideMenu,
};

const Layout = connect(mapStateToProps, mapDispatchToProps)(LayoutComponent);
export default Layout;