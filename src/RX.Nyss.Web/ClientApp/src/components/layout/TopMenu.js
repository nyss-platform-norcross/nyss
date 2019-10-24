import React from 'react';
import Typography from '@material-ui/core/Typography';
import Link from '@material-ui/core/Link';

import styles from './TopMenu.module.scss';

export const TopMenu = () => {
  return (
    <Typography className={styles.topMenu}>
      <Link href="#">National societies</Link>
      <Link href="#">Health risks</Link>
      <Link href="#">Settings</Link>
    </Typography>
  );
}
