import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as nationalSocietiesActions from './logic/nationalSocietiesActions';
import * as appActions from '../app/logic/appActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import Typography from '@material-ui/core/Typography';
import Button from '@material-ui/core/Button';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import NationalSocietiesTable from './NationalSocietiesTable';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';

const NationalSocietiesListPageComponent = ({ showStringsKeys, match, openModule, getList, ...props }) => {
  useMount(() => {
    openModule(match.path, match.params);
    getList();
  })

  return (
    <Fragment>
      <TableActions>
        <Button onClick={props.goToCreation} variant="outlined" color="primary" startIcon={<AddIcon />}>
          {strings(stringKeys.nationalSociety.addNew)}
       </Button>
      </TableActions>

      <NationalSocietiesTable
        list={props.list}
        goToEdition={props.goToEdition}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        isRemoving={props.isRemoving}
        remove={props.remove}
      />
    </Fragment>
  );
}

NationalSocietiesListPageComponent.propTypes = {
  getNationalSocieties: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = state => ({
  list: state.nationalSocieties.listData,
  isListFetching: state.nationalSocieties.listFetching,
  isRemoving: state.nationalSocieties.listRemoving
});

const mapDispatchToProps = {
  getList: nationalSocietiesActions.getList.invoke,
  goToCreation: nationalSocietiesActions.goToCreation,
  goToEdition: nationalSocietiesActions.goToEdition,
  goToDashboard: nationalSocietiesActions.goToDashboard,
  remove: nationalSocietiesActions.remove.invoke,
  openModule: appActions.openModule.invoke
};

export const NationalSocietiesListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietiesListPageComponent)
);
