import styles from './DataCollectorsPerformanceTable.module.scss';
import React, { useState } from 'react';
import { useSelector } from 'react-redux';
import PropTypes from "prop-types";
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import { strings, stringKeys } from '../../strings';
import { TableContainer } from '../common/table/TableContainer';
import { getIconFromStatus } from './logic/dataCollectorsService';
import { DataCollectorStatusIcon } from '../common/icon/DataCollectorStatusIcon';
import { Loading } from '../common/loading/Loading';
import Icon from '@material-ui/core/Icon';
import { DataCollectorsPerformanceColumnFilters } from './DataCollectorsPerformanceColumnFilters';

export const DataCollectorsPerformanceTable = ({ list, isListFetching, filters, getDataCollectorPerformanceList }) => {
  const [isOpen, setIsOpen] = useState(false);
  const [anchorEl, setAnchorEl] = useState(null);
  const [statusFilters, setStatusFilters] = useState(null);
  const projectId = useSelector(state => state.appData.siteMap.parameters.projectId);

  const openFilter = (event) => {
    setAnchorEl(event.currentTarget);
    setStatusFilters(getStatusFilter(event.currentTarget.id));
    setIsOpen(true);
  }

  const getStatusFilter = (status) => {
    switch (status) {
      case 'lastWeek': return filters.lastWeek;
      case 'twoWeeksAgo': return filters.twoWeeksAgo;
      case 'threeWeeksAgo': return filters.threeWeeksAgo;
      case 'fourWeeksAgo': return filters.fourWeeksAgo;
      case 'fiveWeeksAgo': return filters.fiveWeeksAgo;
      case 'sixWeeksAgo': return filters.sixWeeksAgo;
      case 'sevenWeeksAgo': return filters.sevenWeeksAgo;
      case 'eightWeeksAgo': return filters.eightWeeksAgo;
      default: return null;
    }
  }

  const filterIsActive = (status) => {
    switch (status) {
      case 'lastWeek': return !filters.lastWeek.reportingCorrectly || !filters.lastWeek.reportingWithErrors || !filters.lastWeek.notReporting;
      case 'twoWeeksAgo': return !filters.twoWeeksAgo.reportingCorrectly || !filters.twoWeeksAgo.reportingWithErrors || !filters.twoWeeksAgo.notReporting;
      case 'threeWeeksAgo': return !filters.threeWeeksAgo.reportingCorrectly || !filters.threeWeeksAgo.reportingWithErrors || !filters.threeWeeksAgo.notReporting;
      case 'fourWeeksAgo': return !filters.fourWeeksAgo.reportingCorrectly || !filters.fourWeeksAgo.reportingWithErrors || !filters.fourWeeksAgo.notReporting;
      case 'fiveWeeksAgo': return !filters.fiveWeeksAgo.reportingCorrectly || !filters.fiveWeeksAgo.reportingWithErrors || !filters.fiveWeeksAgo.notReporting;
      case 'sixWeeksAgo': return !filters.sixWeeksAgo.reportingCorrectly || !filters.sixWeeksAgo.reportingWithErrors || !filters.sixWeeksAgo.notReporting;
      case 'sevenWeeksAgo': return !filters.sevenWeeksAgo.reportingCorrectly || !filters.sevenWeeksAgo.reportingWithErrors || !filters.sevenWeeksAgo.notReporting;
      case 'eightWeeksAgo': return !filters.eightWeeksAgo.reportingCorrectly || !filters.eightWeeksAgo.reportingWithErrors || !filters.eightWeeksAgo.notReporting;
      default: return false;
    }
  }

  const handleClose = (fields) => {
    statusFilters.reportingCorrectly = fields.reportingCorrectly.value;
    statusFilters.reportingWithErrors = fields.reportingWithErrors.value;
    statusFilters.notReporting = fields.notReporting.value;
    setIsOpen(false);
    getDataCollectorPerformanceList(projectId, filters);
  }

  return !!filters && (
    <TableContainer sticky>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.name)}</TableCell>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.daysSinceLastReport)}</TableCell>
            <TableCell>
              <div className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusLastWeek)}
                {filterIsActive('lastWeek') &&
                  <Icon className={styles.filterActiveIcon}>arrow_drop_down</Icon>
                }
                <Icon id="lastWeek" onClick={openFilter} style={{ fontSize: 15 }}>filter_alt</Icon>
              </div>

            </TableCell>
            <TableCell>
              <div className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusTwoWeeksAgo)}
                {filterIsActive('twoWeeksAgo') &&
                  <Icon className={styles.filterActiveIcon}>arrow_drop_down</Icon>
                }
                <Icon id="twoWeeksAgo" onClick={openFilter} className={styles.filterIcon}>filter_alt</Icon>
              </div>
            </TableCell>
            <TableCell>
              <div className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusThreeWeeksAgo)}
                {filterIsActive('threeWeeksAgo') &&
                  <Icon className={styles.filterActiveIcon}>arrow_drop_down</Icon>
                }
                <Icon id="threeWeeksAgo" onClick={openFilter} className={styles.filterIcon}>filter_alt</Icon>
              </div>
            </TableCell>
            <TableCell>
              <div className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusFourWeeksAgo)}
                {filterIsActive('fourWeeksAgo') &&
                  <Icon className={styles.filterActiveIcon}>arrow_drop_down</Icon>
                }
                <Icon id="fourWeeksAgo" onClick={openFilter} className={styles.filterIcon}>filter_alt</Icon>
              </div>
            </TableCell>
            <TableCell>
              <div className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusFiveWeeksAgo)}
                {filterIsActive('fiveWeeksAgo') &&
                  <Icon className={styles.filterActiveIcon}>arrow_drop_down</Icon>
                }
                <Icon id="fiveWeeksAgo" onClick={openFilter} className={styles.filterIcon}>filter_alt</Icon>
              </div>
            </TableCell>
            <TableCell>
              <div className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusSixWeeksAgo)}
                {filterIsActive('sixWeeksAgo') &&
                  <Icon className={styles.filterActiveIcon}>arrow_drop_down</Icon>
                }
                <Icon id="sixWeeksAgo" onClick={openFilter} className={styles.filterIcon}>filter_alt</Icon>
              </div>
            </TableCell>
            <TableCell>
              <div className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusSevenWeeksAgo)}
                {filterIsActive('sevenWeeksAgo') &&
                  <Icon className={styles.filterActiveIcon}>arrow_drop_down</Icon>
                }
                <Icon id="sevenWeeksAgo" onClick={openFilter} className={styles.filterIcon}>filter_alt</Icon>
              </div>
            </TableCell>
            <TableCell>
              <div className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusEightWeeksAgo)}
                {filterIsActive('eightWeeksAgo') &&
                  <Icon className={styles.filterActiveIcon}>arrow_drop_down</Icon>
                }
                <Icon id="eightWeeksAgo" onClick={openFilter} className={styles.filterIcon}>filter_alt</Icon>
              </div>
            </TableCell>
            <TableCell />
          </TableRow>
        </TableHead>
        <TableBody>
          {isListFetching && <Loading />}

          {!isListFetching && (
            list.map((row, index) => (
              <TableRow key={index} hover>
                <TableCell>{row.name}</TableCell>
                <TableCell style={{ textAlign: "center" }}>{row.daysSinceLastReport > -1 ? row.daysSinceLastReport : '-'}</TableCell>
                <TableCell style={{ textAlign: "center" }}>
                  <DataCollectorStatusIcon status={row.statusLastWeek} icon={getIconFromStatus(row.statusLastWeek)} />
                </TableCell>
                <TableCell style={{ textAlign: "center" }}>
                  <DataCollectorStatusIcon status={row.statusTwoWeeksAgo} icon={getIconFromStatus(row.statusTwoWeeksAgo)} />
                </TableCell>
                <TableCell style={{ textAlign: "center" }}>
                  <DataCollectorStatusIcon status={row.statusThreeWeeksAgo} icon={getIconFromStatus(row.statusThreeWeeksAgo)} />
                </TableCell>
                <TableCell style={{ textAlign: "center" }}>
                  <DataCollectorStatusIcon status={row.statusFourWeeksAgo} icon={getIconFromStatus(row.statusFourWeeksAgo)} />
                </TableCell>
                <TableCell style={{ textAlign: "center" }}>
                  <DataCollectorStatusIcon status={row.statusFiveWeeksAgo} icon={getIconFromStatus(row.statusFiveWeeksAgo)} />
                </TableCell>
                <TableCell style={{ textAlign: "center" }}>
                  <DataCollectorStatusIcon status={row.statusSixWeeksAgo} icon={getIconFromStatus(row.statusSixWeeksAgo)} />
                </TableCell>
                <TableCell style={{ textAlign: "center" }}>
                  <DataCollectorStatusIcon status={row.statusSevenWeeksAgo} icon={getIconFromStatus(row.statusSevenWeeksAgo)} />
                </TableCell>
                <TableCell style={{ textAlign: "center" }}>
                  <DataCollectorStatusIcon status={row.statusEightWeeksAgo} icon={getIconFromStatus(row.statusEightWeeksAgo)} />
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>

      <DataCollectorsPerformanceColumnFilters
        open={isOpen}
        anchorEl={anchorEl}
        filters={statusFilters}
        onClose={handleClose} />
    </TableContainer>
  );
}

DataCollectorsPerformanceTable.propTypes = {
  isListFetching: PropTypes.bool,
  list: PropTypes.array,
  filters: PropTypes.object,
  getDataCollectorPerformanceList: PropTypes.func
};

export default DataCollectorsPerformanceTable;
