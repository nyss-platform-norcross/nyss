import styles from "./SideMenu.module.scss";

import React from "react";
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import { Link } from "react-router-dom";
import { push } from "connected-react-router";
import {
  useTheme,
  Drawer,
  Grid,
  useMediaQuery,
  makeStyles,
} from "@material-ui/core";
import { toggleSideMenu } from "../app/logic/appActions";
import { MenuSection } from "./MenuSection";
import { stringKeys, strings } from "../../strings";
import ExpandButton from "../common/buttons/expandButton/ExpandButton";
import { AccountSection } from "./AccountSection";
import { expandSideMenu } from "../app/logic/appActions";

const drawerWidth = 230;

const useStyles = makeStyles((theme) => ({
  MenuContainer: {
    paddingTop: "12px",
    backgroundColor: theme.palette.background.default,
  },
  SideMenu: {
    display: "flex",
    flexDirection: "column",
    height: "100%",
    backgroundColor: theme.palette.background.default,
    position: "relative",
  },
  SideMenuContent: {
    overflowX: "hidden",
    overflowY: "auto",
    scrollbarColor: `#B4B4B4 ${theme.palette.background.default}`,
  },
  drawer: {
    zIndex: 100,
    "@media (min-width: 1280px)": {
      width: drawerWidth,
    },
    flexShrink: 0,
    whiteSpace: "nowrap",
    "& .MuiDrawer-paper": {
      borderRight: "none",
      borderLeft: "none",
    },
  },
  drawerOpen: {
    width: drawerWidth,
    transition: theme.transitions.create("width", {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.enteringScreen,
    }),
  },
  drawerClose: {
    transition: theme.transitions.create("width", {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.leavingScreen,
    }),
    width: "230px",
    [theme.breakpoints.up("sm")]: {
      width: "58px",
    },
  },
  logo: {
    height: "100%",
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
  },
  image: {
    height: "38px",
  },
  overflowVisible: {
    overflowY: "visible",
  },
}));

const SideMenuComponent = ({
  generalMenu,
  sideMenu,
  sideMenuOpen,
  toggleSideMenu,
  push,
  isSideMenuExpanded,
  expandSideMenu,
  isSupervisor,
}) => {
  const theme = useTheme();
  const classes = useStyles();
  const isSmallScreen = useMediaQuery(theme.breakpoints.down("md"));

  const userLanguageCode = useSelector(
    (state) => state.appData.user.languageCode,
  );

  const handleItemClick = (item) => {
    push(item.url);
    closeDrawer();
  };

  const closeDrawer = () => {
    toggleSideMenu(false);
  };

  const handleExpandClick = () => {
    expandSideMenu(!isSideMenuExpanded);
  };

  if (isSupervisor) return null;
  return (
    <div className={styles.sideMenu}>
      <div className={styles.drawer}>
        <Drawer
          variant={isSmallScreen ? "temporary" : "permanent"}
          anchor={"left"}
          open={!isSmallScreen || sideMenuOpen}
          onClose={closeDrawer}
          className={`${classes.drawer} ${
            !isSmallScreen &&
            (isSideMenuExpanded ? classes.drawerOpen : classes.drawerClose)
          }`}
          classes={{
            paper:
              !isSmallScreen &&
              (isSideMenuExpanded ? classes.drawerOpen : classes.drawerClose),
          }}
          PaperProps={{
            classes: {
              root: classes.overflowVisible,
            },
          }}
          ModalProps={{
            keepMounted: isSmallScreen,
          }}
        >
          <div className={classes.SideMenu}>
            <ExpandButton
              onClick={handleExpandClick}
              isExpanded={isSideMenuExpanded}
            />
            <div className={styles.sideMenuHeader}>
              <Link to="/" className={classes.logo}>
                <img
                  className={classes.image}
                  src={
                    !isSideMenuExpanded && !isSmallScreen
                      ? "/images/logo-small.svg"
                      : "/images/logo.svg"
                  }
                  alt="Nyss logo"
                />
              </Link>
            </div>
            <div className={classes.SideMenuContent}>
              <Grid
                container
                className={classes.MenuContainer}
                direction={"column"}
              >
                {generalMenu.length !== 0 && (
                  <MenuSection
                    menuTitle={strings(stringKeys.sideMenu.general)}
                    menuItems={generalMenu}
                    handleItemClick={handleItemClick}
                    isExpanded={isSideMenuExpanded}
                  />
                )}
                {sideMenu.length !== 0 && (
                  <Grid style={{ marginTop: 20 }}>
                    <MenuSection
                      menuTitle={strings(stringKeys.sideMenu.nationalSocieties)}
                      menuItems={sideMenu}
                      handleItemClick={handleItemClick}
                      isExpanded={isSideMenuExpanded}
                    />
                  </Grid>
                )}
              </Grid>
            </div>
            <AccountSection
              handleItemClick={handleItemClick}
              isExpanded={isSideMenuExpanded}
            />
          </div>
        </Drawer>
      </div>
    </div>
  );
};

SideMenuComponent.propTypes = {
  appReady: PropTypes.bool,
  sideMenu: PropTypes.array,
};

const mapStateToProps = (state) => ({
  generalMenu: state.appData.siteMap.generalMenu,
  sideMenu: state.appData.siteMap.sideMenu,
  sideMenuOpen: state.appData.mobile.sideMenuOpen,
  isSideMenuExpanded: state.appData.isSideMenuExpanded,
  isSupervisor:
    state.appData.user.roles.includes("Supervisor") ||
    state.appData.user.roles.includes("HeadSupervisor"),
});

const mapDispatchToProps = {
  push: push,
  toggleSideMenu: toggleSideMenu,
  expandSideMenu: expandSideMenu,
};

export const SideMenu = connect(
  mapStateToProps,
  mapDispatchToProps,
)(SideMenuComponent);
