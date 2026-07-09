import React from "react";
import { useDispatch, useSelector } from "react-redux";
import PropTypes from "prop-types";
import {
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Typography,
  makeStyles,
  Tooltip,
} from "@material-ui/core";
import { RcIcon } from "../icons/RcIcon";
import { logout } from "../../authentication/authActions";
import { strings, stringKeys } from "../../strings";

export const AccountSection = ({ handleItemClick, isExpanded }) => {
  const useRtlDirection = useSelector(
    (state) => state.appData.direction === "rtl",
  );
  const user = useSelector((state) => state.appData.user);
  const isSupervisor =
    user.roles.includes("Supervisor") || user.roles.includes("HeadSupervisor");

  const useStyles = makeStyles((theme) => ({
    AccordionContainer: {
      position: "sticky",
      marginTop: "auto",
      bottom: 0,
      zIndex: 1,
    },
    Accordion: {
      backgroundColor: theme.palette.backgroundDark.main,
      "&.MuiAccordion-root:before": {
        backgroundColor: "inherit",
      },
    },
    AccordionExpanded: {
      margin: "0 !important",
    },
    AccordionSummary: {
      backgroundColor: "#EDEDED",
      "&:hover": {
        backgroundColor: "rgba(0, 0, 0, 0.08)",
      },
    },
    AccordionSummaryContent: {
      display: "flex",
      flexDirection: "row",
      alignItems: "center",
      gap: "12px",
      padding: "5px 0 5px 0",
      margin: "10px 0 10px 0",
    },
    AccordionSummaryContentCollapsed: {
      justifyContent: "center",
    },
    AccordionSummaryRoot: {
      boxShadow: "0px -2px 2px 0px rgba(124, 124, 124, 0.20)",
      borderRadius: "8px 8px 0 0",
    },
    AccordionSummaryExpanded: {
      padding: "0 8px",
      borderRadius: "8px 8px 0 0",
    },
    AccordionDetails: {
      padding: 0,
    },
    List: {
      width: "100%",
    },
    ListItem: {
      color: "#1E1E1E",
      padding: "8px 16px 8px 16px",
      display: "flex",
      justifyContent: "center",
    },
    ListItemUser: {
      color: "#1E1E1E",
      padding: "0px 16px 0px 16px",
    },
    ListItemTextUserContainer: {
      display: "flex",
      flexDirection: "column",
      textAlign: "center",
      paddingLeft: 0,
      paddingRight: 0,
    },
    ListItemTextUser: {
      color: "#7C7C7C",
    },
    ListItemText: {
      color: "#1E1E1E",
    },
    ListItemTextWrapper: {
      margin: "0",
      padding: "8px 0 8px 0",
    },
    ListItemIcon: {
      minWidth: "20px",
      paddingRight: "20px",
    },
    ListItemIconCollapsed: {
      paddingRight: "0px",
    },
    User: {
      fontSize: 16,
    },
    Account: {
      fontWeight: "bold",
      padding: useRtlDirection ? "8px 8px 8px 0" : "8px 0 8px 8px",
      backgroundColor: theme.palette.background.default,
    },
    Hide: {
      color: "transparent",
      userSelect: "none",
    },
  }));
  const classes = useStyles();
  const dispatch = useDispatch();
  const handleLogout = () => dispatch(logout.invoke());

  if (isSupervisor) return null;

  return (
    <div className={classes.AccordionContainer}>
      <Typography
        variant="body2"
        className={`${classes.Account} ${!isExpanded && classes.Hide}`}
      >
        {strings(stringKeys.sideMenu.account)}
      </Typography>
      <Accordion
        square={false}
        className={classes.Accordion}
        classes={{ expanded: classes.AccordionExpanded }}
      >
        <Tooltip title={strings(stringKeys.sideMenu.account)}>
          <div>
            <AccordionSummary
              className={classes.AccordionSummary}
              classes={{
                root: classes.AccordionSummaryRoot,
                content: `${classes.AccordionSummaryContent} ${
                  !isExpanded && classes.AccordionSummaryContentCollapsed
                }`,
                expanded: classes.AccordionSummaryExpanded,
              }}
            >
              <RcIcon
                icon="UserCircle"
                style={{
                  fontSize: "24px",
                }}
              />

              {isExpanded && (
                <Typography className={classes.User}>{user.name}</Typography>
              )}
            </AccordionSummary>
          </div>
        </Tooltip>
        <AccordionDetails className={classes.AccordionDetails}>
          <List
            style={{ maxWidth: "inherit" }}
            component="nav"
            className={classes.List}
            aria-label="Side navigation menu"
            disablePadding
          >
            {isExpanded && (
              <ListItem className={classes.ListItemUser}>
                <ListItemText className={classes.ListItemTextUserContainer}>
                  <Tooltip title={user.roles[0]}>
                    <Typography
                      variant="body2"
                      noWrap
                      className={classes.ListItemTextUser}
                    >
                      {user.roles[0]}
                    </Typography>
                  </Tooltip>
                  <Tooltip title={user.email}>
                    <Typography noWrap className={classes.ListItemTextUser}>
                      {user.email}
                    </Typography>
                  </Tooltip>
                </ListItemText>
              </ListItem>
            )}
            <ListItem
              button
              onClick={() => handleItemClick({ url: "/feedback" })}
              className={classes.ListItem}
            >
              <Tooltip title={strings(stringKeys.feedback.send)}>
                <ListItemIcon
                  className={`${classes.ListItemIcon} ${
                    !isExpanded && classes.ListItemIconCollapsed
                  }`}
                >
                  <RcIcon icon="Feedback" className={classes.ListItemText} />
                </ListItemIcon>
              </Tooltip>
              {isExpanded && (
                <Tooltip title={strings(stringKeys.feedback.send)}>
                  <ListItemText
                    classes={{
                      primary: classes.ListItemText,
                      root: classes.ListItemTextWrapper,
                    }}
                    primaryTypographyProps={{ noWrap: true }}
                  >
                    {strings(stringKeys.feedback.send)}
                  </ListItemText>
                </Tooltip>
              )}
            </ListItem>
            <ListItem
              button
              onClick={handleLogout}
              className={classes.ListItem}
            >
              <Tooltip title={strings(stringKeys.user.logout)}>
                <ListItemIcon
                  className={`${classes.ListItemIcon} ${
                    !isExpanded && classes.ListItemIconCollapsed
                  }`}
                >
                  <RcIcon icon="Logout" className={classes.ListItemText} />
                </ListItemIcon>
              </Tooltip>
              {isExpanded && (
                <Tooltip title={strings(stringKeys.user.logout)}>
                  <ListItemText
                    classes={{
                      primary: classes.ListItemText,
                      root: classes.ListItemTextWrapper,
                    }}
                    primaryTypographyProps={{ noWrap: true }}
                  >
                    {strings(stringKeys.user.logout)}
                  </ListItemText>
                </Tooltip>
              )}
            </ListItem>
          </List>
        </AccordionDetails>
      </Accordion>
    </div>
  );
};

AccountSection.propTypes = {
  logout: PropTypes.func,
  sendFeedback: PropTypes.func,
  isSendingFeedback: PropTypes.bool,
  sendFeedbackResult: PropTypes.string,
};
