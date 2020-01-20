import styles from './SideMenu.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemText from '@material-ui/core/ListItemText';
import { Link } from 'react-router-dom'
import { push } from "connected-react-router";
import Drawer from "@material-ui/core/Drawer";
import { useTheme } from "@material-ui/core";
import useMediaQuery from '@material-ui/core/useMediaQuery';
import { toggleSideMenu } from '../app/logic/appActions';

const SideMenuComponent = ({ sideMenu, sideMenuOpen, toggleSideMenu, push }) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('md'));

  const handleItemClick = (item) => {
    push(item.url);
    closeDrawer();
  };

  const closeDrawer = () => {
    toggleSideMenu(false);
  }
  return (
    <div className={styles.drawer}>
      <Drawer
        variant={fullScreen ? "temporary" : "permanent"}
        anchor={"left"}
        open={!fullScreen || sideMenuOpen}
        onClose={closeDrawer}
        classes={{
          paper: styles.drawer
        }}
        ModalProps={{
          keepMounted: fullScreen
        }}
      >
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
                <ListItem key={`sideMenuItem_${index}`} className={item.isActive ? styles.active : ""} button onClick={() => handleItemClick(item)}>
                  <ListItemText primary={item.title} />
                </ListItem>
              ))}
            </List>
          )}
        </div>
      </Drawer>
    </div>
  );
}

SideMenuComponent.propTypes = {
  appReady: PropTypes.bool,
  sideMenu: PropTypes.array
};

const mapStateToProps = state => ({
  sideMenu: state.appData.siteMap.sideMenu,
  sideMenuOpen: state.appData.mobile.sideMenuOpen
});

const mapDispatchToProps = {
  push: push,
  toggleSideMenu: toggleSideMenu
};

export const SideMenu = connect(mapStateToProps, mapDispatchToProps)(SideMenuComponent);
