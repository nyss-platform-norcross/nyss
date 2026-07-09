import { Link } from "react-router-dom";
import React from "react";
import { connect } from "react-redux";
import { UserStatus } from "./UserStatus";
import {
  Grid,
  Icon,
  useTheme,
  useMediaQuery,
  makeStyles,
} from "@material-ui/core";
import { toggleSideMenu } from "../app/logic/appActions";

const useStyles = makeStyles((theme) => ({
  hamburgerMenu: {
    cursor: "pointer",
    [theme.breakpoints.up("lg")]: {
      display: "none",
    },
  },
  smallLogo: {
    height: "36px",
    [theme.breakpoints.up("lg")]: {
      display: "none",
    },
  },
  logo: {
    marginTop: "20px",
    height: "36px",
  },
  header: {
    paddingLeft: "20px",
    paddingRight: "20px",
    [theme.breakpoints.up("lg")]: {
      paddingLeft: "15px",
      paddingRight: "15px",
    },
  },
}));

const HeaderComponent = ({
  sideMenuOpen,
  toggleSideMenu,
  rtl,
  isSupervisor,
}) => {
  const theme = useTheme();
  const classes = useStyles();
  const isSmallScreen = useMediaQuery(theme.breakpoints.down("md"));

  return (
    <Grid container className={classes.header}>
      {!isSupervisor && (
        <div onClick={() => toggleSideMenu(!sideMenuOpen)}>
          <Icon className={classes.hamburgerMenu} fontSize="large">
            menu
          </Icon>
          <img
            className={`${classes.smallLogo}`}
            style={{
              marginLeft: rtl ? "0" : "8px",
              marginRight: rtl ? "8px" : "0",
              paddingTop: "6px",
            }}
            src="/images/logo-small.svg"
            alt="Nyss logo"
          />
        </div>
      )}
      {isSupervisor && (
        <Grid container justifyContent="space-between">
          <Link to="/">
            <img
              src={
                isSmallScreen
                  ? "/images/logo-small.svg?cache=" + new Date().getTime()
                  : "/images/logo.svg?cache=" + new Date().getTime()
              }
              alt="Nyss logo"
              className={classes.logo}
            />
          </Link>
          <UserStatus />
        </Grid>
      )}
    </Grid>
  );
};

const mapStateToProps = (state) => ({
  sideMenuOpen: state.appData.mobile.sideMenuOpen,
  rtl: state.appData.direction === "rtl",
  isSupervisor:
    state.appData.user.roles.includes("Supervisor") ||
    state.appData.user.roles.includes("HeadSupervisor"),
});

const mapDispatchToProps = {
  toggleSideMenu: toggleSideMenu,
};

export const Header = connect(
  mapStateToProps,
  mapDispatchToProps,
)(HeaderComponent);
