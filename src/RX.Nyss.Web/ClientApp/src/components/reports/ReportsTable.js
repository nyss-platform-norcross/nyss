import React, { Fragment } from 'react';
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

export const ReportsTable = ({ isListFetching, list, projectId, getList, page, rowsPerPage, totalRows, reportListType, markAsError, unmarkAsError, isMarkingAsError, user, filters }) => {

  const onChangePage = (event, page) => {
    getList(projectId, page);
  };

  const dashIfEmpty = (text, ...args) => {
    return [text || "-", ...args].filter(x => !!x).join(", ");
  };

  const getMarkAsErrorMenuItem = (row) => {
    if(row.isMarkedAsError){
      return {
        title: strings(stringKeys.reports.list.unmarkAsError),
        action: () => unmarkAsError(row.reportId, projectId, page, filters)
      };
    } else{
      return  {
        title: strings(stringKeys.reports.list.markAsError),
        action: () => markAsError(row.reportId, projectId, page, filters)
      };
    }
  };

  const hasMarkAsErrorAccess = user.roles.filter((r) => { return accessMap.reports.markAsError.indexOf(r) !== -1; }).length > 0;

  return (
    <TableContainer>
      {isListFetching && <Loading absolute />}
      <Table stickyHeader>
        <TableHead>
          <TableRow>
            <TableCell style={{ width: "9%", minWidth: "115px" }}>{strings(stringKeys.nationalSocietyReports.list.date)}</TableCell>
            <TableCell style={{ width: "6%" }}>{strings(stringKeys.nationalSocietyReports.list.status)}</TableCell>
            <TableCell style={{ width: "11%" }}>{strings(stringKeys.nationalSocietyReports.list.dataCollectorDisplayName)}</TableCell>
            <TableCell style={{ width: "8%" }}>{strings(stringKeys.nationalSocietyReports.list.dataCollectorPhoneNumber)}</TableCell>
            <TableCell style={{ width: "18%" }}>{strings(stringKeys.nationalSocietyReports.list.location)}</TableCell>
            <TableCell style={{ width: "11%" }}>{strings(stringKeys.nationalSocietyReports.list.healthRisk)}</TableCell>
            <TableCell style={{ width: "6%" }}>{strings(stringKeys.nationalSocietyReports.list.malesBelowFive)}</TableCell>
            <TableCell style={{ width: "7%" }}>{strings(stringKeys.nationalSocietyReports.list.malesAtLeastFive)}</TableCell>
            <TableCell style={{ width: "6%" }}>{strings(stringKeys.nationalSocietyReports.list.femalesBelowFive)}</TableCell>
            <TableCell style={{ width: "7%" }}>{strings(stringKeys.nationalSocietyReports.list.femalesAtLeastFive)}</TableCell>
            {reportListType === "fromDcp" &&
              <Fragment>
                <TableCell style={{ width: "9%", minWidth: "50px" }}>{strings(stringKeys.reports.list.referredCount)}</TableCell>
                <TableCell style={{ width: "9%", minWidth: "50px" }}>{strings(stringKeys.reports.list.deathCount)}</TableCell>
                <TableCell style={{ width: "9%", minWidth: "50px" }}>{strings(stringKeys.reports.list.fromOtherVillagesCount)}</TableCell>
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
              <TableCell>{row.dataCollectorDisplayName}</TableCell>
              <TableCell>{row.phoneNumber}</TableCell>
              <TableCell>{dashIfEmpty(row.region, row.district, row.village, row.zone)}</TableCell>
              <TableCell>{dashIfEmpty(row.healthRiskName)}</TableCell>
              <TableCell>{dashIfEmpty(row.countMalesBelowFive)}</TableCell>
              <TableCell>{dashIfEmpty(row.countMalesAtLeastFive)}</TableCell>
              <TableCell>{dashIfEmpty(row.countFemalesBelowFive)}</TableCell>
              <TableCell>{dashIfEmpty(row.countFemalesAtLeastFive)}</TableCell>
              {reportListType === "fromDcp" &&
                <Fragment>
                  <TableCell>{dashIfEmpty(row.referredCount)}</TableCell>
                  <TableCell>{dashIfEmpty(row.deathCount)}</TableCell>
                  <TableCell>{dashIfEmpty(row.fromOtherVillagesCount)}</TableCell>
                </Fragment>
              }
              <TableCell>
                <TableRowActions>
                  {hasMarkAsErrorAccess && row.isValid && !row.isInAlert && (
                    <TableRowMenu
                      id={row.id}
                      icon={<MoreVertIcon />}
                      isFetching={isMarkingAsError[row.id]}
                      items={[
                        getMarkAsErrorMenuItem(row)
                      ]}
                    />
                  )}                  
                </TableRowActions>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <TablePager totalRows={totalRows} rowsPerPage={rowsPerPage} page={page} onChangePage={onChangePage} />
    </TableContainer>
  );
}

ReportsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default ReportsTable;
