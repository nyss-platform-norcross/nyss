import React, { useState } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as nationalSocietyReportsActions from './logic/nationalSocietyReportsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import NationalSocietyReportsTable from './NationalSocietyReportsTable';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import FormControl from '@material-ui/core/FormControl';
import InputLabel from '@material-ui/core/InputLabel';
import Select from '@material-ui/core/Select';
import MenuItem from '@material-ui/core/MenuItem';

const NationalSocietyReportsListPageComponent = (props) => {
  useMount(() => {
    props.openNationalSocietyReportsList(props.nationalSocietyId);
  });

  const [reportListFilter, setReportListFilter] = useState(props.reportListFilter);

  if (!props.data || !props.reportListFilter) {
    return null;
  }

  const handleReportListTypeChange = event => {
    const newFilter = {
      ...props.reportListFilter,
      ...{
        reportListType: event.target.value
      }
    }

    setReportListFilter(newFilter);
    props.getList(props.nationalSocietyId, props.page, newFilter);
  }

  return (
    <Grid container spacing={3}>
      <Grid item xs={12}>
        <Grid container spacing={3}>
          <Grid item>
            <FormControl style={{ minWidth: '250px' }}>
              <InputLabel>{strings(stringKeys.nationalSocietyReports.list.selectReportListType)}</InputLabel>
              <Select
                onChange={handleReportListTypeChange}
                value={props.reportListFilter.reportListType}
              >
                <MenuItem value="unknownSender">
                  {strings(stringKeys.nationalSocietyReports.list.unknownSenderReportListType)}
                </MenuItem>
                <MenuItem value="main">
                  {strings(stringKeys.nationalSocietyReports.list.mainReportsListType)}
                </MenuItem>
                <MenuItem value="fromDcp">
                  {strings(stringKeys.nationalSocietyReports.list.dcpReportListType)}
                </MenuItem>
              </Select>
            </FormControl>
          </Grid>
        </Grid>
      </Grid>

      <Grid item xs={12}>
        <NationalSocietyReportsTable
          list={props.data.data}
          isListFetching={props.isListFetching}
          getList={props.getList}
          nationalSocietyId={props.nationalSocietyId}
          page={props.data.page}
          totalRows={props.data.totalRows}
          rowsPerPage={props.data.rowsPerPage}
          reportListType={props.reportListFilter.reportListType}
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
  isRemoving: state.nationalSocietyReports.listRemoving,
  reportListFilter: state.nationalSocietyReports.filter
});

const mapDispatchToProps = {
  openNationalSocietyReportsList: nationalSocietyReportsActions.openList.invoke,
  getList: nationalSocietyReportsActions.getList.invoke
};

export const NationalSocietyReportsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyReportsListPageComponent)
);
