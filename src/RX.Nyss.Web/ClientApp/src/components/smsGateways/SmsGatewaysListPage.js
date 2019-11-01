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

const SmsGatewaysListPageComponent = (props) => {
  useMount(() => {
    props.openSmsGatewaysList(props.match.path, props.match.params);
  });

  return (
    <Fragment>
      <Typography variant="h2">SMS Gateways</Typography>

      <TableActions>
        <Button onClick={() => props.goToCreation(props.match.params.nationalSocietyId)} variant="outlined" color="primary" startIcon={<AddIcon />}>
          Add SMS Gateway
       </Button>
      </TableActions>

      <SmsGatewaysTable
        list={props.list}
        goToEdition={props.goToEdition}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        isRemoving={props.isRemoving}
        remove={props.remove}
        nationalSocietyId={props.match.params.nationalSocietyId}
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
  list: PropTypes.array,
  //ToDo
  //match: PropTypes.shape()
};

const mapStateToProps = state => ({
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
