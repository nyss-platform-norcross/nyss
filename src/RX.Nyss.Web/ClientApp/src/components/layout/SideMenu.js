import styles from './SideMenu.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import { Link } from 'react-router-dom'
import { push } from "connected-react-router";
import { useTheme, List, ListItem, ListItemText, Drawer, useMediaQuery } from "@material-ui/core";
import { toggleSideMenu } from '../app/logic/appActions';

const SideMenuComponent = ({ sideMenu, sideMenuOpen, toggleSideMenu, push }) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('md'));

  const userLanguageCode = useSelector(state => state.appData.user.languageCode);

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
            <Link to="/" className={userLanguageCode !== 'ar' ? styles.logo : styles.logoDirectionRightToLeft}>
              <img src="/images/logo.svg" alt="Nyss logo" />
            </Link>
          </div>

          {sideMenu.length !== 0 && (
            <List component="nav" className={styles.list}>
              {sideMenu.map((item, index) => {
                const hasSubmenu = item.submenu && item.submenu.length > 0;

                return (
                  <ListItem
                    key={`sideMenuItem_${index}`}
                    className={`${hasSubmenu ? styles.withSubList : ""} ${
                      item.isActive ? styles.active : ""
                    }`}
                    button
                    onClick={() => handleItemClick(item)}
                  >
                    <ListItemText primary={item.title} />
                    {hasSubmenu ? <SubMenu submenu={item.submenu} handleItemClick={handleItemClick} /> : null}
                  </ListItem>
                );
              })}
            </List>
          )}
        </div>
      </Drawer>
    </div>
  );
};

const SubMenu = ({submenu, handleItemClick}) => {
  return (
    <List component="nav" className={styles.sublist}>
      {submenu.map((subItem, index) => (
        <ListItem
          key={`sideMenuItem_${index}__subItem_${index}`}
          className={subItem.isActive ? styles.active : ""}
          button
          onClick={(e) => {
            e.stopPropagation();
            handleItemClick(subItem);
          }}
        >
          <ListItemText primary={subItem.title} />
        </ListItem>
      ))}
    </List>
  );
};

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
