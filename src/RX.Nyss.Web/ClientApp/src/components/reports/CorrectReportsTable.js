import React, { Fragment, useState } from 'react';
import PropTypes from 'prop-types';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import dayjs from 'dayjs';
import TablePager from '../common/tablePagination/TablePager';
import { TableContainer } from '../common/table/TableContainer';
import { TableRowActions } from '../common/tableRowAction/TableRowActions';
import { TableRowAction } from '../common/tableRowAction/TableRowAction';
import EditIcon from '@material-ui/icons/Edit';
import { accessMap } from '../../authentication/accessMap';
import { TableRowMenu } from '../common/tableRowAction/TableRowMenu';
import MoreVertIcon from '@material-ui/icons/MoreVert';
import { ConfirmationDialog } from '../common/confirmationDialog/ConfirmationDialog';
import { DataCollectorType } from '../common/filters/logic/reportFilterConstsants';
import { DateColumnName, reportStatus, ReportType } from './logic/reportsConstants';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableSortLabel,
} from '@material-ui/core';
import { alertStatus } from '../alerts/logic/alertsConstants';

export const CorrectReportsTable = ({ isListFetching, isMarkingAsError, markAsError, goToEditing, projectId,
  list, page, onChangePage, rowsPerPage, totalRows, filters, sorting, onSort, projectIsClosed,
  goToAlert, acceptReport, dismissReport }) => {

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
    return [text || '-', ...args].filter(x => !!x).join(', ');
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

  const canMarkAsError = (row) =>
    !projectIsClosed
    && !row.isAnonymized
    && row.isValid
    && !row.alert
    && !row.isMarkedAsError
    && !row.isActivityReport;

  const alertAllowsCrossCheckingOfReport = (alert) =>
    alert.status === alertStatus.pending
    || (alert.status === alertStatus.escalated && !alert.reportWasCrossCheckedBeforeEscalation);

  const canCrossCheck = (report, reportStatus) =>
    !report.isAnonymized
    && report.isValid
    && !report.isMarkedAsError
    && !report.isActivityReport
    && report.reportType !== ReportType.dataCollectionPoint
    && (!report.alert || (report.status !== reportStatus && alertAllowsCrossCheckingOfReport(report.alert)));

  const getRowMenu = (row) => [
    {
      title: strings(stringKeys.reports.list.markAsError),
      roles: accessMap.reports.markAsError,
      disabled: !canMarkAsError(row),
      action: () => setMarkErrorConfirmationDialog({ isOpen: true, reportId: row.id, isMarkedAsError: row.isMarkedAsError })
    },
    {
      title: strings(stringKeys.reports.list.goToAlert),
      roles: accessMap.reports.goToAlert,
      disabled: !row.alert,
      action: () => goToAlert(projectId, row.alert.id)
    },
    {
      title: strings(stringKeys.reports.list.acceptReport),
      roles: accessMap.reports.crossCheck,
      disabled: !canCrossCheck(row, reportStatus.accepted),
      action: () => acceptReport(row.id)
    },
    {
      title: strings(stringKeys.reports.list.dismissReport),
      roles: accessMap.reports.crossCheck,
      disabled: !canCrossCheck(row, reportStatus.rejected),
      action: () => dismissReport(row.id)
    }
  ];

  const canEdit = (row) => 
    (!row.isAnonymized || !row.dataCollector)
    && !row.isActivityReport
    && !projectIsClosed;

  return (
    <Fragment>
      <TableContainer sticky>
        {isListFetching && <Loading absolute />}
        <Table>
          <TableHead>
            <TableRow>
              <TableCell style={{ width: '6%', minWidth: '80px' }}>
                <TableSortLabel
                  active={sorting.orderBy === DateColumnName}
                  direction={sorting.sortAscending ? 'asc' : 'desc'}
                  onClick={createSortHandler(DateColumnName)}
                >
                  {strings(stringKeys.reports.list.date)}
                </TableSortLabel>
              </TableCell>
              <TableCell style={{ width: '6%' }}>{strings(stringKeys.reports.list.status)}</TableCell>
              <TableCell style={{ width: '12%' }}>{strings(stringKeys.reports.list.dataCollectorDisplayName)}</TableCell>
              <TableCell style={{ width: '20%' }}>{strings(stringKeys.reports.list.location)}</TableCell>
              <TableCell style={{ width: '14%' }}>{strings(stringKeys.reports.list.healthRisk)}</TableCell>
              <TableCell style={{ width: '7%' }}>{strings(stringKeys.reports.list.malesBelowFive)}</TableCell>
              <TableCell style={{ width: '8%' }}>{strings(stringKeys.reports.list.malesAtLeastFive)}</TableCell>
              <TableCell style={{ width: '7%' }}>{strings(stringKeys.reports.list.femalesBelowFive)}</TableCell>
              <TableCell style={{ width: '8%' }}>{strings(stringKeys.reports.list.femalesAtLeastFive)}</TableCell>
              {filters.dataCollectorType === DataCollectorType.collectionPoint &&
                <Fragment>
                  <TableCell style={{ width: '10%', minWidth: '50px' }}>{strings(stringKeys.reports.list.referredCount)}</TableCell>
                  <TableCell style={{ width: '10%', minWidth: '50px' }}>{strings(stringKeys.reports.list.deathCount)}</TableCell>
                  <TableCell style={{ width: '10%', minWidth: '50px' }}>{strings(stringKeys.reports.list.fromOtherVillagesCount)}</TableCell>
                </Fragment>
              }
              <TableCell style={{ width: '3%' }} />
            </TableRow>
          </TableHead>
          <TableBody>
            {list.map(row => (
              <TableRow key={row.id} hover>
                <TableCell>{dayjs(row.dateTime).format('YYYY-MM-DD HH:mm')}</TableCell>
                <TableCell>
                  {strings(stringKeys.reports.status[row.status])}
                </TableCell>
                <TableCell>
                  {row.dataCollectorDisplayName}
                  {!row.isAnonymized && row.dataCollectorDisplayName ? <br /> : ''}
                  {(!row.isAnonymized || !row.dataCollector) && row.phoneNumber}
                </TableCell>
                <TableCell>{dashIfEmpty(row.region, row.district, row.village, row.zone)}</TableCell>
                <TableCell>{dashIfEmpty(row.healthRiskName)}</TableCell>
                <TableCell>{dashIfEmpty(row.countMalesBelowFive)}</TableCell>
                <TableCell>{dashIfEmpty(row.countMalesAtLeastFive)}</TableCell>
                <TableCell>{dashIfEmpty(row.countFemalesBelowFive)}</TableCell>
                <TableCell>{dashIfEmpty(row.countFemalesAtLeastFive)}</TableCell>
                {filters.dataCollectorType === DataCollectorType.collectionPoint &&
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
                      onClick={() => goToEditing(projectId, row.id)}
                      icon={<EditIcon />}
                      title={strings(stringKeys.reports.list.editReport)}
                      roles={accessMap.reports.edit}
                      condition={canEdit(row)} />
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

CorrectReportsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default CorrectReportsTable;
