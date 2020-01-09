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
import { getIconFromStatus } from './logic/dataCollectorsService';
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
              <TableCell style={{textAlign: "center"}}>{row.daysSinceLastReport > -1 ? row.daysSinceLastReport : '-'}</TableCell>
              <TableCell style={{textAlign: "center"}}>
                <DataCollectorStatusIcon status={row.statusLastWeek} icon={getIconFromStatus(row.statusLastWeek)} />
              </TableCell>
              <TableCell style={{textAlign: "center"}}>
                <DataCollectorStatusIcon status={row.statusTwoWeeksAgo} icon={getIconFromStatus(row.statusTwoWeeksAgo)} />
              </TableCell>
              <TableCell style={{textAlign: "center"}}>
                <DataCollectorStatusIcon status={row.statusThreeWeeksAgo} icon={getIconFromStatus(row.statusThreeWeeksAgo)} />
              </TableCell>
              <TableCell style={{textAlign: "center"}}>
                <DataCollectorStatusIcon status={row.statusFourWeeksAgo} icon={getIconFromStatus(row.statusFourWeeksAgo)} />
              </TableCell>
              <TableCell style={{textAlign: "center"}}>
                <DataCollectorStatusIcon status={row.statusFiveWeeksAgo} icon={getIconFromStatus(row.statusFiveWeeksAgo)} />
              </TableCell>
              <TableCell style={{textAlign: "center"}}>
                <DataCollectorStatusIcon status={row.statusSixWeeksAgo} icon={getIconFromStatus(row.statusSixWeeksAgo)} />
              </TableCell>
              <TableCell style={{textAlign: "center"}}>
                <DataCollectorStatusIcon status={row.statusSevenWeeksAgo} icon={getIconFromStatus(row.statusSevenWeeksAgo)} />
              </TableCell>
              <TableCell style={{textAlign: "center"}}>
                <DataCollectorStatusIcon status={row.statusEightWeeksAgo} icon={getIconFromStatus(row.statusEightWeeksAgo)} />
              </TableCell>
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
