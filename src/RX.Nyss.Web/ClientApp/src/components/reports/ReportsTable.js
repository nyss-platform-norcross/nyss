import styles from '../common/table/Table.module.scss';

import React, { Fragment, useState } from 'react';
import PropTypes from "prop-types";
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import dayjs from "dayjs";
import TablePager from '../common/tablePagination/TablePager';
import { TableContainer } from '../common/table/TableContainer';
import { TableRowActions } from '../common/tableRowAction/TableRowActions';
import { TableRowAction } from '../common/tableRowAction/TableRowAction';
import EditIcon from '@material-ui/icons/Edit';
import { accessMap } from '../../authentication/accessMap';
import { TableRowMenu } from '../common/tableRowAction/TableRowMenu';
import MoreVertIcon from '@material-ui/icons/MoreVert';
import { ConfirmationDialog } from '../common/confirmationDialog/ConfirmationDialog';
import { ReportListType } from '../common/filters/logic/reportFilterConstsants'
import { DateColumnName } from './logic/reportsConstants'
import TableSortLabel from '@material-ui/core/TableSortLabel';
import { Typography } from "@material-ui/core";

export const ReportsTable = ({ isListFetching, isMarkingAsError, markAsError, goToEdition, projectId, user, list, page, onChangePage, rowsPerPage, totalRows, reportsType, filters, sorting, onSort, projectIsClosed, goToAlert }) => {
  
  const [markErrorConfirmationDialog, setMarkErrorConfirmationDialog] = useState({ isOpen: false, reportId: null, isMarkedAsError: null });
  const [value, setValue] = useState(sorting);

  const updateValue = (change) => {
    const newValue = {
      ...value,
      ...change
    }

    setValue(newValue);
    return newValue;
  };

  const dashIfEmpty = (text, ...args) => {
    return [text || "-", ...args].filter(x => !!x).join(", ");
  };

  const createSortHandler = column => event => {
    handleSortChange(event, column);
  };

  const handleSortChange = (event, column) => {
    const isAscending = sorting.orderBy === column && sorting.sortAscending;
    onSort(updateValue({ orderBy: column, sortAscending: !isAscending }));
  }

  const handlePageChange = (event, page) => {
    onChangePage(page);
  }

  const markAsErrorConfirmed = () => {
    markAsError(markErrorConfirmationDialog.reportId);
    setMarkErrorConfirmationDialog({ isOpen: false })
  }

  const getRowMenu = (row) => [
    {
      title: strings(stringKeys.reports.list.markAsError),
      roles: accessMap.reports.markAsError,
      condition: !projectIsClosed && !row.isAnonymized && row.isValid && !row.alertId && !row.isMarkedAsError && row.userHasAccessToReportDataCollector,
      action: () => setMarkErrorConfirmationDialog({ isOpen: true, reportId: row.reportId, isMarkedAsError: row.isMarkedAsError })
    },
    {
      title: strings(stringKeys.reports.list.goToAlert),
      roles: accessMap.reports.goToAlert,
      condition: !row.isAnonymized && !!row.alertId,
      action: () => goToAlert(projectId, row.alertId)
    }
  ];

  return (
    <Fragment>
      <TableContainer sticky>
        {isListFetching && <Loading absolute />}
        <Table>
          <TableHead>
            <TableRow>
              <TableCell style={{ width: "6%", minWidth: "80px" }}>
                <TableSortLabel
                  active={sorting.orderBy === DateColumnName}
                  direction={sorting.sortAscending ? 'asc' : 'desc'}
                  onClick={createSortHandler(DateColumnName)}
                >
                  {strings(stringKeys.reports.list.date)}
                </TableSortLabel>
              </TableCell>
              <TableCell style={{ width: "6%" }}>{strings(stringKeys.reports.list.status)}</TableCell>
              <TableCell style={{ width: "12%" }}>{strings(stringKeys.reports.list.dataCollectorDisplayName)}</TableCell>
              <TableCell style={{ width: "20%" }}>{strings(stringKeys.reports.list.location)}</TableCell>
              <TableCell style={{ width: "14%" }}>{strings(stringKeys.reports.list.healthRisk)}</TableCell>
              {!filters.status &&
                <TableCell style={{ width: "12%" }}>{strings(stringKeys.reports.list.message)}</TableCell>
              }
              <TableCell style={{ width: "7%" }}>{strings(stringKeys.reports.list.malesBelowFive)}</TableCell>
              <TableCell style={{ width: "8%" }}>{strings(stringKeys.reports.list.malesAtLeastFive)}</TableCell>
              <TableCell style={{ width: "7%" }}>{strings(stringKeys.reports.list.femalesBelowFive)}</TableCell>
              <TableCell style={{ width: "8%" }}>{strings(stringKeys.reports.list.femalesAtLeastFive)}</TableCell>
              {reportsType === ReportListType.fromDcp &&
                <Fragment>
                  <TableCell style={{ width: "10%", minWidth: "50px" }}>{strings(stringKeys.reports.list.referredCount)}</TableCell>
                  <TableCell style={{ width: "10%", minWidth: "50px" }}>{strings(stringKeys.reports.list.deathCount)}</TableCell>
                  <TableCell style={{ width: "10%", minWidth: "50px" }}>{strings(stringKeys.reports.list.fromOtherVillagesCount)}</TableCell>
                </Fragment>
              }
              <TableCell style={{ width: "3%" }} />
            </TableRow>
          </TableHead>
          <TableBody>
            {list.map(row => (
              <TableRow key={row.id} hover>
                <TableCell>{dayjs(row.dateTime).format('YYYY-MM-DD HH:mm')}</TableCell>
                <TableCell>
                  {row.isMarkedAsError
                    ? strings(stringKeys.reports.list.markedAsError)
                    : row.isValid ? strings(stringKeys.reports.list.success) : strings(stringKeys.reports.list.error)}
                </TableCell>
                <TableCell>
                  {row.dataCollectorDisplayName}
                  {!row.isAnonymized && row.dataCollectorDisplayName ? <br /> : ""}
                  {!row.isAnonymized && row.phoneNumber}
                </TableCell>
                <TableCell>{dashIfEmpty(row.region, row.district, row.village, row.zone)}</TableCell>
                <TableCell>{dashIfEmpty(row.healthRiskName)}</TableCell>
                {!filters.status &&
                  <TableCell>
                    <Typography className={styles.message} title={row.message}>{dashIfEmpty(row.message)}</Typography>
                  </TableCell>
                }
                <TableCell>{dashIfEmpty(row.countMalesBelowFive)}</TableCell>
                <TableCell>{dashIfEmpty(row.countMalesAtLeastFive)}</TableCell>
                <TableCell>{dashIfEmpty(row.countFemalesBelowFive)}</TableCell>
                <TableCell>{dashIfEmpty(row.countFemalesAtLeastFive)}</TableCell>
                {reportsType === ReportListType.fromDcp &&
                  <Fragment>
                    <TableCell>{dashIfEmpty(row.referredCount)}</TableCell>
                    <TableCell>{dashIfEmpty(row.deathCount)}</TableCell>
                    <TableCell>{dashIfEmpty(row.fromOtherVillagesCount)}</TableCell>
                  </Fragment>
                }
                <TableCell>
                  <TableRowActions>
                    <TableRowMenu
                      id={row.id}
                      icon={<MoreVertIcon />}
                      isFetching={isMarkingAsError[row.id]}
                      items={getRowMenu(row)}
                    />
                    <TableRowAction
                      onClick={() => goToEdition(projectId, row.reportId)}
                      icon={<EditIcon />}
                      title={strings(stringKeys.reports.list.editReport)}
                      roles={accessMap.reports.edit}
                      condition={!row.isAnonymized && (row.reportType === "Aggregate" || row.reportType === "DataCollectionPoint")} />
                  </TableRowActions>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        <TablePager totalRows={totalRows} rowsPerPage={rowsPerPage} page={page} onChangePage={handlePageChange} />
      </TableContainer>

      <ConfirmationDialog
        isOpened={markErrorConfirmationDialog.isOpen}
        isFetching={isMarkingAsError}
        titleText={strings(stringKeys.reports.list.markAsErrorConfirmation)}
        contentText={strings(stringKeys.reports.list.markAsErrorConfirmationText)}
        submit={() => markAsErrorConfirmed()}
        close={() => setMarkErrorConfirmationDialog({ isOpen: false })}
        roles={accessMap.reports.markAsError}
      />
    </Fragment>
  );
}

ReportsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default ReportsTable;
