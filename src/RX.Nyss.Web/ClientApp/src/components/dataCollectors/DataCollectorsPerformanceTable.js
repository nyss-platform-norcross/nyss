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
import { DataCollectorStatusIcon } from '../common/icon/DataCollectorStatusIcon';

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
              <TableCell><DataCollectorStatusIcon status={row.statusLastWeek} icon={getMarkerIconFromStatus(row.statusLastWeek)} /></TableCell>
              <TableCell><DataCollectorStatusIcon status={row.statusTwoWeeksAgo} icon={getMarkerIconFromStatus(row.statusTwoWeeksAgo)} /></TableCell>
              <TableCell><DataCollectorStatusIcon status={row.statusThreeWeeksAgo} icon={getMarkerIconFromStatus(row.statusThreeWeeksAgo)} /></TableCell>
              <TableCell><DataCollectorStatusIcon status={row.statusFourWeeksAgo} icon={getMarkerIconFromStatus(row.statusFourWeeksAgo)} /></TableCell>
              <TableCell><DataCollectorStatusIcon status={row.statusFiveWeeksAgo} icon={getMarkerIconFromStatus(row.statusFiveWeeksAgo)} /></TableCell>
              <TableCell><DataCollectorStatusIcon status={row.statusSixWeeksAgo} icon={getMarkerIconFromStatus(row.statusSixWeeksAgo)} /></TableCell>
              <TableCell><DataCollectorStatusIcon status={row.statusSevenWeeksAgo} icon={getMarkerIconFromStatus(row.statusSevenWeeksAgo)} /></TableCell>
              <TableCell><DataCollectorStatusIcon status={row.statusEightWeeksAgo} icon={getMarkerIconFromStatus(row.statusEightWeeksAgo)} /></TableCell>
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
