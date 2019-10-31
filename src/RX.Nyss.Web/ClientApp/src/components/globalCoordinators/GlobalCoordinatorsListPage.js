import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as globalCoordinatorsActions from './logic/globalCoordinatorsActions';
import * as appActions from '../app/logic/appActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import Typography from '@material-ui/core/Typography';
import Button from '@material-ui/core/Button';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import GlobalCoordinatorsTable from './GlobalCoordinatorsTable';
import { useMount } from '../../utils/lifecycle';

const GlobalCoordinatorsListPageComponent = (props) => {
  useMount(() => {
    props.openModule(props.match.path, props.match.params);
    props.getList();
  });

  return (
    <Fragment>
      <Typography variant="h2">Global Coordinators</Typography>

      <TableActions>
        <Button onClick={props.goToCreation} variant="outlined" color="primary" startIcon={<AddIcon />}>
          Add Global Coordinator
       </Button>
      </TableActions>

      <GlobalCoordinatorsTable
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

GlobalCoordinatorsListPageComponent.propTypes = {
  getGlobalCoordinators: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = state => ({
  list: state.globalCoordinators.listData,
  isListFetching: state.globalCoordinators.listFetching,
  isRemoving: state.globalCoordinators.listRemoving
});

const mapDispatchToProps = {
  getList: globalCoordinatorsActions.getList.invoke,
  goToCreation: globalCoordinatorsActions.goToCreation,
  goToEdition: globalCoordinatorsActions.goToEdition,
  remove: globalCoordinatorsActions.remove.invoke,
  openModule: appActions.openModule.invoke
};

export const GlobalCoordinatorsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(GlobalCoordinatorsListPageComponent)
);
