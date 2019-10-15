import React from 'react';
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemText from '@material-ui/core/ListItemText';

import styles from './SideMenu.module.scss';

export const SideMenu = () => {
    return (
        <List component="nav" aria-label="main mailbox folders">
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
    );
}
