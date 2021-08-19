import styles from './DataCollectorsPerformanceTable.module.scss';

import React, { useState, useEffect } from 'react';
import PropTypes from "prop-types";
import { strings, stringKeys } from '../../strings';
import { TableContainer } from '../common/table/TableContainer';
import { getIconFromStatus } from './logic/dataCollectorsService';
import { DataCollectorStatusIcon } from '../common/icon/DataCollectorStatusIcon';
import { DataCollectorsPerformanceColumnFilters } from './DataCollectorsPerformanceColumnFilters';
import TablePager from '../common/tablePagination/TablePager';
import { Tooltip, Table, TableBody, TableCell, TableHead, TableRow, Icon } from '@material-ui/core';
import InfoIcon from '@material-ui/icons/InfoOutlined';

export const DataCollectorsPerformanceTable = ({ list, completeness, page, rowsPerPage, totalRows, isListFetching, filters, onChange }) => {
  const [isOpen, setIsOpen] = useState(false);
  const [anchorEl, setAnchorEl] = useState(null);
  const [selectedWeek, setSelectedWeek] = useState(null);
  const [statusFilters, setStatusFilters] = useState(null);
  const [epiWeeks, setEpiWeeks] = useState(null);

  useEffect(() => {
    !!filters && setEpiWeeks(filters.epiWeekFilters.map(filter => filter.epiWeek).reverse());
  }, [filters]);

  const openFilter = (event) => {
    setAnchorEl(event.currentTarget);
    setSelectedWeek(parseInt(event.currentTarget.id));
    setStatusFilters(getStatusFilter(parseInt(event.currentTarget.id)));
    setIsOpen(true);
  }

  const getStatusFilter = (week) => {
    return Object.assign({}, filters.epiWeekFilters.find(epiWeekFilter => epiWeekFilter.epiWeek === week));
  }

  const filterIsActive = (week) => {
    const filter = filters.epiWeekFilters.find(epiWeekFilter => epiWeekFilter.epiWeek === week);
    return !!filter && (!filter.reportingCorrectly || !filter.reportingWithErrors || !filter.notReporting);
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

  return !!filters && !!epiWeeks && (
    <TableContainer sticky isFetching={isListFetching}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.name)}</TableCell>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.villageName)}</TableCell>
            <TableCell>{strings(stringKeys.dataCollector.performanceList.daysSinceLastReport)}</TableCell>
            {epiWeeks.map(week => (
              <TableCell key={`filter_week_${week}`}>
                <div id={week} onClick={openFilter} className={styles.filterHeader}>
                  {`${strings(stringKeys.dataCollector.performanceList.epiWeek)} ${week}`}
                  <Icon className={styles.filterIcon}>{filterIsActive(week) ? 'filter_alt' : 'expand_more'}</Icon>
                </div>
              </TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {!isListFetching && completeness != null && filters.trainingStatus === 'Trained' && (
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

              {completeness.map(week => (
                <TableCell className={styles.completenessAlignmentAndBorder} key={`completeness_${week.epiWeek}`}>
                  <Tooltip title={renderTooltipText(week)} onClick={handleTooltipClick} arrow>
                    <span>{`${week.percentage} %`}</span>
                  </Tooltip>
                </TableCell>
              ))}

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
                {row.performanceInEpiWeeks.map(week => (
                  <TableCell className={styles.centeredText} key={`dc_performance_${week.epiWeek}`}>
                    <DataCollectorStatusIcon status={week.reportingStatus} icon={getIconFromStatus(week.reportingStatus)} />
                  </TableCell>
                ))}
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
