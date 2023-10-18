import styles from './SideMenu.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import { Link } from 'react-router-dom'
import { push } from "connected-react-router";
import { useTheme, Drawer, Grid, useMediaQuery, makeStyles } from "@material-ui/core";
import { toggleSideMenu } from '../app/logic/appActions';
import { MenuSection } from './MenuSection';
import { stringKeys, strings } from '../../strings';
import { AccountSection } from './AccountSection';



const useStyles = makeStyles(() => ({
  MenuContainer: {
    paddingTop: '12px',
    backgroundColor: "#f1f1f1",
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
          <Grid container className={classes.MenuContainer} direction={'column'}>
            {generalMenu.length !== 0 && (
              <MenuSection menuTitle={strings(stringKeys.sideMenu.general)} menuItems={generalMenu} handleItemClick={handleItemClick}/>
              )}
            {sideMenu.length !== 0 && (
              <Grid style={{ marginTop: 20 }}>
                <MenuSection menuTitle={strings(stringKeys.sideMenu.nationalSocieties)} menuItems={sideMenu} handleItemClick={handleItemClick}/>
              </Grid>
            )}
          </Grid>
          <AccountSection handleItemClick={handleItemClick}/>
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
