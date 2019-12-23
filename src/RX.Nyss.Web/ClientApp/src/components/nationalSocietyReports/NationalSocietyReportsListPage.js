import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as nationalSocietyReportsActions from './logic/nationalSocietyReportsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import NationalSocietyReportsTable from './NationalSocietyReportsTable';
import { useMount } from '../../utils/lifecycle';
import Grid from '@material-ui/core/Grid';

const NationalSocietyReportsListPageComponent = (props) => {
  useMount(() => {
    props.openNationalSocietyReportsList(props.nationalSocietyId);
  });

  if (!props.data) {
    return null;
  }

  return (
    <Grid container spacing={3}>
      <Grid item xs={12}>
        <NationalSocietyReportsTable
          list={props.data.data}
          isListFetching={props.isListFetching}
          getList={props.getList}
          nationalSocietyId={props.nationalSocietyId}
          page={props.data.page}
          totalRows={props.data.totalRows}
          rowsPerPage={props.data.rowsPerPage}
        />
      </Grid>
    </Grid>
  );
}

NationalSocietyReportsListPageComponent.propTypes = {
  getNationalSocietyReports: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  data: state.nationalSocietyReports.paginatedListData,
  isListFetching: state.nationalSocietyReports.listFetching,
  isRemoving: state.nationalSocietyReports.listRemoving
});

const mapDispatchToProps = {
  openNationalSocietyReportsList: nationalSocietyReportsActions.openList.invoke,
  getList: nationalSocietyReportsActions.getList.invoke
};

export const NationalSocietyReportsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyReportsListPageComponent)
);
