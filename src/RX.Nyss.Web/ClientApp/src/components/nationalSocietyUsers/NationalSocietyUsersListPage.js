import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as nationalSocietyUsersActions from './logic/nationalSocietyUsersActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import Typography from '@material-ui/core/Typography';
import Button from '@material-ui/core/Button';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import NationalSocietyUsersTable from './NationalSocietyUsersTable';
import { useMount } from '../../utils/lifecycle';

const NationalSocietyUsersListPageComponent = (props) => {
  useMount(() => {
    props.openNationalSocietyUsersList(props.nationalSocietyId);
  });

  return (
    <Fragment>
      <Typography variant="h2">Users</Typography>

      <TableActions>
        <Button onClick={() => props.goToCreation(props.nationalSocietyId)} variant="outlined" color="primary" startIcon={<AddIcon />}>
          Add User
       </Button>
      </TableActions>

      <NationalSocietyUsersTable
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

NationalSocietyUsersListPageComponent.propTypes = {
  getNationalSocietyUsers: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  list: state.nationalSocietyUsers.listData,
  isListFetching: state.nationalSocietyUsers.listFetching,
  isRemoving: state.nationalSocietyUsers.listRemoving
});

const mapDispatchToProps = {
  openNationalSocietyUsersList: nationalSocietyUsersActions.openList.invoke,
  goToCreation: nationalSocietyUsersActions.goToCreation,
  goToEdition: nationalSocietyUsersActions.goToEdition,
  remove: nationalSocietyUsersActions.remove.invoke
};

export const NationalSocietyUsersListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyUsersListPageComponent)
);
