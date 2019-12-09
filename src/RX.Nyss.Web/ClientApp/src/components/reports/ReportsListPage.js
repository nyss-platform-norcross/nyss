import React, { useState } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as reportsActions from './logic/reportsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import ReportsTable from './ReportsTable';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { MenuItem, Select, Grid, FormControl, InputLabel } from '@material-ui/core';

const ReportsListPageComponent = (props) => {
  useMount(() => {
    props.openReportsList(props.projectId);
  });

  const [reportListFilter, setReportListFilter] = useState(props.reportListFilter);

  if (!props.data || !props.reportListFilter) {
    return null;
  }

  const handleReportListTypeChange = event => {
    const newFilter = {
      ...reportListFilter,
      ...{
        reportListType: event.target.value
      }
    }

    setReportListFilter(newFilter);
    props.getList(props.projectId, props.page, newFilter);
  }

  return (
    <Grid container spacing={3}>
      <Grid item xs={12}>
        <Grid container spacing={3}>
          <Grid item xs={6}>
            <FormControl style={{ minWidth: '200px' }}>
              <InputLabel>{strings(stringKeys.reports.list.selectReportListType)}</InputLabel>
              <Select
                onChange={handleReportListTypeChange}
                value={props.reportListFilter.reportListType}
              >
                <MenuItem value="main">
                  {strings(stringKeys.reports.list.mainReportsListType)}
                </MenuItem>
                <MenuItem value="training">
                  {strings(stringKeys.reports.list.trainingReportsListType)}
                </MenuItem>
              </Select>
            </FormControl>

          </Grid>
        </Grid>
      </Grid>

      <Grid item xs={12}>
        <ReportsTable
          list={props.data.data}
          isListFetching={props.isListFetching}
          getList={props.getList}
          projectId={props.projectId}
          page={props.data.page}
          totalRows={props.data.totalRows}
          rowsPerPage={props.data.rowsPerPage}
        />
      </Grid>
    </Grid>
  );
}

ReportsListPageComponent.propTypes = {
  getReports: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  data: state.reports.paginatedListData,
  isListFetching: state.reports.listFetching,
  isRemoving: state.reports.listRemoving,
  reportListFilter: state.reports.filter
});

const mapDispatchToProps = {
  openReportsList: reportsActions.openList.invoke,
  getList: reportsActions.getList.invoke
};

export const ReportsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ReportsListPageComponent)
);
