import React from 'react';
import { Header } from './Header';
import { Breadcrumb } from './Breadcrumb';
import { SideMenu } from './SideMenu';
import { BaseLayout } from './BaseLayout';

import styles from './Layout.module.scss';

export const Layout = ({ children }) => (
  <BaseLayout>
    <div className={styles.sideMenu}>
      <SideMenu />
    </div>
    <div className={styles.mainContent}>

      <div className={`${styles.header}`}>
        <Header />
      </div>

      <div className={styles.pageContentContainer}>
        <div className={styles.pageContent}>
          <Breadcrumb />

          <div>
            {children}
          </div>
        </div>
      </div>
    </div>
  </BaseLayout>
);

export default Layout;