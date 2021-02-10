import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as nationalSocietyUsersActions from './logic/nationalSocietyUsersActions';
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import NationalSocietyUsersTable from './NationalSocietyUsersTable';
import { useMount } from '../../utils/lifecycle';
import { stringKeys, strings } from '../../strings';
import { TableActionsButton } from '../common/tableActions/TableActionsButton';

const NationalSocietyUsersListPageComponent = (props) => {
  useMount(() => {
    props.openNationalSocietyUsersList(props.nationalSocietyId);
  });

  return (
    <Fragment>
      {!props.nationalSocietyIsArchived &&
        <TableActions>
          <TableActionsButton onClick={() => props.goToAddExisting(props.nationalSocietyId)} icon={<AddIcon />}>
            {strings(stringKeys.nationalSocietyUser.addExisting)}
          </TableActionsButton>

          <TableActionsButton onClick={() => props.goToCreation(props.nationalSocietyId)} icon={<AddIcon />}>
            {strings(stringKeys.nationalSocietyUser.addNew)}
          </TableActionsButton>
        </TableActions>}

      <NationalSocietyUsersTable
        list={props.list}
        goToEdition={props.goToEdition}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        isRemoving={props.isRemoving}
        isSettingAsHead={props.isSettingAsHead}
        remove={props.remove}
        nationalSocietyId={props.nationalSocietyId}
        setAsHeadManager={props.setAsHeadManager}
        user={props.user}
      />
    </Fragment>
  );
}

NationalSocietyUsersListPageComponent.propTypes = {
  getNationalSocietyUsers: PropTypes.func,
  goToCreation: PropTypes.func,
  goToAddExisting: PropTypes.func,
  goToEdition: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  user: state.appData.user,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  list: state.nationalSocietyUsers.listData,
  isListFetching: state.nationalSocietyUsers.listFetching,
  isRemoving: state.nationalSocietyUsers.listRemoving,
  isSettingAsHead: state.nationalSocietyUsers.settingAsHead,
  nationalSocietyIsArchived: state.appData.siteMap.parameters.nationalSocietyIsArchived
});

const mapDispatchToProps = {
  openNationalSocietyUsersList: nationalSocietyUsersActions.openList.invoke,
  goToCreation: nationalSocietyUsersActions.goToCreation,
  goToAddExisting: nationalSocietyUsersActions.goToAddExisting,
  goToEdition: nationalSocietyUsersActions.goToEdition,
  remove: nationalSocietyUsersActions.remove.invoke,
  setAsHeadManager: nationalSocietyUsersActions.setAsHeadManager.invoke,
};

export const NationalSocietyUsersListPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyUsersListPageComponent)
);
