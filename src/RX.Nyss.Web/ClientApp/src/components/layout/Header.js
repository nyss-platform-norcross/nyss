import styles from './Header.module.scss';
import { Link } from 'react-router-dom'
import React from 'react';
import { connect, useSelector } from "react-redux";
import { UserStatus } from './UserStatus';
import { Icon, useTheme, useMediaQuery } from '@material-ui/core';
import { toggleSideMenu } from '../app/logic/appActions';

const HeaderComponent = ({ sideMenuOpen, toggleSideMenu, directionRtl, isSupervisor }) =>{
  const userLanguageCode = useSelector(state => state.appData.user.languageCode);
  const theme = useTheme();
  const isSmallScreen = useMediaQuery(theme.breakpoints.down('md'));

  return (
    <div className={styles.header}>
      {!isSupervisor && (
        <div className={styles.placeholder}>
          <Icon className={styles.toggleMenu} onClick={() => toggleSideMenu(!sideMenuOpen)}>menu</Icon>
          <img className={`${styles.smallLogo} ${directionRtl ? styles.rtl : ''}`} src="/images/logo-small.svg" alt="Nyss logo" />
        </div>
      )}
      {isSupervisor && (
          <div className={styles.supervisorHeader}>
            <Link to="/" className={userLanguageCode !== 'ar' ? styles.logo : styles.logoDirectionRightToLeft}>
              <img src={isSmallScreen ? "/images/logo-small.svg?cache=" + new Date().getTime() : "/images/logo.svg?cache=" + new Date().getTime()} alt="Nyss logo"/>
            </Link>
            <UserStatus />
          </div>
      )}
    </div>
  )};

const mapStateToProps = state => ({
  sideMenuOpen: state.appData.mobile.sideMenuOpen,
  directionRtl: state.appData.user.languageCode === 'ar',
  isSupervisor: state.appData.user.roles.includes("Supervisor") || state.appData.user.roles.includes("HeadSupervisor")
});

const mapDispatchToProps = {
  toggleSideMenu: toggleSideMenu
};

export const Header = connect(mapStateToProps, mapDispatchToProps)(HeaderComponent);
