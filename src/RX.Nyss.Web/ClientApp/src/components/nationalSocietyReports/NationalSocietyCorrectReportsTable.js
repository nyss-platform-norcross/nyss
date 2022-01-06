import styles from "./NationalSocietyReportsTable.module.scss";

import { Fragment, useState } from 'react';
import PropTypes from "prop-types";
import { TableContainer } from '../common/table/TableContainer';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import dayjs from "dayjs";
import TablePager from '../common/tablePagination/TablePager';
import { DataCollectorType } from '../common/filters/logic/reportFilterConstsants'
import { DateColumnName } from './logic/nationalSocietyReportsConstants'
import {
  Typography,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableSortLabel,
} from "@material-ui/core";
import { renderDataCollectorDisplayName, renderReportValue } from "../reports/logic/reportsService";

export const NationalSocietyCorrectReportsTable = ({ isListFetching, list, page, onChangePage, rowsPerPage, totalRows, reportsType, filters, sorting, onSort }) => {

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

  return (
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
                {strings(stringKeys.nationalSocietyReports.list.date)}
              </TableSortLabel>
            </TableCell>
            <TableCell style={{ width: "6%" }}>{strings(stringKeys.nationalSocietyReports.list.status)}</TableCell>
            <TableCell style={{ width: "11%" }}>{strings(stringKeys.nationalSocietyReports.list.project)}</TableCell>
            <TableCell style={{ width: "11%" }}>{strings(stringKeys.nationalSocietyReports.list.dataCollectorDisplayName)}</TableCell>
            <TableCell style={{ width: "14%" }}>{strings(stringKeys.nationalSocietyReports.list.location)}</TableCell>
            <TableCell style={{ width: "11%" }}>{strings(stringKeys.nationalSocietyReports.list.healthRisk)}</TableCell>
            {!filters.status &&
              <TableCell style={{ width: "11%" }}>{strings(stringKeys.nationalSocietyReports.list.message)}</TableCell>
            }
            <TableCell style={{ width: "7%" }}>{strings(stringKeys.nationalSocietyReports.list.malesBelowFive)}</TableCell>
            <TableCell style={{ width: "8%" }}>{strings(stringKeys.nationalSocietyReports.list.malesAtLeastFive)}</TableCell>
            <TableCell style={{ width: "7%" }}>{strings(stringKeys.nationalSocietyReports.list.femalesBelowFive)}</TableCell>
            <TableCell style={{ width: "8%" }}>{strings(stringKeys.nationalSocietyReports.list.femalesAtLeastFive)}</TableCell>
            {reportsType === DataCollectorType.collectionPoint &&
              <Fragment>
                <TableCell style={{ width: "10%", minWidth: "50px" }}>{strings(stringKeys.nationalSocietyReports.list.referredCount)}</TableCell>
                <TableCell style={{ width: "10%", minWidth: "50px" }}>{strings(stringKeys.nationalSocietyReports.list.deathCount)}</TableCell>
                <TableCell style={{ width: "10%", minWidth: "50px" }}>{strings(stringKeys.nationalSocietyReports.list.fromOtherVillagesCount)}</TableCell>
              </Fragment>
            }
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map(row => (
            <TableRow key={row.id} hover>
              <TableCell>
                <span>{dayjs(row.dateTime).format('YYYY-MM-DD HH:mm')}</span>
              </TableCell>
              <TableCell>
                {strings(stringKeys.reports.status[row.status])}
              </TableCell>
              <TableCell>{dashIfEmpty(row.projectName)}</TableCell>
              <TableCell>
                {renderDataCollectorDisplayName(row)}
              </TableCell>
              <TableCell>{dashIfEmpty(row.region, row.district, row.village, row.zone)}</TableCell>
              <TableCell>{dashIfEmpty(row.healthRiskName)}</TableCell>
              {!filters.status &&
                <TableCell>
                  <Typography className={styles.message} title={row.message}>{dashIfEmpty(row.message)}</Typography>
                </TableCell>
              }
              <TableCell>{renderReportValue(row.countMalesBelowFive)}</TableCell>
              <TableCell>{renderReportValue(row.countMalesAtLeastFive)}</TableCell>
              <TableCell>{renderReportValue(row.countFemalesBelowFive)}</TableCell>
              <TableCell>{renderReportValue(row.countFemalesAtLeastFive)}</TableCell>
              {reportsType === DataCollectorType.collectionPoint &&
                <Fragment>
                  <TableCell>{renderReportValue(row.referredCount)}</TableCell>
                  <TableCell>{renderReportValue(row.deathCount)}</TableCell>
                  <TableCell>{renderReportValue(row.fromOtherVillagesCount)}</TableCell>
                </Fragment>
              }
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <TablePager totalRows={totalRows} rowsPerPage={rowsPerPage} page={page} onChangePage={handlePageChange} />
    </TableContainer>
  );
}

NationalSocietyCorrectReportsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default NationalSocietyCorrectReportsTable;
