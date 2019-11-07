import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as smsGatewaysActions from './logic/smsGatewaysActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import Typography from '@material-ui/core/Typography';
import Button from '@material-ui/core/Button';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import SmsGatewaysTable from './SmsGatewaysTable';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';

const SmsGatewaysListPageComponent = (props) => {
  useMount(() => {
    props.openSmsGatewaysList(props.nationalSocietyId);
  });

  return (
    <Fragment>
      <Typography variant="h2">{strings(stringKeys.smsGateway.title)}</Typography>

      <TableActions>
        <Button onClick={() => props.goToCreation(props.nationalSocietyId)} variant="outlined" color="primary" startIcon={<AddIcon />}>
          {strings(stringKeys.smsGateway.addNew)}
       </Button>
      </TableActions>

      <SmsGatewaysTable
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

SmsGatewaysListPageComponent.propTypes = {
  getSmsGateways: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  list: state.smsGateways.listData,
  isListFetching: state.smsGateways.listFetching,
  isRemoving: state.smsGateways.listRemoving
});

const mapDispatchToProps = {
  openSmsGatewaysList: smsGatewaysActions.openList.invoke,
  goToCreation: smsGatewaysActions.goToCreation,
  goToEdition: smsGatewaysActions.goToEdition,
  remove: smsGatewaysActions.remove.invoke
};

export const SmsGatewaysListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(SmsGatewaysListPageComponent)
);
