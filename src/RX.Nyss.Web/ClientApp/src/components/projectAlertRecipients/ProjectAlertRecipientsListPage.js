import React, { Fragment } from "react";
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import * as projectAlertRecipientsActions from "./logic/projectAlertRecipientsActions";
import ProjectAlertRecipientsTable from "./ProjectAlertRecipientsTable";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import { Typography } from "@material-ui/core";
import SettingsTableHeader from "../common/settingsTableHeader/SettingsTableHeader";
import { trackPageView } from "../../utils/appInsightsHelper";
import { makeStyles } from "@material-ui/core/styles";
import AddIcon from "@material-ui/icons/Add";

const useStyles = makeStyles(() => ({
  buttonContainer: {
    width:"100%",
    display:"flex", 
    justifyContent:"end",
    marginTop: 8,
    marginBottom: 4
  }
}));

const ProjectAlertRecipientsListPageComponent = (props) => {
  useMount(() => {
    props.openProjectAlertRecipientsList(props.projectId);

    // Track page view
    trackPageView("ProjectAlertRecipientsListPage");
  });

  const classes = useStyles();
  const useRtlDirection = useSelector(
    (state) => state.appData.direction === "rtl",
  );

  return (
    <Fragment>
      <SettingsTableHeader />
      <Typography variant="subtitle1">
        {strings(stringKeys.projectAlertRecipient.description)}
      </Typography>
      {!props.nationalSocietyIsArchived && !props.projectIsClosed && (
        <div className={classes.buttonContainer}>
        <TableActionsButton
            onClick={() => props.goToCreation(props.projectId)}
            add
            variant="contained"
            rtl={useRtlDirection}
            startIcon={<AddIcon />}
          >
            {strings(stringKeys.common.buttons.add)}
          </TableActionsButton>
          </div>
        )}
      <ProjectAlertRecipientsTable
        list={props.list}
        isListFetching={props.isListFetching}
        isRemoving={props.isRemoving}
        goToEdition={props.goToEdition}
        remove={props.remove}
        projectId={props.projectId}
        isClosed={props.projectIsClosed}
        rtl={useRtlDirection}
      />
    </Fragment>
  );
};

ProjectAlertRecipientsListPageComponent.propTypes = {
  getProjectAlertRecipients: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

const mapStateToProps = (state, ownProps) => ({
  list: state.projectAlertRecipients.listData,
  isListFetching: state.projectAlertRecipients.listFetching,
  isRemoving: state.projectAlertRecipients.listRemoving,
  nationalSocietyIsArchived:
    state.appData.siteMap.parameters.nationalSocietyIsArchived,
  projectIsClosed: state.appData.siteMap.parameters.projectIsClosed,
  projectId: ownProps.match.params.projectId,
});

const mapDispatchToProps = {
  openProjectAlertRecipientsList: projectAlertRecipientsActions.openList.invoke,
  goToCreation: projectAlertRecipientsActions.goToCreation,
  goToEdition: projectAlertRecipientsActions.goToEdition,
  remove: projectAlertRecipientsActions.remove.invoke,
};

export const ProjectAlertRecipientsListPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(ProjectAlertRecipientsListPageComponent),
);
