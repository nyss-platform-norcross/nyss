import styles from './TopMenu.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Typography from '@material-ui/core/Typography';
import { Link } from 'react-router-dom'
import { getMenu, placeholders } from '../../siteMap';

const TopMenuComponent = ({ siteMap }) => {
  const menu = getMenu("/", siteMap.parameters, placeholders.topMenu);

  return (
    <Typography className={styles.topMenu}>
      {menu.length !== 0 && menu.map((item, index) => (
        <Link key={`topMenu${index}`} to={item.url}>{item.title}</Link>
      ))}
    </Typography>
  );
}

TopMenuComponent.propTypes = {
  siteMap: PropTypes.object
};

const mapStateToProps = state => ({
  siteMap: state.appData.siteMap
});

export const TopMenu = connect(mapStateToProps)(TopMenuComponent);
