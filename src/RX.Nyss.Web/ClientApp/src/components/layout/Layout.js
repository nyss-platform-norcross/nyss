import React from 'react';
import styles from './Layout.module.scss';
import { Header } from './Header';
import { Breadcrumb } from './Breadcrumb';
import { SideMenu } from './SideMenu';

export const Layout = ({ children }) => {
    return (
        <div className={styles.layout}>
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
        </div>
    );
}
