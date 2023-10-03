import styles from './SideMenu.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import { Link } from 'react-router-dom'
import { push } from "connected-react-router";
import { useTheme, Drawer, Grid, useMediaQuery, makeStyles } from "@material-ui/core";
import { toggleSideMenu } from '../app/logic/appActions';
import { MenuSection } from './MenuSection';


const useStyles = makeStyles((theme) => ({
  SideMenuIcon: {
    fontSize: '22px',
    color: '#1E1E1E',
  },
  SideMenuIconActive: {
    color: '#D52B1E',
  },
  ListItemIconWrapper: {
    minWidth: '20px',
  },
  SideMenuText: {
    color: '#1E1E1E',
    fontSize: '16px',
  },
  ListItemActive: {
    "& span": {
      color: '#D52B1E',
      fontWeight: '600',
    }
  },
  MenuContainer: {
    height: '100%',
    marginTop: '32px',
  },
  SideMenu: {
    display: 'flex',
    flexDirection: 'column',
    height: '100%',
    background: 'linear-gradient(90deg, #f1f1f1 90%, #ececec)',
  }
}));


const SideMenuComponent = ({ generalMenu, sideMenu, sideMenuOpen, toggleSideMenu, push }) => {
  const theme = useTheme();
  const classes = useStyles();
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
        <div className={classes.SideMenu}>
          <div className={styles.sideMenuHeader}>
            <Link to="/" className={userLanguageCode !== 'ar' ? styles.logo : styles.logoDirectionRightToLeft}>
              <img src="/images/logo.svg" alt="Nyss logo" />
            </Link>
          </div>
          <Grid container className={classes.MenuContainer} direction={'column'} justifyContent='space-between'>
            {generalMenu.length !== 0 && (
              <MenuSection menuTitle={"General"} menuItems={generalMenu} handleItemClick={handleItemClick}/>
              )}
            {sideMenu.length !== 0 && (
              <MenuSection menuTitle={"National Societies"} menuItems={sideMenu} handleItemClick={handleItemClick}/>
              )}
            <div>
              {/*Insert account here*/}
            </div>
          </Grid>
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
  generalMenu: state.appData.siteMap.generalMenu,
  sideMenu: state.appData.siteMap.sideMenu,
  sideMenuOpen: state.appData.mobile.sideMenuOpen
});

const mapDispatchToProps = {
  push: push,
  toggleSideMenu: toggleSideMenu
};

export const SideMenu = connect(mapStateToProps, mapDispatchToProps)(SideMenuComponent);
