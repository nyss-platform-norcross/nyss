import React from 'react';
import globalLayoutStyles from './Layout.module.scss';
import styles from './AnonymousLayout.module.scss';

export const AnonymousLayout = ({ children }) => {
    return (
        <div className={globalLayoutStyles.layout}>
            <div className={`${globalLayoutStyles.header} ${styles.anonymousHeader}`}>
                <img src="/images/logo.png" alt="" />
            </div>
            <div className={styles.content}>
                {children}
            </div>
        </div>
    );
}
