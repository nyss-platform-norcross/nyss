import React from 'react';
import { useDispatch, useSelector } from "react-redux";
import PropTypes from "prop-types";
import { List, ListItem, ListItemIcon, ListItemText, Accordion, AccordionSummary, AccordionDetails, Typography, makeStyles, Tooltip } from '@material-ui/core';
import { RcIcon } from '../icons/RcIcon';
import { logout } from '../../authentication/authActions';
import { strings, stringKeys } from '../../strings';

const useStyles = makeStyles(() => ({
    AccordionContainer: {
      position: "sticky",
      marginTop: "auto",
      bottom: 0,
      zIndex: 1,
    },
    Accordion: {
      backgroundColor: '#F4F4F4',
      "&.MuiAccordion-root:before": {
        backgroundColor: "inherit",
      },
    },
    AccordionExpanded: {
      margin: "0 !important",
    },
    AccordionSummary: {
      backgroundColor: '#EDEDED',
      "&:hover": {
        backgroundColor: "rgba(0, 0, 0, 0.08)",
      }
    },
    AccordionSummaryContent: {
      display: 'flex',
      flexDirection: 'row',
      alignItems: "center",
      gap: "12px",
      padding: "5px 0 5px 0",
      margin: "10px 0 10px 0",
    },
    AccordionSummaryRoot: {
      boxShadow: "0px -2px 2px 0px rgba(124, 124, 124, 0.20)",
      borderRadius: "8px 8px 0 0",
    },
    AccordionSummaryExpanded: {
      padding: "0 8px",
      borderRadius: "8px 8px 0 0"
    },
    AccordionDetails: {
      padding: 0
    },
    List: {
      width: "100%"
    },
    ListItem: {
      color: "#1E1E1E",
      padding: "8px 16px 8px 16px"
    },
    ListItemUser: {
      color: "#1E1E1E",
      padding: "0px 16px 0px 16px"
    },
    ListItemTextUserContainer: {
      display: "flex",
      flexDirection: "column",
      textAlign: "center",
      paddingLeft: 0,
      paddingRight: 0,
    },
    ListItemTextUser: {
      fontSize: 12,
      color: "#7C7C7C"
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
      paddingRight: "20px"
    },
    User: {
      fontSize: 16
    },
    Account: {
      fontSize: 12,
      fontWeight: "bold",
      padding: "8px 0 8px 8px",
      backgroundColor: "#F1F1F1"
    },
}));



export const AccountSection = ({handleItemClick}) => {
  const classes = useStyles()
  const dispatch = useDispatch();

  const user = useSelector(state => state.appData.user);
  const isSupervisor = user.roles.includes("Supervisor") || user.roles.includes("HeadSupervisor")

  const handleLogout = () => dispatch(logout.invoke());

  if(isSupervisor) return null;

  return (
    <div className={classes.AccordionContainer}>
      <Typography className={classes.Account}>{strings(stringKeys.sideMenu.account)}</Typography>
      <Accordion square={false} className={classes.Accordion} classes={{ expanded: classes.AccordionExpanded }}>
        <AccordionSummary
          className={classes.AccordionSummary}
          classes={{
            root: classes.AccordionSummaryRoot,
            content: classes.AccordionSummaryContent,
            expanded: classes.AccordionSummaryExpanded,
          }}
          >
          <RcIcon icon="UserCircle" style={{
            fontSize: "24px",
          }}/>
          <Typography className={classes.User}>{user.name}</Typography>
        </AccordionSummary>
        <AccordionDetails className={classes.AccordionDetails}>
        <List component="nav" className={classes.List} aria-label="Side navigation menu" disablePadding>
          <ListItem className={classes.ListItemUser}>
            <ListItemText className={classes.ListItemTextUserContainer}>
              <Tooltip title={user.roles[0]}>
                <Typography noWrap className={classes.ListItemTextUser}>
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
          <ListItem button onClick={() => handleItemClick({ url: "/feedback" })} className={classes.ListItem}>
            <ListItemIcon className={classes.ListItemIcon}>
              <RcIcon icon="Feedback" className={classes.ListItemText}/>
            </ListItemIcon>
            <ListItemText classes={{ primary: classes.ListItemText, root: classes.ListItemTextWrapper }}>
              {strings(stringKeys.feedback.send)}
            </ListItemText>
          </ListItem>
          <ListItem button onClick={handleLogout} className={classes.ListItem}>
            <ListItemIcon className={classes.ListItemIcon}>
              <RcIcon icon="Logout" className={classes.ListItemText}/>
            </ListItemIcon>
            <ListItemText classes={{ primary: classes.ListItemText, root: classes.ListItemTextWrapper }}>
              {strings(stringKeys.user.logout)}
            </ListItemText>
          </ListItem>
        </List>
        </AccordionDetails>
      </Accordion>
    </div>
  )
}

AccountSection.propTypes = {
  logout: PropTypes.func,
  sendFeedback: PropTypes.func,
  isSendingFeedback: PropTypes.bool,
  sendFeedbackResult: PropTypes.string,
};