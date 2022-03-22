import styles from './ReportsTable.module.scss';
import { Fragment, useState } from 'react';
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
import { renderDataCollectorDisplayName, renderReportValue } from './logic/reportsService';

export const CorrectReportsTable = ({ isListFetching, isMarkingAsError, goToEditing, projectId,
  list, page, onChangePage, rowsPerPage, totalRows, filters, sorting, onSort, projectIsClosed,
  goToAlert, acceptReport, dismissReport, rtl }) => {
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

  const alertAllowsCrossCheckingOfReport = (alert) =>
    alert.status === alertStatus.open
    || (alert.status === alertStatus.escalated && !alert.reportWasCrossCheckedBeforeEscalation);

  const canCrossCheck = (report, reportStatus) =>
    !report.isAnonymized
    && report.isValid
    && !report.isMarkedAsError
    && !report.isActivityReport
    && report.reportType !== ReportType.dataCollectionPoint
    && report.status !== reportStatus
    && (!report.alert || alertAllowsCrossCheckingOfReport(report.alert))
    && !!report.village;

  const getRowMenu = (row) => [
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
    !row.isAnonymized
    && (!row.isActivityReport || filters.dataCollectorType === DataCollectorType.unknownSender)
    && row.status === reportStatus.new
    && !projectIsClosed
    && !row.dataCollectorIsDeleted;

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
                <TableCell>{dashIfEmpty(!row.isActivityReport && (strings(stringKeys.reports.status[row.status])))}</TableCell>
                <TableCell className={styles.phoneNumber}>
                  {renderDataCollectorDisplayName(row)}
                </TableCell>
                <TableCell>{dashIfEmpty(row.region, row.district, row.village, row.zone)}</TableCell>
                <TableCell>{dashIfEmpty(row.healthRiskName)}</TableCell>
                <TableCell>{renderReportValue(row.countMalesBelowFive)}</TableCell>
                <TableCell>{renderReportValue(row.countMalesAtLeastFive)}</TableCell>
                <TableCell>{renderReportValue(row.countFemalesBelowFive)}</TableCell>
                <TableCell>{renderReportValue(row.countFemalesAtLeastFive)}</TableCell>
                {filters.dataCollectorType === DataCollectorType.collectionPoint &&
                <Fragment>
                  <TableCell>{renderReportValue(row.referredCount)}</TableCell>
                  <TableCell>{renderReportValue(row.deathCount)}</TableCell>
                  <TableCell>{renderReportValue(row.fromOtherVillagesCount)}</TableCell>
                </Fragment>
                }
                <TableCell>
                  <TableRowActions directionRtl={rtl}>
                    <TableRowAction
                      onClick={() => goToEditing(projectId, row.id)}
                      icon={<EditIcon />}
                      title={strings(stringKeys.reports.list.editReport)}
                      roles={accessMap.reports.edit}
                      condition={canEdit(row)}
                      directionRtl={rtl}
                    />
                    { !row.isActivityReport &&
                    <TableRowMenu
                      id={row.id}
                      icon={<MoreVertIcon />}
                      isFetching={isMarkingAsError[row.id]}
                      items={getRowMenu(row)}
                      directionRtl={rtl}
                    />
                    }
                  </TableRowActions>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        <TablePager totalRows={totalRows} rowsPerPage={rowsPerPage} page={page} onChangePage={handlePageChange} rtl={rtl} />
      </TableContainer>
    </Fragment>
  );
}

CorrectReportsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default CorrectReportsTable;
