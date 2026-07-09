import React, { Fragment } from "react";
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import * as projectsActions from "./logic/projectsActions";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import TableActions from "../common/tableActions/TableActions";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import ProjectsTable from "./ProjectsTable";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { accessMap } from "../../authentication/accessMap";
import { trackPageView } from "../../utils/appInsightsHelper";

const ProjectsListPageComponent = (props) => {
  useMount(() => {
    props.openProjectsList(props.nationalSocietyId);

    // Track page view
    trackPageView("ProjectsListPage");
  });

  const userLanguageCode = useSelector(
    (state) => state.appData.user.languageCode,
  );

  return (
    <Fragment>
      {!props.nationalSocietyIsArchived && (
        <TableActions>
          <TableActionsButton
            onClick={() => props.goToCreation(props.nationalSocietyId)}
            add
            roles={accessMap.projects.add}
            variant="contained"
            rtl={userLanguageCode === "ar"}
          >
            {strings(stringKeys.common.buttons.add)}
          </TableActionsButton>
        </TableActions>
      )}

      <ProjectsTable
        list={props.list}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        nationalSocietyId={props.nationalSocietyId}
        close={props.close}
        isClosing={props.isClosing}
        callingUserRoles={props.callingUserRoles}
        isHeadManager={props.isHeadManager}
        rtl={userLanguageCode === "ar"}
      />
    </Fragment>
  );
};

ProjectsListPageComponent.propTypes = {
  getProjects: PropTypes.func,
  goToDashboard: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  close: PropTypes.func,
  isListFetching: PropTypes.bool,
  list: PropTypes.array,
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  list: state.projects.listData,
  isListFetching: state.projects.listFetching,
  isClosing: state.projects.isClosing,
  callingUserRoles: state.appData.user.roles,
  isHeadManager: state.appData.siteMap.parameters.isCurrentUserHeadManager,
  nationalSocietyIsArchived:
    state.appData.siteMap.parameters.nationalSocietyIsArchived,
});

const mapDispatchToProps = {
  openProjectsList: projectsActions.openList.invoke,
  goToDashboard: projectsActions.goToDashboard,
  goToCreation: projectsActions.goToCreation,
  goToEdition: projectsActions.goToEdition,
  close: projectsActions.close.invoke,
};

export const ProjectsListPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsListPageComponent),
);
