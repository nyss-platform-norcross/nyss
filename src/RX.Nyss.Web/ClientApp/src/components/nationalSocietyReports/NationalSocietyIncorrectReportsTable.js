import styles from './NationalSocietyReportsTable.module.scss';

import { useState } from 'react';
import PropTypes from 'prop-types';
import { TableContainer } from '../common/table/TableContainer';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import dayjs from 'dayjs';
import TablePager from '../common/tablePagination/TablePager';
import { DateColumnName } from './logic/nationalSocietyReportsConstants';
import {
  Typography,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableSortLabel,
} from '@material-ui/core';
import { renderDataCollectorDisplayName } from '../reports/logic/reportsService';

export const NationalSocietyIncorrectReportsTable = ({ isListFetching, list, page, onChangePage, rowsPerPage, totalRows, sorting, onSort }) => {

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

  return (
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
                {strings(stringKeys.nationalSocietyReports.list.date)}
              </TableSortLabel>
            </TableCell>
            <TableCell style={{ width: '40%' }}>{strings(stringKeys.nationalSocietyReports.list.errorType)}</TableCell>
            <TableCell style={{ width: '11%' }}>{strings(stringKeys.nationalSocietyReports.list.message)}</TableCell>
            <TableCell style={{ width: '11%' }}>{strings(stringKeys.nationalSocietyReports.list.project)}</TableCell>
            <TableCell style={{ width: '11%' }}>{strings(stringKeys.nationalSocietyReports.list.dataCollectorDisplayName)}</TableCell>
            <TableCell style={{ width: '14%' }}>{strings(stringKeys.nationalSocietyReports.list.location)}</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map(row => (
            <TableRow key={row.id} hover>
              <TableCell>
                <span>{dayjs(row.dateTime).format('YYYY-MM-DD HH:mm')}</span>
              </TableCell>
              <TableCell>
                {strings(stringKeys.reports.errorTypes[row.errorType])}
              </TableCell>
              <TableCell>
                <Typography className={styles.message} title={row.message}>{dashIfEmpty(row.message)}</Typography>
              </TableCell>
              <TableCell>{dashIfEmpty(row.projectName)}</TableCell>
              <TableCell>
                {renderDataCollectorDisplayName(row)}
              </TableCell>
              <TableCell>{dashIfEmpty(row.region, row.district, row.village, row.zone)}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <TablePager totalRows={totalRows} rowsPerPage={rowsPerPage} page={page} onChangePage={handlePageChange} />
    </TableContainer>
  );
}

NationalSocietyIncorrectReportsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default NationalSocietyIncorrectReportsTable;
