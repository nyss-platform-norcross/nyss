import styles from './Header.module.scss';

import React from 'react';
import { connect } from "react-redux";
import { TopMenu } from './TopMenu';
import { UserStatus } from './UserStatus';
import { Icon } from '@material-ui/core';
import { toggleSideMenu } from '../app/logic/appActions';

const HeaderComponent = ({ sideMenuOpen, toggleSideMenu, directionRtl }) => {
  return (
    <div className={styles.header}>
      <div className={styles.placeholder}>
        <Icon className={styles.toggleMenu} onClick={() => toggleSideMenu(!sideMenuOpen)}>menu</Icon>
        <img className={`${styles.smallLogo} ${directionRtl ? styles.rtl : ''}`} src="/images/logo-small.svg" alt="Nyss logo" />
      </div>
      <div className={styles.topMenu}>
        <TopMenu />
      </div>
      <div className={styles.user}>
        <UserStatus />
      </div>
    </div>
  );
}

const mapStateToProps = state => ({
  sideMenuOpen: state.appData.mobile.sideMenuOpen,
  directionRtl: state.appData.user.languageCode === 'ar'
});

const mapDispatchToProps = {
  toggleSideMenu: toggleSideMenu
};

export const Header = connect(mapStateToProps, mapDispatchToProps)(HeaderComponent);
