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

const drawerWidth = 240;

const useStyles = makeStyles((theme) => ({
  MenuContainer: {
    height: '100%',
    marginTop: '12px',
  },
  SideMenu: {
    display: 'flex',
    flexDirection: 'column',
    height: '100%',
    background: '#F4F4F4',
  },
  drawer: {
    zIndex: 1,
    "@media (min-width: 1280px)": {
      width: drawerWidth,
    },
    flexShrink: 0,
    whiteSpace: "nowrap",
    '& .MuiDrawer-paper': {
      borderRight: 'none',
    },
  },
  drawerOpen: {
    width: drawerWidth,
    transition: theme.transitions.create('width', {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.enteringScreen,
    }),
  },
  drawerClose: {
    transition: theme.transitions.create('width', {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.leavingScreen,
    }),
    overflowX: 'hidden',
    width: theme.spacing(7) + 1,
    [theme.breakpoints.up('sm')]: {
      width: theme.spacing(9) + 2,
    },
  },
  logo: {
    height: "100%",
    display: "flex",
    justifyContent: "center",
    alignItems: "center"
  },
  image: {
    height: "38px"
  }
}));


const SideMenuComponent = ({ generalMenu, sideMenu, sideMenuOpen, toggleSideMenu, push, isSideMenuExpanded }) => {
  const theme = useTheme();
  const classes = useStyles();
  const isSmallScreen = useMediaQuery(theme.breakpoints.down('md'));

  const userLanguageCode = useSelector(state => state.appData.user.languageCode);

  const handleItemClick = (item) => {
    push(item.url);
    closeDrawer();
  };

  const closeDrawer = () => {
    toggleSideMenu(false);
  }
  return (
    <Drawer
      variant={isSmallScreen ? "temporary" : "permanent"}
      anchor={"left"}
      open={!isSmallScreen || sideMenuOpen}
      onClose={closeDrawer}
      className={`${classes.drawer} ${!isSmallScreen && (isSideMenuExpanded ? classes.drawerOpen : classes.drawerClose)}`}
      classes={{
        paper: !isSmallScreen && (isSideMenuExpanded ? classes.drawerOpen : classes.drawerClose)
      }}
      ModalProps={{
        keepMounted: isSmallScreen
      }}
    >
      <div className={classes.SideMenu}>
        <div className={styles.sideMenuHeader}>
          <Link to="/" className={userLanguageCode !== 'ar' ? classes.logo : styles.logoDirectionRightToLeft}>
            <img className={classes.image} src={!isSideMenuExpanded && !isSmallScreen ? "/images/logo-small.svg" : "/images/logo.svg"} alt="Nyss logo" />
          </Link>
        </div>
        <Grid container className={classes.MenuContainer} direction={'column'} justifyContent='space-between'>
          <Grid container direction='column'>
          {generalMenu.length !== 0 && (
            <MenuSection menuTitle={strings(stringKeys.sideMenu.general)} menuItems={generalMenu} handleItemClick={handleItemClick} isExpanded={isSideMenuExpanded}/>
            )}
          {sideMenu.length !== 0 && (
            <Grid style={{ marginTop: 20 }}>
              <MenuSection menuTitle={strings(stringKeys.sideMenu.nationalSocieties)} menuItems={sideMenu} handleItemClick={handleItemClick} isExpanded={isSideMenuExpanded}/>
            </Grid>
            )}
          </Grid>
          <AccountSection handleItemClick={handleItemClick} isExpanded={isSideMenuExpanded}/>
        </Grid>
      </div>
    </Drawer>
  );
}

SideMenuComponent.propTypes = {
  appReady: PropTypes.bool,
  sideMenu: PropTypes.array
};

const mapStateToProps = state => ({
  generalMenu: state.appData.siteMap.generalMenu,
  sideMenu: state.appData.siteMap.sideMenu,
  sideMenuOpen: state.appData.mobile.sideMenuOpen,
  isSideMenuExpanded: state.appData.isSideMenuExpanded
});

const mapDispatchToProps = {
  push: push,
  toggleSideMenu: toggleSideMenu
};

export const SideMenu = connect(mapStateToProps, mapDispatchToProps)(SideMenuComponent);
