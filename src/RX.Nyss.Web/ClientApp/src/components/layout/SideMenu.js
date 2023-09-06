import styles from './SideMenu.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import { Link } from 'react-router-dom'
import { push } from "connected-react-router";
import { useTheme, List, ListItem, ListItemText, ListItemIcon, Drawer, useMediaQuery, makeStyles } from "@material-ui/core";
import { toggleSideMenu } from '../app/logic/appActions';
import { RcIcon } from '../icons/RcIcon';


const useStyles = makeStyles((theme) => ({
  SideMenuIcon: {
    fontSize: '26px',
    color: '#4F4F4F',
  },
  SideMenuIconActive: {
    color: '#D52B1E',
  },
  ListItemIconWrapper: {
    minWidth: '20px',
    marginRight: '5px',
  },
  SideMenuText: {
    color: '#4F4F4F',
  },
  ListItemActive: {
    "& span": {
      color: '#D52B1E',
      fontWeight: '600',
    },
  }
}));


const SideMenuComponent = ({ sideMenu, sideMenuOpen, toggleSideMenu, push }) => {
  const theme = useTheme();
  const classes = useStyles();
  const fullScreen = useMediaQuery(theme.breakpoints.down('md'));

  const userLanguageCode = useSelector(state => state.appData.user.languageCode);

  const mapPathToSideMenuIcon = (path) => {
    if (path.includes('dashboard')) {
      return "Dashboard"
    } else if (path.includes('reports')) {
      return "Report"
    } else if (path.includes('users')) {
      return "Users"
    } else if (path.includes('settings')) {
      return "Settings"
    } else if (path.includes('datacollectors')) {
      return "DataCollectors"
    } else if (path.includes('alerts')) {
      return "Alerts"
    } else if (path.includes('overview')) {
      return "Settings"
    } else if (path.includes('projects')) {
      return "Project"
    } else {
      return "Dashboard"
    }
  }

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
            <List component="nav" className={styles.list} aria-label="Side navigation menu">
              {sideMenu.map((item) => {
                return (
                  <ListItem key={`sideMenuItem_${item.title}`} className={item.isActive ? classes.ListItemActive : undefined} button onClick={() => handleItemClick(item)} >
                    <ListItemIcon className={classes.ListItemIconWrapper}>
                      {item.url && <RcIcon icon={mapPathToSideMenuIcon(item.url)} className={`${classes.SideMenuIcon} ${item.isActive ? classes.SideMenuIconActive : ''}`} />}
                    </ListItemIcon>
                    <ListItemText primary={item.title} primaryTypographyProps={{'className': classes.SideMenuText }} />
                  </ListItem>
                )
              })}
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
