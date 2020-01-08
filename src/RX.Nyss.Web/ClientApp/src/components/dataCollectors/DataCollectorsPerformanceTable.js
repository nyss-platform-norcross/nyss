import styles from './DataCollectorsPerformanceTable.module.scss';
import React from 'react';
import PropTypes from "prop-types";
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import { TableContainer } from '../common/table/TableContainer';
import { getMarkerIconFromStatus } from './logic/dataCollectorsService';

const createIcon = (status) => {
  return (<span class="material-icons">{getMarkerIconFromStatus(status)}</span>)
}

export const DataCollectorsPerformanceTable = ({ isListFetching, list, projectId }) => {
  if (isListFetching) {
    return <Loading />;
  }

  return (
    <TableContainer>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.name)}</TableCell>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.daysSinceLastReport)}</TableCell>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.statusLastWeek)}</TableCell>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.statusTwoWeeksAgo)}</TableCell>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.statusThreeWeeksAgo)}</TableCell>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.statusFourWeeksAgo)}</TableCell>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.statusFiveWeeksAgo)}</TableCell>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.statusSixWeeksAgo)}</TableCell>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.statusSevenWeeksAgo)}</TableCell>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.statusEightWeeksAgo)}</TableCell>
            <TableCell />
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map((row, index) => (
            <TableRow key={index} hover>
              <TableCell>{row.name}</TableCell>
              <TableCell>{row.daysSinceLastReport > -1 ? row.daysSinceLastReport : '-'}</TableCell>
              <TableCell>{createIcon(row.statusLastWeek)}</TableCell>
              <TableCell>{createIcon(row.statusTwoWeeksAgo)}</TableCell>
              <TableCell>{createIcon(row.statusThreeWeeksAgo)}</TableCell>
              <TableCell>{createIcon(row.statusFourWeeksAgo)}</TableCell>
              <TableCell>{createIcon(row.statusFiveWeeksAgo)}</TableCell>
              <TableCell>{createIcon(row.statusSixWeeksAgo)}</TableCell>
              <TableCell>{createIcon(row.statusSevenWeeksAgo)}</TableCell>
              <TableCell>{createIcon(row.statusEightWeeksAgo)}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

DataCollectorsPerformanceTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default DataCollectorsPerformanceTable;
