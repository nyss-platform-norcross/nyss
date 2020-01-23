import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as nationalSocietyUsersActions from './logic/nationalSocietyUsersActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import Button from '@material-ui/core/Button';
import Box from '@material-ui/core/Box';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import NationalSocietyUsersTable from './NationalSocietyUsersTable';
import { useMount } from '../../utils/lifecycle';
import { stringKeys, strings } from '../../strings';

const NationalSocietyUsersListPageComponent = (props) => {
  useMount(() => {
    props.openNationalSocietyUsersList(props.nationalSocietyId);
  });

  return (
    <Fragment>
      {!props.nationalSocietyIsArchived &&
      <TableActions>
          <Box mr={2}>
            <Button onClick={() => props.goToAddExisting(props.nationalSocietyId)} variant="outlined" color="primary" startIcon={<AddIcon />}>
                {strings(stringKeys.nationalSocietyUser.addExisting)}
            </Button>
          </Box>

          <Button onClick={() => props.goToCreation(props.nationalSocietyId)} variant="outlined" color="primary" startIcon={<AddIcon />}>
            {strings(stringKeys.nationalSocietyUser.addNew)}
          </Button>
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

export const NationalSocietyUsersListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyUsersListPageComponent)
);
