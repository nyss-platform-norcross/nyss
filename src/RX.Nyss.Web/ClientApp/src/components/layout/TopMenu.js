import React from 'react';
import Typography from '@material-ui/core/Typography';
import { Link } from 'react-router-dom'

import styles from './TopMenu.module.scss';

export const TopMenu = () => {
  return (
    <Typography className={styles.topMenu}>
      <Link to="/nationalsocieties">National societies</Link>
      <Link to="#">Health risks</Link>
      <Link to="#">Settings</Link>
    </Typography>
  );
}
