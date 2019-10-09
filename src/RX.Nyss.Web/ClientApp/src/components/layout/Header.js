import React from 'react';
import styles from './Header.module.scss';
import { TopMenu } from './TopMenu';

export const Header = () => {
    return (
        <div className={styles.header}>
            <div className={styles.logo}>
                <img src="/images/logo.png" alt="" />
            </div>
            <div className={styles.topMenu}>
                <TopMenu />
            </div>
            <div className={styles.user}>

            </div>
        </div>
    );
}
