import styles from './SideMenu.module.scss';

import React from 'react';
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemText from '@material-ui/core/ListItemText';
import { Link } from 'react-router-dom'

export const SideMenu = () => {
  return (
    <div className={styles.sideMenu}>
      <div className={styles.sideMenuHeader}>
        <Link to="/" className={styles.logo}>
          <div className={styles.headerName}>Nyss</div>
          <div className={styles.headerDescription}>Community Based Surveillance</div>
        </Link>
      </div>
      <List component="nav" className={styles.list}>
        <ListItem button>
          <ListItemText primary="Dashboard" />
        </ListItem>
        <ListItem button selected>
          <ListItemText primary="Projects" className={styles.selected} />
        </ListItem>
        <ListItem button>
          <ListItemText primary="Drafts" />
        </ListItem>
      </List>
    </div>
  );
}
