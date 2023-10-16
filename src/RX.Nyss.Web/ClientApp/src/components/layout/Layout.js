import React from 'react';
import { Header } from './Header';
import { SideMenu } from './SideMenu';
import { BaseLayout } from './BaseLayout';

import styles from './Layout.module.scss';
import { MessagePopup } from './MessagePopup';
import { TabMenu } from './TabMenu';
import ExpandButton from '../common/buttons/expandButton/ExpandButton';

const pageContentId = "pageContent";

export const resetPageContentScroll = () => {
  const element = document.getElementById(pageContentId);
  element && element.scrollTo(0, 0)
}

export const Layout = ({ fillPage, children }) => {

  return (
    <BaseLayout>
      <div className={styles.sideMenu}>
        <SideMenu />
        <ExpandButton />
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

export default Layout;