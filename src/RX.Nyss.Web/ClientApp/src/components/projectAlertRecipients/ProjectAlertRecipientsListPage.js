import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as projectAlertRecipientsActions from './logic/projectAlertRecipientsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import ProjectAlertRecipientsTable from './ProjectAlertRecipientsTable';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { TableActionsButton } from '../common/tableActions/TableActionsButton';

const ProjectAlertRecipientsListPageComponent = (props) => {
  useMount(() => {
    props.openProjectAlertRecipientsList(props.projectId);
  });

  return (
    <Fragment>
      {!props.nationalSocietyIsArchived &&
      <TableActions>
        <TableActionsButton onClick={() => props.goToCreation(props.projectId)} icon={<AddIcon />}>
          {strings(stringKeys.projectAlertRecipient.addNew)}
        </TableActionsButton>
      </TableActions>}

      <ProjectAlertRecipientsTable
        list={props.list}
        isListFetching={props.isListFetching}
        isRemoving={props.isRemoving}
        goToEdition={props.goToEdition}
        remove={props.remove}
        projectId={props.projectId}
      />
    </Fragment>
  );
}

ProjectAlertRecipientsListPageComponent.propTypes = {
  getProjectAlertRecipients: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  list: state.projectAlertRecipients.listData,
  isListFetching: state.projectAlertRecipients.listFetching,
  isRemoving: state.projectAlertRecipients.listRemoving,
  nationalSocietyIsArchived: state.appData.siteMap.parameters.nationalSocietyIsArchived
});

const mapDispatchToProps = {
  openProjectAlertRecipientsList: projectAlertRecipientsActions.openList.invoke,
  goToCreation: projectAlertRecipientsActions.goToCreation,
  goToEdition: projectAlertRecipientsActions.goToEdition,
  remove: projectAlertRecipientsActions.remove.invoke
};

export const ProjectAlertRecipientsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectAlertRecipientsListPageComponent)
);
