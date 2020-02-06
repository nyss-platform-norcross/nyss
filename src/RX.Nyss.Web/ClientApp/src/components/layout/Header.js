import styles from './Header.module.scss';

import React from 'react';
import { connect } from "react-redux";
import { TopMenu } from './TopMenu';
import { UserStatus } from './UserStatus';
import Icon from '@material-ui/core/Icon';
import { toggleSideMenu } from '../app/logic/appActions';

const HeaderComponent = ({ sideMenuOpen, toggleSideMenu }) => {
  return (
    <div className={styles.header}>
      <div className={styles.placeholder}>
        <Icon className={styles.toggleMenu} onClick={() => toggleSideMenu(!sideMenuOpen)}>menu</Icon>
        <img className={styles.smallLogo} src="/images/logo-small.svg" alt="Nyss logo" />
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
  breadcrumb: state.appData.siteMap.breadcrumb,
  sideMenuOpen: state.appData.mobile.sideMenuOpen
});

const mapDispatchToProps = {
  toggleSideMenu: toggleSideMenu
};

export const Header = connect(mapStateToProps, mapDispatchToProps)(HeaderComponent);
