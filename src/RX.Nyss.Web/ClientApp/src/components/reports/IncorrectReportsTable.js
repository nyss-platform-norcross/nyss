import styles from '../common/table/Table.module.scss';

import React, { Fragment, useState } from 'react';
import PropTypes from 'prop-types';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import dayjs from 'dayjs';
import TablePager from '../common/tablePagination/TablePager';
import { TableContainer } from '../common/table/TableContainer';
import { DateColumnName, ReportErrorType, reportDetailedFormatErrors } from './logic/reportsConstants';
import {
  Typography,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableSortLabel,
} from '@material-ui/core';
import CorrectedButton from "./CorrectedButton";

export const IncorrectReportsTable = ({
  isListFetching,
  list,
  page,
  onChangePage,
  rowsPerPage,
  totalRows,
  filters,
  sorting,
  onSort,
  onCorrect,
}) => {

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

  const errorTypeString = (errorType) => {
    const errorTypeTranslation = strings(stringKeys.reports.errorTypes[errorType]);

    if (reportDetailedFormatErrors.some(e => e === errorType)) {
      return (
        <span>
          {strings(stringKeys.reports.errorTypes[ReportErrorType.formatError])}
          <br />
          {errorTypeTranslation}
        </span>
      );
    } else {
      return errorTypeTranslation;
    }
  }

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
              <TableCell style={{ width: '30%' }}>{strings(stringKeys.reports.list.errorType)}</TableCell>
              <TableCell style={{ width: '12%' }}>{strings(stringKeys.reports.list.message)}</TableCell>
              <TableCell style={{ width: '12%' }}>{strings(stringKeys.reports.list.dataCollectorDisplayName)}</TableCell>
              <TableCell style={{ width: '23%' }}>{strings(stringKeys.reports.list.location)}</TableCell>
              <TableCell style={{ width: '13%' }}>{strings(stringKeys.reports.list.corrected)}</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {list.map(row => (
              <TableRow key={row.id} hover>
                <TableCell>{dayjs(row.dateTime).format('YYYY-MM-DD HH:mm')}</TableCell>
                <TableCell>{errorTypeString(row.reportErrorType)}</TableCell>
                <TableCell>
                  <Typography className={styles.message} title={row.message}>{dashIfEmpty(row.message)}</Typography>
                </TableCell>
                <TableCell>
                  {row.dataCollectorDisplayName}
                  {!row.isAnonymized && row.dataCollectorDisplayName ? <br /> : ''}
                  {(!row.isAnonymized || !row.dataCollector) && row.phoneNumber}
                </TableCell>
                <TableCell>{dashIfEmpty(row.region, row.district, row.village, row.zone)}</TableCell>
                <TableCell>
                  <CorrectedButton onClick={() => onCorrect(row)} />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        <TablePager totalRows={totalRows} rowsPerPage={rowsPerPage} page={page} onChangePage={handlePageChange} />
      </TableContainer>
    </Fragment>
  )
}

IncorrectReportsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default IncorrectReportsTable;
