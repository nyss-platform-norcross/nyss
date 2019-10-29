import styles from './SideMenu.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemText from '@material-ui/core/ListItemText';
import { Link } from 'react-router-dom'
import { push } from "connected-react-router";

const SideMenuComponent = ({ sideMenu, push }) => {
  const onItemClick = (item) => {
    push(item.url);
  };

  return (
    <div className={styles.sideMenu}>
      <div className={styles.sideMenuHeader}>
        <Link to="/" className={styles.logo}>
          <div className={styles.headerName}>Nyss</div>
          <div className={styles.headerDescription}>Community Based Surveillance</div>
        </Link>
      </div>

      {sideMenu.length !== 0 && (
        <List component="nav" className={styles.list}>
          {sideMenu.map((item, index) => (
            <ListItem key={`sideMenuItem_${index}`} button onClick={() => onItemClick(item)}>
              <ListItemText primary={item.title} />
            </ListItem>
          ))}
        </List>
      )}
    </div>
  );
}

SideMenuComponent.propTypes = {
  appReady: PropTypes.bool,
  sideMenu: PropTypes.array
};

const mapStateToProps = state => ({
  sideMenu: state.appData.siteMap.sideMenu
});

const mapDispatchToProps = {
  push: push
};

export const SideMenu = connect(mapStateToProps, mapDispatchToProps)(SideMenuComponent);
