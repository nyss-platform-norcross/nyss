import React, { useEffect } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Layout } from '../../layout/Layout';
import { useLayout } from '../../../utils/layout';
import Typography from '@material-ui/core/Typography';
import * as nationalSocietiesActions from '../logic/nationalSocietiesActions';
import * as appActions from '../../app/logic/appActions';
import Button from '@material-ui/core/Button';
import AddIcon from '@material-ui/icons/Add';
import { TableActions } from '../../common/tableActions/TableActions';
import * as consts from "../logic/nationalSocietiesConstants";
import { NationalSocietiesTable } from './NationalSocietiesTable';

const NationalSocietiesListPageComponent = (props) => {
  useEffect(() => {
    props.getList()
    props.updateSiteMap(props.match.path, {})
  }, [])

  return (
    <div>
      <Typography variant="h2">National Societies</Typography>

      <TableActions>
        <Button onClick={props.add} variant="outlined" color="primary" startIcon={<AddIcon />}>
          Add national society
       </Button>
      </TableActions>

      <NationalSocietiesTable
        list={props.list}
        isFetching={props.isFetching}
        remove={props.remove}
        pendingRequests={props.pendingRequests}
      />
    </div>
  );
}

NationalSocietiesListPageComponent.propTypes = {
  getNationalSocieties: PropTypes.func,
  add: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  updateSiteMap: PropTypes.func,
  list: PropTypes.array
};

const mapStateToProps = state => ({
  list: state.nationalSocieties.list.data,
  pendingRequests: state.requests.pending,
  isFetching: state.requests.pending[consts.GET_NATIONAL_SOCIETIES.name]
});

const mapDispatchToProps = {
  getList: nationalSocietiesActions.getList.invoke,
  add: nationalSocietiesActions.add.invoke,
  remove: nationalSocietiesActions.remove.invoke,
  updateSiteMap: appActions.updateSiteMap
};

export const NationalSocietiesListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietiesListPageComponent)
);
