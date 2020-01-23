import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as projectsActions from './logic/projectsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import ProjectsTable from './ProjectsTable';
import { TableActionsButton } from '../common/tableActions/TableActionsButton';

const ProjectsListPageComponent = (props) => {
  useMount(() => {
    props.openProjectsList(props.nationalSocietyId);
  });

  return (
    <Fragment>
      {!props.nationalSocietyIsArchived && (
        <TableActions>
          <TableActionsButton onClick={() => props.goToCreation(props.nationalSocietyId)} icon={<AddIcon />}>
            {strings(stringKeys.project.addNew)}
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
      />
    </Fragment>
  );
}

ProjectsListPageComponent.propTypes = {
  getProjects: PropTypes.func,
  goToDashboard: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  close: PropTypes.func,
  isListFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  list: state.projects.listData,
  isListFetching: state.projects.listFetching,
  isClosing: state.projects.isClosing,
  callingUserRoles: state.appData.user.roles,
  isHeadManager: state.appData.siteMap.parameters.nationalSocietyHeadManagerId === state.appData.user.id,
  nationalSocietyIsArchived: state.appData.siteMap.parameters.nationalSocietyIsArchived
});

const mapDispatchToProps = {
  openProjectsList: projectsActions.openList.invoke,
  goToDashboard: projectsActions.goToDashboard,
  goToCreation: projectsActions.goToCreation,
  goToEdition: projectsActions.goToEdition,
  close: projectsActions.close.invoke
};

export const ProjectsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsListPageComponent)
);
