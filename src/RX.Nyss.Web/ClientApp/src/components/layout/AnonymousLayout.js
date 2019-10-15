import React from 'react';
import { BaseLayout } from './BaseLayout';

import globalLayoutStyles from './Layout.module.scss';
import styles from './AnonymousLayout.module.scss';

export const AnonymousLayout = ({ children }) => {
    return (
        <BaseLayout>
            <div className={`${globalLayoutStyles.header} ${styles.anonymousHeader}`}>
                <img src="/images/logo.png" alt="" />
            </div>
            <div className={styles.content}>
                {children}
            </div>
        </BaseLayout>
    );
}
