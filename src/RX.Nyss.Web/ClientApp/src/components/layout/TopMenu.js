import React from 'react';
import Typography from '@material-ui/core/Typography';
import Link from '@material-ui/core/Link';

import styles from './TopMenu.module.scss';

export const TopMenu = () => {
    return (
        <Typography className={styles.topMenu}>
            <Link>National societies</Link>
            <Link>Health risks</Link>
            <Link>Settings</Link>
        </Typography>
    );
}
