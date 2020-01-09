import React, { useState } from 'react';
import styles from "./ReportsListPage.module.scss";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as reportsActions from './logic/reportsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import ReportsTable from './ReportsTable';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { FormControlLabel, Radio, RadioGroup } from '@material-ui/core';
import MenuItem from '@material-ui/core/MenuItem';
import Select from '@material-ui/core/Select';
import Grid from '@material-ui/core/Grid';
import FormControl from '@material-ui/core/FormControl';
import InputLabel from '@material-ui/core/InputLabel';
import Button from '@material-ui/core/Button';
import Link from '@material-ui/core/Link';
import TableActions from '../common/tableActions/TableActions';

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
      ...props.reportListFilter,
      ...{
        reportListType: event.target.value
      }
    }

    setReportListFilter(newFilter);
    props.getList(props.projectId, props.page, newFilter);
  }

  const handleIsTrainingChange = event => {
    const newFilter = {
      ...props.reportListFilter,
      ...{
        isTraining: event.target.value === "true"
      }
    }
    setReportListFilter(newFilter);
    props.getList(props.projectId, props.page, newFilter);
  }

  return (
    <Grid container spacing={3}>
      
      <Grid item xs={12}>

      <TableActions>          
          <Button onClick={() => props.exportToExcel(props.projectId, props.reportListFilter)} variant="outlined" color="primary">
            {strings(stringKeys.reports.list.exportToExcel)}
          </Button>
      </TableActions>

        <Grid container spacing={3}>
          <Grid item>
            <FormControl style={{ minWidth: '250px' }}>
              <InputLabel>{strings(stringKeys.reports.list.selectReportListType)}</InputLabel>
              <Select
                onChange={handleReportListTypeChange}
                value={props.reportListFilter.reportListType}
              >
                <MenuItem value="main">
                  {strings(stringKeys.reports.list.mainReportsListType)}
                </MenuItem>
                <MenuItem value="fromDcp">
                  {strings(stringKeys.reports.list.dcpReportListType)}
                </MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item>
            <InputLabel className={styles.trainingStateLabel}>{strings(stringKeys.reports.list.trainingReportsListType)}</InputLabel>
            <RadioGroup
              value={props.reportListFilter.isTraining}
              onChange={handleIsTrainingChange}
              className={styles.trainingStateRadioGroup}
            >
              <FormControlLabel control={<Radio />} label={strings(stringKeys.reports.list.nonTraining)} value={false} />
              <FormControlLabel control={<Radio />} label={strings(stringKeys.reports.list.training)} value={true} />
            </RadioGroup >
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
          reportListType={props.reportListFilter.reportListType}
          markAsError = {props.markAsError}
          isMarkingAsError ={props.isMarkingAsError}
          user = {props.user}
          filters = {props.reportListFilter}
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
  reportListFilter: state.reports.filter,
  user: state.appData.user,
  isMarkingAsError: state.reports.markingAsError
});

const mapDispatchToProps = {
  openReportsList: reportsActions.openList.invoke,
  getList: reportsActions.getList.invoke,
  exportToExcel: reportsActions.exportToExcel.invoke,
  markAsError: reportsActions.markAsError.invoke
};

export const ReportsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ReportsListPageComponent)
);
