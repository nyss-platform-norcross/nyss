import styles from './TopMenu.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import { Typography } from '@material-ui/core';
import { Link } from 'react-router-dom'

const TopMenuComponent = ({ topMenu }) => {
  const directionRtl = useSelector(state => state.appData.direction === 'rtl');

  return (
    <Typography className={`${styles.topMenu} ${directionRtl ? styles.rtl : ''}`}>
      {topMenu.length !== 0 && topMenu.map((item, index) => (
        <Link key={`topMenu${index}`} to={item.url} className={item.isActive ? styles.active : ""}>
          <span>{item.title}</span>
        </Link>
      ))}
    </Typography>
  )
};


TopMenuComponent.propTypes = {
  topMenu: PropTypes.array
};

const mapStateToProps = state => ({
  topMenu: state.appData.siteMap.generalMenu
});

export const TopMenu = connect(mapStateToProps)(TopMenuComponent);
