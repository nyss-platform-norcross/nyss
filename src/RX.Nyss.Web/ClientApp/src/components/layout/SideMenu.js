import styles from './SideMenu.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemText from '@material-ui/core/ListItemText';
import { Link } from 'react-router-dom'
import { getMenu, placeholders } from '../../siteMap';

const SideMenuComponent = ({ siteMap }) => {
  const menu = getMenu(siteMap.path, siteMap.parameters, placeholders.leftMenu);

  const onItemClick = (item) => {

  };

  return (
    <div className={styles.sideMenu}>
      <div className={styles.sideMenuHeader}>
        <Link to="/" className={styles.logo}>
          <div className={styles.headerName}>Nyss</div>
          <div className={styles.headerDescription}>Community Based Surveillance</div>
        </Link>
      </div>

      {menu.length !== 0 && (
        <List component="nav" className={styles.list}>
          {menu.map(item => (
            <ListItem button onClick={() => onItemClick(item)}>
              <ListItemText primary={item.title} />
            </ListItem>
          ))}
        </List>
      )}
    </div>
  );
}

SideMenuComponent.propTypes = {
  appReady: PropTypes.bool
};

const mapStateToProps = state => ({
  siteMap: state.appData.siteMap
});

export const SideMenu = connect(mapStateToProps)(SideMenuComponent);
