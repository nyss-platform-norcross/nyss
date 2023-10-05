import React, { useState } from 'react';
import { useDispatch, useSelector } from "react-redux";
import PropTypes from "prop-types";
import { List, ListItem, ListItemIcon, ListItemText, Accordion, AccordionSummary, AccordionDetails, Typography, makeStyles, Tooltip } from '@material-ui/core';
import { RcIcon } from '../icons/RcIcon';
import { logout } from '../../authentication/authActions';
import { sendFeedback } from '../app/logic/appActions';
import { FeedbackDialog } from '../feedback/FeedbackDialog';
import { strings, stringKeys } from '../../strings';

const useStyles = makeStyles(() => ({
    AccordionContainer: {
      width: "100%"
    },
    Accordion: {
      backgroundColor: '#F4F4F4',
      "&.MuiAccordion-root:before": {
        backgroundColor: "inherit",
      },
    },
    AccordionExpanded: {
      margin: 0
    },
    AccordionSummary: {
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
      margin: "0 0 8px 8px"
    },
}));



export const AccountSection = () => {
  const classes = useStyles()
  const dispatch = useDispatch();

  const [feedbackDialogOpened, setFeedbackDialogOpened] = useState(false);
  const isSendingFeedback = useSelector(state => state.appData.feedback.isSending);
  const sendFeedbackResult = useSelector(state => state.appData.feedback.result);
  const user = useSelector(state => state.appData.user);
  const isSupervisor = user.roles.includes("Supervisor") || user.roles.includes("HeadSupervisor")


  const handleFeedbackDialogClose = () => {
    setFeedbackDialogOpened(false);
  }

  const handleLogout = () => dispatch(logout.invoke());

  if(isSupervisor) return null;

  return (
    <div className={classes.AccordionContainer}>
      <Typography className={classes.Account}>{strings(stringKeys.sideMenu.account)}</Typography>
      <Accordion square={false} className={classes.Accordion} classes={{ expanded: classes.AccordionExpanded }}>
        <AccordionSummary
          style={{
            backgroundColor: '#EDEDED',
          }}
          classes={{
            root: classes.AccordionSummaryRoot,
            content: classes.AccordionSummary,
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
          <ListItem button onClick={() => setFeedbackDialogOpened(true)} className={classes.ListItem}>
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
          <FeedbackDialog
          isOpened={feedbackDialogOpened}
          close={handleFeedbackDialogClose}
          sendFeedback={sendFeedback.invoke}
          isSending={isSendingFeedback}
          result={sendFeedbackResult} />
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