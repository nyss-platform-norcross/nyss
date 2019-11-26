import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as dataCollectorsActions from './logic/dataCollectorsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import Typography from '@material-ui/core/Typography';
import Button from '@material-ui/core/Button';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import DataCollectorsTable from './DataCollectorsTable';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';

const DataCollectorsMapOverviewPageComponent = (props) => {
  useMount(() => {
    props.openDataCollectorsMapOverview(props.projectId);
  });

  return (
    <Fragment>
      {'bla'}
      {/* <TableActions>
        <Button onClick={() => props.goToCreation(props.projectId)} variant="outlined" color="primary" startIcon={<AddIcon />}>
          {strings(stringKeys.dataCollector.addNew)}
       </Button>
      </TableActions>

      <DataCollectorsTable
        list={props.list}
        goToEdition={props.goToEdition}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        isRemoving={props.isRemoving}
        remove={props.remove}
        projectId={props.projectId}
      /> */}
    </Fragment>
  );
}

DataCollectorsMapOverviewPageComponent.propTypes = {
   getDataCollectorsMapOverview: PropTypes.func,
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId
});

const mapDispatchToProps = {
  openDataCollectorsMapOverview: dataCollectorsActions.openMapOverview.invoke
};

export const DataCollectorsMapOverviewPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsMapOverviewPageComponent)
);
