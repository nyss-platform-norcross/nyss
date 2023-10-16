import styles from './TabMenu.module.scss';

import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { push } from "connected-react-router";
import { Tabs, Tab, Grid, Typography, makeStyles } from '@material-ui/core';

const useStyles = makeStyles({
  nsHeader: {
    color: "#4F4F4F",
    fontSize: 16,
    fontWeight: 400,
    textAlign: "center"
  },
  projectHeader: {
    color: "#000",
    fontSize: 32,
    fontWeight: 700,
    textAlign: "center",
    margin: "20px 0 20px 0"
  }
});

const TabMenuComponent = ({ projectTabMenu, tabMenu, push, currentUrl, title, projectName, nationalSocietyName }) => {
  const classes = useStyles()

  const onItemClick = (item) => {
    push(item.url);
  };

  // http addresses are case insensitive so compare to-lower versions
  const showTabMenu = tabMenu.some(t => t.url.toLowerCase() === currentUrl.toLowerCase());

  return (
    <div className={styles.tabMenu}>
      {nationalSocietyName && projectName ?
        <>
          <Typography className={classes.nsHeader}>{nationalSocietyName}</Typography>
          <Typography className={classes.projectHeader}>{projectName}</Typography>
        </> :
        <div className={styles.header}>{title}</div>
      }
      <Grid container justifyContent='center' style={{ width: "100%" }}>
      {/* Only display project tab menu for all users other than data consumer since the role only has acces to project dashboard */}
      {projectTabMenu && projectTabMenu.length > 1 && (
        <Tabs
        value={projectTabMenu.indexOf(projectTabMenu.find(t => t.isActive))}
        className={styles.projectTabs}
        scrollButtons="auto"
        indicatorColor="primary"
        variant="scrollable">
          {projectTabMenu.map(item => (
            <Tab key={`tabMenu_${item.url}`} label={item.title} onClick={() => onItemClick(item)} />
            ))}
        </Tabs>
      )}
      </Grid>
      {showTabMenu && (
        <Tabs
          value={tabMenu.indexOf(tabMenu.find(t => t.isActive))}
          className={styles.tabs}
          scrollButtons="auto"
          indicatorColor="primary"
          variant="scrollable">
          {tabMenu.map(item => (
            <Tab key={`tabMenu_${item.url}`} label={item.title} onClick={() => onItemClick(item)} />
          ))}
        </Tabs>
      )}
    </div>
  );
}

TabMenuComponent.propTypes = {
  appReady: PropTypes.bool,
  tabMenu: PropTypes.array
};

const mapStateToProps = state => ({
  nationalSocietyName: state.appData.siteMap.parameters.nationalSocietyName,
  projectName: state.appData.siteMap.parameters.projectName,
  projectTabMenu: state.appData.siteMap.projectTabMenu,
  tabMenu: state.appData.siteMap.tabMenu,
  title: state.appData.siteMap.title,
  currentUrl: state.appData.route.url
});

const mapDispatchToProps = {
  push: push
};

export const TabMenu = connect(mapStateToProps, mapDispatchToProps)(TabMenuComponent);
