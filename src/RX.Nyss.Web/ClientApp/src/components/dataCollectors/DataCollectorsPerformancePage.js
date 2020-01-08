import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import Button from '@material-ui/core/Button';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import DataCollectorsPerformanceTable from './DataCollectorsPerformanceTable';
import * as dataCollectorActions from './logic/dataCollectorsActions';

const DataCollectorsPerformancePageComponent = (props) => {
  useMount(() => {
    props.openDataCollectorsPerformanceList(props.projectId);
  });

  return (
    <Fragment>
      <TableActions>
        <Button onClick={() => props.goToCreation(props.projectId)} variant="outlined" color="primary" startIcon={<AddIcon />}>
          {strings(stringKeys.dataCollector.addNew)}
       </Button>
      </TableActions>

      <DataCollectorsPerformanceTable
        list={props.list}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        projectId={props.projectId}
      />
    </Fragment>
  );
}

DataCollectorsPerformancePageComponent.propTypes = {
  openDataCollectorsPerformanceList: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  list: state.dataCollectors.performanceListData,
  isListFetching: state.dataCollectors.performanceListFetching,
});

const mapDispatchToProps = {
  openDataCollectorsPerformanceList: dataCollectorActions.openDataCollectorsPerformanceList.invoke
};

export const DataCollectorsPerformancePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsPerformancePageComponent)
);
