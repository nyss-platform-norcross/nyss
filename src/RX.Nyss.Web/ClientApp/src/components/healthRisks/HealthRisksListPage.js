import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as healthRisksActions from './logic/healthRisksActions';
import * as appActions from '../app/logic/appActions';
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import HealthRisksTable from './HealthRisksTable';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { TableActionsButton } from '../common/buttons/tableActionsButton/TableActionsButton';

const HealthRisksListPageComponent = (props) => {
  useMount(() => {
    props.openModule(props.match.path, props.match.params);
    props.getList();
  });

  return (
    <Fragment>
      <TableActions>
        <TableActionsButton
          onClick={props.goToCreation}
          icon={<AddIcon />}
          variant={"contained"}
          color={"primary"}
        >
          {strings(stringKeys.healthRisk.addNew)}
        </TableActionsButton>
      </TableActions>

      <HealthRisksTable
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

HealthRisksListPageComponent.propTypes = {
  getHealthRisks: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = state => ({
  list: state.healthRisks.listData,
  isListFetching: state.healthRisks.listFetching,
  isRemoving: state.healthRisks.listRemoving
});

const mapDispatchToProps = {
  getList: healthRisksActions.getList.invoke,
  goToCreation: healthRisksActions.goToCreation,
  goToEdition: healthRisksActions.goToEdition,
  remove: healthRisksActions.remove.invoke,
  openModule: appActions.openModule.invoke
};

export const HealthRisksListPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(HealthRisksListPageComponent)
);
