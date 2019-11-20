import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as reportsActions from './logic/reportsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import Typography from '@material-ui/core/Typography';
import Button from '@material-ui/core/Button';
import AddIcon from '@material-ui/icons/Add';
import TableActions from '../common/tableActions/TableActions';
import ReportsTable from './ReportsTable';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';

const ReportsListPageComponent = (props) => {
  useMount(() => {
    props.openReportsList(props.projectId);
  });

  return (
    <Fragment>
      <Typography variant="h2">{strings(stringKeys.reports.list.title)}</Typography>
     
      <ReportsTable
        list={props.list}
        isListFetching={props.isListFetching}
      />
    </Fragment>
  );
}

ReportsListPageComponent.propTypes = {
  getReports: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  list: state.reports.listData,
  isListFetching: state.reports.listFetching,
  isRemoving: state.reports.listRemoving
});

const mapDispatchToProps = {
  openReportsList: reportsActions.openList.invoke,      
};

export const ReportsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ReportsListPageComponent)
);
