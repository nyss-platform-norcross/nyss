import React from 'react';
import { Header } from './Header';
import { Breadcrumb } from './Breadcrumb';
import { SideMenu } from './SideMenu';
import { BaseLayout } from './BaseLayout';

import styles from './Layout.module.scss';

export const Layout = ({ children }) => {
    return (
        <BaseLayout>
            <div className={`${styles.header}`}>
                <Header />
            </div>
            <div className={`${styles.breadcrumb}`}>
                <Breadcrumb />
            </div>
            <div className={styles.mainContent}>
                <div className={styles.sideMenu}>
                    <SideMenu />
                </div>
                <div className={styles.pageContent}>
                    {children}
                </div>
            </div>
        </BaseLayout>
    );
}
