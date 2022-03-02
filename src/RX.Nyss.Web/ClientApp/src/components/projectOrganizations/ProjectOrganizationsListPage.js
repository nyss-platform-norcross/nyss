import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as projectOrganizationsActions from './logic/projectOrganizationsActions';
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import ProjectOrganizationsTable from './ProjectOrganizationsTable';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { TableActionsButton } from '../common/buttons/tableActionsButton/TableActionsButton';
import { accessMap } from '../../authentication/accessMap';

const ProjectOrganizationsListPageComponent = (props) => {
  useMount(() => {
    props.openProjectOrganizationsList(props.projectId);
  });

  return (
    <Fragment>
      {!props.nationalSocietyIsArchived && !props.projectIsClosed &&
      <TableActions>
        <TableActionsButton
          onClick={() => props.goToCreation(props.projectId)}
          icon={<AddIcon />}
          roles={accessMap.projectOrganizations.add}
          variant={"contained"}
        >
          {strings(stringKeys.common.buttons.add)}
        </TableActionsButton>
      </TableActions>}

      <ProjectOrganizationsTable
        list={props.list}
        isListFetching={props.isListFetching}
        isRemoving={props.isRemoving}
        remove={props.remove}
        projectId={props.projectId}
        isClosed={props.projectIsClosed}
      />
    </Fragment>
  );
}

ProjectOrganizationsListPageComponent.propTypes = {
  getProjectOrganizations: PropTypes.func,
  goToCreation: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  list: state.projectOrganizations.listData,
  isListFetching: state.projectOrganizations.listFetching,
  isRemoving: state.projectOrganizations.listRemoving,
  nationalSocietyIsArchived: state.appData.siteMap.parameters.nationalSocietyIsArchived,
  projectIsClosed: state.appData.siteMap.parameters.projectIsClosed
});

const mapDispatchToProps = {
  openProjectOrganizationsList: projectOrganizationsActions.openList.invoke,
  goToCreation: projectOrganizationsActions.goToCreation,
  remove: projectOrganizationsActions.remove.invoke
};

export const ProjectOrganizationsListPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectOrganizationsListPageComponent)
);
