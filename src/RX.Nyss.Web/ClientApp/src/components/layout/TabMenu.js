import styles from "./TabMenu.module.scss";

import React from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { push } from "connected-react-router";
import { Tabs, Tab, Grid, Typography } from "@material-ui/core";

const TabMenuComponent = ({ tabMenu, push, currentUrl }) => {
  const onItemClick = (item) => {
    push(item.url);
  };

  // http addresses are case insensitive so compare to-lower versions
  const showTabMenu = tabMenu.some(
    (t) => t.url.toLowerCase() === currentUrl.toLowerCase(),
  );
  return (
    <Grid style={{ margin: "20px 0px" }}>
      {showTabMenu && (
        <Tabs
          value={tabMenu.indexOf(tabMenu.find((t) => t.isActive))}
          className={styles.tabs}
          scrollButtons="auto"
          indicatorColor="primary"
          variant="scrollable"
        >
          {tabMenu.map((item) => (
            <Tab
              key={`tabMenu_${item.url}`}
              label={<Typography style={{ fontWeight: item.isActive ? "bold" : "normal" }}>{item.title}</Typography>}
              onClick={item.isActive ? () => null : () => onItemClick(item)}
              style={{ textTransform: "none" }}

            />
          ))}
        </Tabs>
      )}
    </Grid>
  );
};

TabMenuComponent.propTypes = {
  appReady: PropTypes.bool,
  tabMenu: PropTypes.array,
};

const mapStateToProps = (state) => ({
  tabMenu: state.appData.siteMap.tabMenu,
  currentUrl: state.appData.route.url,
});

const mapDispatchToProps = {
  push: push,
};

export const TabMenu = connect(
  mapStateToProps,
  mapDispatchToProps,
)(TabMenuComponent);
