import React from 'react';
import { Header } from './Header';
import { Breadcrumb } from './Breadcrumb';
import { SideMenu } from './SideMenu';
import { BaseLayout } from './BaseLayout';

import styles from './Layout.module.scss';
import { MessagePopup } from './MessagePopup';
import { TabMenu } from './TabMenu';

const pageContentId = "pageContent";

export const resetPageContentScroll = () =>
  document.getElementById(pageContentId).scrollTo(0, 0)

export const Layout = ({ children }) => (
  <BaseLayout>
    <div className={styles.sideMenu}>
      <SideMenu />
    </div>
    <div className={styles.mainContent}>

      <div className={`${styles.header}`}>
        <Header />
      </div>

      <div className={styles.pageContentContainer} id={pageContentId}>
        <div className={styles.pageContent}>
          <Breadcrumb />

          <div>
            <TabMenu />
            {children}
          </div>
        </div>
      </div>
    </div>
    <MessagePopup />
  </BaseLayout>
);

export default Layout;