import styles from './DataCollectorsPerformanceTable.module.scss';

import React, { useState } from 'react';
import PropTypes from "prop-types";
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import Icon from '@material-ui/core/Icon';
import Tooltip from '@material-ui/core/Tooltip';
import InfoIcon from '@material-ui/icons/InfoOutlined';
import { strings, stringKeys } from '../../strings';
import { TableContainer } from '../common/table/TableContainer';
import { getIconFromStatus } from './logic/dataCollectorsService';
import { DataCollectorStatusIcon } from '../common/icon/DataCollectorStatusIcon';
import { DataCollectorsPerformanceColumnFilters } from './DataCollectorsPerformanceColumnFilters';
import TablePager from '../common/tablePagination/TablePager';

export const DataCollectorsPerformanceTable = ({ list, completeness, page, rowsPerPage, totalRows, isListFetching, filters, onChange }) => {
  const [isOpen, setIsOpen] = useState(false);
  const [anchorEl, setAnchorEl] = useState(null);
  const [selectedWeek, setSelectedWeek] = useState(null);
  const [statusFilters, setStatusFilters] = useState(null);

  const openFilter = (event) => {
    setAnchorEl(event.currentTarget);
    setSelectedWeek(event.currentTarget.id);
    setStatusFilters(getStatusFilter(event.currentTarget.id));
    setIsOpen(true);
  }

  const getStatusFilter = (status) => {
    switch (status) {
      case 'lastWeek': return Object.assign({}, filters.lastWeek);
      case 'twoWeeksAgo': return Object.assign({}, filters.twoWeeksAgo);
      case 'threeWeeksAgo': return Object.assign({}, filters.threeWeeksAgo);
      case 'fourWeeksAgo': return Object.assign({}, filters.fourWeeksAgo);
      case 'fiveWeeksAgo': return Object.assign({}, filters.fiveWeeksAgo);
      case 'sixWeeksAgo': return Object.assign({}, filters.sixWeeksAgo);
      case 'sevenWeeksAgo': return Object.assign({}, filters.sevenWeeksAgo);
      case 'eightWeeksAgo': return Object.assign({}, filters.eightWeeksAgo);
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

  const onChangePage = (e, page) => {
    onChange({ type: 'changePage', pageNumber: page });
  }

  const handleClose = (fields) => {
    onChange({ type: 'updateSorting', week: selectedWeek, filters: fields });
    setIsOpen(false);
  }

  const handleTooltipClick = e => e.stopPropagation();

  const renderTooltipText = (completeness) => {
    let text = strings(stringKeys.dataCollector.performanceList.completenessValueDescription);

    if (typeof (text) === 'string') { // only replace values when not in "strings editing" mode
      text = text.replace('{{active}}', completeness.activeDataCollectors);
      text = text.replace('{{total}}', completeness.totalDataCollectors);
    }

    return text;
  }

  return !!filters && (
    <TableContainer sticky isFetching={isListFetching}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.name)}</TableCell>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.villageName)}</TableCell>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.daysSinceLastReport)}</TableCell>
            <TableCell>
              <div id="lastWeek" onClick={openFilter} className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusLastWeek)}
                <Icon className={styles.filterIcon}>{filterIsActive('lastWeek') ? 'filter_alt' : 'expand_more'}</Icon>
              </div>
              </TableCell>
              <TableCell>
              <div id="twoWeeksAgo" onClick={openFilter} className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusTwoWeeksAgo)}
                <Icon className={styles.filterIcon}>{filterIsActive('twoWeeksAgo') ? 'filter_alt' : 'expand_more'}</Icon>
              </div>
            </TableCell>
            <TableCell>
              <div id="threeWeeksAgo" onClick={openFilter} className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusThreeWeeksAgo)}
                <Icon className={styles.filterIcon}>{filterIsActive('threeWeeksAgo') ? 'filter_alt' : 'expand_more'}</Icon>
              </div>
            </TableCell>
            <TableCell>
              <div id="fourWeeksAgo" onClick={openFilter} className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusFourWeeksAgo)}
                <Icon className={styles.filterIcon}>{filterIsActive('fourWeeksAgo') ? 'filter_alt' : 'expand_more'}</Icon>
              </div>
            </TableCell>
            <TableCell>
              <div id="fiveWeeksAgo" onClick={openFilter} className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusFiveWeeksAgo)}
                <Icon className={styles.filterIcon}>{filterIsActive('fiveWeeksAgo') ? 'filter_alt' : 'expand_more'}</Icon>
              </div>
            </TableCell>
            <TableCell>
              <div id="sixWeeksAgo" onClick={openFilter} className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusSixWeeksAgo)}
                <Icon className={styles.filterIcon}>{filterIsActive('sixWeeksAgo') ? 'filter_alt' : 'expand_more'}</Icon>
              </div>
            </TableCell>
            <TableCell>
              <div id="sevenWeeksAgo" onClick={openFilter} className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusSevenWeeksAgo)}
                <Icon className={styles.filterIcon}>{filterIsActive('sevenWeeksAgo') ? 'filter_alt' : 'expand_more'}</Icon>
              </div>
            </TableCell>
            <TableCell>
              <div id="eightWeeksAgo" onClick={openFilter} className={styles.filterHeader}>
                {strings(stringKeys.dataCollector.performanceList.statusEightWeeksAgo)}
                <Icon className={styles.filterIcon}>{filterIsActive('eightWeeksAgo') ? 'filter_alt' : 'expand_more'}</Icon>
              </div>
            </TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {!isListFetching && completeness != null && (
            <TableRow hover>
              <TableCell className={styles.completenessBorderBottomColor}>
                <span className={styles.completeness}>
                  {strings(stringKeys.dataCollector.performanceList.completenessTitle)}
                  <Tooltip title={strings(stringKeys.dataCollector.performanceList.completenessDescription)} onClick={handleTooltipClick} arrow>
                    <InfoIcon fontSize="small" className={styles.completenessTooltip} />
                  </Tooltip>
                </span>
              </TableCell>
              <TableCell className={styles.completenessAlignmentAndBorder}>-</TableCell>
              <TableCell className={styles.completenessAlignmentAndBorder}>-</TableCell>
              <TableCell className={styles.completenessAlignmentAndBorder}>
                <Tooltip title={renderTooltipText(completeness.lastWeek)} onClick={handleTooltipClick} arrow>
                  <span>{`${completeness.lastWeek.percentage} %`}</span>
                </Tooltip>
              </TableCell>
              <TableCell className={styles.completenessAlignmentAndBorder}>
                <Tooltip title={renderTooltipText(completeness.twoWeeksAgo)} onClick={handleTooltipClick} arrow>
                  <span>{`${completeness.twoWeeksAgo.percentage} %`}</span>
                </Tooltip>
              </TableCell>
              <TableCell className={styles.completenessAlignmentAndBorder}>
                <Tooltip title={renderTooltipText(completeness.threeWeeksAgo)} onClick={handleTooltipClick} arrow>
                  <span>{`${completeness.threeWeeksAgo.percentage} %`}</span>
                </Tooltip>
              </TableCell>
              <TableCell className={styles.completenessAlignmentAndBorder}>
                <Tooltip title={renderTooltipText(completeness.fourWeeksAgo)} onClick={handleTooltipClick} arrow>
                  <span>{`${completeness.fourWeeksAgo.percentage} %`}</span>
                </Tooltip>
              </TableCell>
              <TableCell className={styles.completenessAlignmentAndBorder}>
                <Tooltip title={renderTooltipText(completeness.fiveWeeksAgo)} onClick={handleTooltipClick} arrow>
                  <span>{`${completeness.fiveWeeksAgo.percentage} %`}</span>
                </Tooltip>
              </TableCell>
              <TableCell className={styles.completenessAlignmentAndBorder}>
                <Tooltip title={renderTooltipText(completeness.sixWeeksAgo)} onClick={handleTooltipClick} arrow>
                  <span>{`${completeness.sixWeeksAgo.percentage} %`}</span>
                </Tooltip>
              </TableCell>
              <TableCell className={styles.completenessAlignmentAndBorder}>
                <Tooltip title={renderTooltipText(completeness.sevenWeeksAgo)} onClick={handleTooltipClick} arrow>
                  <span>{`${completeness.sevenWeeksAgo.percentage} %`}</span>
                </Tooltip>
              </TableCell>
              <TableCell className={styles.completenessAlignmentAndBorder}>
                <Tooltip title={renderTooltipText(completeness.eightWeeksAgo)} onClick={handleTooltipClick} arrow>
                  <span>{`${completeness.eightWeeksAgo.percentage} %`}</span>
                </Tooltip>
              </TableCell>
            </TableRow>
          )}
          {!isListFetching && (
            list.map((row, index) => (
              <TableRow key={index} hover>
                <TableCell>
                  {row.name}
                  <br />
                  {row.phoneNumber}
                </TableCell>
                <TableCell className={styles.centeredText}>{row.villageName}</TableCell>
                <TableCell className={styles.centeredText}>{row.daysSinceLastReport > -1 ? row.daysSinceLastReport : '-'}</TableCell>
                <TableCell className={styles.centeredText}>
                  <DataCollectorStatusIcon status={row.statusLastWeek} icon={getIconFromStatus(row.statusLastWeek)} />
                </TableCell>
                <TableCell className={styles.centeredText}>
                  <DataCollectorStatusIcon status={row.statusTwoWeeksAgo} icon={getIconFromStatus(row.statusTwoWeeksAgo)} />
                </TableCell>
                <TableCell className={styles.centeredText}>
                  <DataCollectorStatusIcon status={row.statusThreeWeeksAgo} icon={getIconFromStatus(row.statusThreeWeeksAgo)} />
                </TableCell>
                <TableCell className={styles.centeredText}>
                  <DataCollectorStatusIcon status={row.statusFourWeeksAgo} icon={getIconFromStatus(row.statusFourWeeksAgo)} />
                </TableCell>
                <TableCell className={styles.centeredText}>
                  <DataCollectorStatusIcon status={row.statusFiveWeeksAgo} icon={getIconFromStatus(row.statusFiveWeeksAgo)} />
                </TableCell>
                <TableCell className={styles.centeredText}>
                  <DataCollectorStatusIcon status={row.statusSixWeeksAgo} icon={getIconFromStatus(row.statusSixWeeksAgo)} />
                </TableCell>
                <TableCell className={styles.centeredText}>
                  <DataCollectorStatusIcon status={row.statusSevenWeeksAgo} icon={getIconFromStatus(row.statusSevenWeeksAgo)} />
                </TableCell>
                <TableCell className={styles.centeredText}>
                  <DataCollectorStatusIcon status={row.statusEightWeeksAgo} icon={getIconFromStatus(row.statusEightWeeksAgo)} />
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
      {!!list.length && <TablePager totalRows={totalRows} rowsPerPage={rowsPerPage} page={page} onChangePage={onChangePage} />}

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
