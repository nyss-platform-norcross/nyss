import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as projectsActions from './logic/projectsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import Button from '@material-ui/core/Button';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import ProjectsList from './ProjectsList';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';

const ProjectsListPageComponent = (props) => {
  useMount(() => {
    props.openProjectsList(props.nationalSocietyId);
  });

  return (
    <Fragment>
      <TableActions>
        <Button onClick={() => props.goToCreation(props.nationalSocietyId)} variant="outlined" color="primary" startIcon={<AddIcon />}>
          {strings(stringKeys.project.addNew)}
        </Button>
      </TableActions>

      <ProjectsList
        list={props.list}
        goToEdition={props.goToEdition}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        isRemoving={props.isRemoving}
        remove={props.remove}
        nationalSocietyId={props.nationalSocietyId}
      />
    </Fragment>
  );
}

ProjectsListPageComponent.propTypes = {
  getProjects: PropTypes.func,
  goToDashboard: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  list: state.projects.listData,
  isListFetching: state.projects.listFetching,
  isRemoving: state.projects.listRemoving
});

const mapDispatchToProps = {
  openProjectsList: projectsActions.openList.invoke,
  goToDashboard: projectsActions.goToDashboard,
  goToCreation: projectsActions.goToCreation,
  goToEdition: projectsActions.goToEdition,
  remove: projectsActions.remove.invoke
};

export const ProjectsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsListPageComponent)
);
