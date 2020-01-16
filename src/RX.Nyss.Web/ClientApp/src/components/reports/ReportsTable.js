import styles from "./ReportsTable.module.scss";

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
import { accessMap } from '../../authentication/accessMap';
import { TableRowMenu } from '../common/tableRowAction/TableRowMenu';
import MoreVertIcon from '@material-ui/icons/MoreVert';
import { ConfirmationDialog } from '../common/ConfirmationDialog';
import { ReportListType } from '../common/filters/logic/reportFilterConstsants'
import { DateColumnName } from './logic/reportsConstants'
import TableSortLabel from '@material-ui/core/TableSortLabel';

export const ReportsTable = ({ isListFetching, isMarkingAsError, markAsError, user, list, page, onChangePage, rowsPerPage, totalRows, reportsType, sorting, onSort }) => {

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

  const hasMarkAsErrorAccess = user.roles.filter((r) => { return accessMap.reports.markAsError.indexOf(r) !== -1; }).length > 0;

  return (
    <Fragment>
      <TableContainer>
        {isListFetching && <Loading absolute />}
        <Table stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell style={{ width: "9%", minWidth: "115px" }} className={styles.tableHeader}>
                <TableSortLabel
                  active={sorting.orderBy === DateColumnName}
                  direction={sorting.sortAscending ? 'asc' : 'desc'}
                  onClick={createSortHandler(DateColumnName)}
                >
                  {strings(stringKeys.reports.list.date)}
                </TableSortLabel>
              </TableCell>
              <TableCell style={{ width: "6%" }} className={styles.tableHeader}>{strings(stringKeys.reports.list.status)}</TableCell>
              <TableCell style={{ width: "11%" }} className={styles.tableHeader}>{strings(stringKeys.reports.list.dataCollectorDisplayName)}</TableCell>
              <TableCell style={{ width: "8%" }} className={styles.tableHeader}>{strings(stringKeys.reports.list.dataCollectorPhoneNumber)}</TableCell>
              <TableCell style={{ width: "18%" }} className={styles.tableHeader}>{strings(stringKeys.reports.list.location)}</TableCell>
              <TableCell style={{ width: "11%" }} className={styles.tableHeader}>{strings(stringKeys.reports.list.healthRisk)}</TableCell>
              <TableCell style={{ width: "6%" }} className={styles.tableHeader}>{strings(stringKeys.reports.list.malesBelowFive)}</TableCell>
              <TableCell style={{ width: "7%" }} className={styles.tableHeader}>{strings(stringKeys.reports.list.malesAtLeastFive)}</TableCell>
              <TableCell style={{ width: "6%" }} className={styles.tableHeader}>{strings(stringKeys.reports.list.femalesBelowFive)}</TableCell>
              <TableCell style={{ width: "7%" }} className={styles.tableHeader}>{strings(stringKeys.reports.list.femalesAtLeastFive)}</TableCell>
              {reportsType === ReportListType.fromDcp &&
                <Fragment>
                  <TableCell style={{ width: "9%", minWidth: "50px" }} className={styles.tableHeader}>{strings(stringKeys.reports.list.referredCount)}</TableCell>
                  <TableCell style={{ width: "9%", minWidth: "50px" }} className={styles.tableHeader}>{strings(stringKeys.reports.list.deathCount)}</TableCell>
                  <TableCell style={{ width: "9%", minWidth: "50px" }} className={styles.tableHeader}>{strings(stringKeys.reports.list.fromOtherVillagesCount)}</TableCell>
                </Fragment>
              }
              <TableCell style={{ width: "3%" }} className={styles.tableHeader} />
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
                <TableCell>{row.dataCollectorDisplayName}</TableCell>
                <TableCell>{row.phoneNumber}</TableCell>
                <TableCell>{dashIfEmpty(row.region, row.district, row.village, row.zone)}</TableCell>
                <TableCell>{dashIfEmpty(row.healthRiskName)}</TableCell>
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
                    {hasMarkAsErrorAccess && row.isValid && !row.isInAlert && !row.isMarkedAsError && row.userHasAccessToReportDataCollector && (
                      <TableRowMenu
                        id={row.id}
                        icon={<MoreVertIcon />}
                        isFetching={isMarkingAsError[row.id]}
                        items={[
                          {
                            title: strings(stringKeys.reports.list.markAsError),
                            action: () => setMarkErrorConfirmationDialog({ isOpen: true, reportId: row.reportId, isMarkedAsError: row.isMarkedAsError })
                          }
                        ]}
                      />
                    )}
                  </TableRowActions>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        <TablePager totalRows={totalRows} rowsPerPage={rowsPerPage} page={page} onChangePage={handlePageChange} />
      </TableContainer>

      {hasMarkAsErrorAccess && (
        <ConfirmationDialog
          isOpened={markErrorConfirmationDialog.isOpen}
          isFetching={isMarkingAsError}
          titlteText={strings(stringKeys.reports.list.markAsErrorConfirmation)}
          contentText={strings(stringKeys.reports.list.markAsErrorConfirmationText)}
          submit={() => markAsErrorConfirmed()}
          close={() => setMarkErrorConfirmationDialog({ isOpen: false })}
        />
      )}
    </Fragment>
  );
}

ReportsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default ReportsTable;
