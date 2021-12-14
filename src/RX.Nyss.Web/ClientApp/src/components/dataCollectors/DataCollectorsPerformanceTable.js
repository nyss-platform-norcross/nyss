import styles from './DataCollectorsPerformanceTable.module.scss';
import PropTypes from "prop-types";
import { strings, stringKeys } from '../../strings';
import { TableContainer } from '../common/table/TableContainer';
import { getIconFromStatus } from './logic/dataCollectorsService';
import { DataCollectorStatusIcon } from '../common/icon/DataCollectorStatusIcon';
import TablePager from '../common/tablePagination/TablePager';
import { Tooltip, Table, TableBody, TableCell, TableHead, TableRow } from '@material-ui/core';
import InfoIcon from '@material-ui/icons/InfoOutlined';
import { Loading } from '../common/loading/Loading';

export const DataCollectorsPerformanceTable = ({ list, completeness, epiDateRange, page, rowsPerPage, totalRows, isListFetching, filters, onChange }) => {
  const onChangePage = (e, page) => {
    onChange({ type: 'changePage', pageNumber: page });
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

  const roundToFixed = (value, decimals) => {
    const multiplier = Math.pow(10, decimals);
    return Math.round(value * multiplier) / multiplier;
  }

  const renderPercentage = (active, total) => {
    var percentage = active * 100 / total;
    return roundToFixed(percentage, 1);
  }

  if (!filters || !epiDateRange) {
    return <Loading />
  }

  return (
    <TableContainer sticky isFetching={isListFetching}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell className={styles.nameColumn}>{strings(stringKeys.dataCollector.performanceList.name)}</TableCell>
            <TableCell className={`${styles.villageColumn} ${styles.centeredText}`}>{strings(stringKeys.dataCollector.performanceList.villageName)}</TableCell>
            <TableCell className={`${styles.daysColumn} ${styles.centeredText}`}>{strings(stringKeys.dataCollector.performanceList.daysSinceLastReport)}</TableCell>

            {epiDateRange.map(week => (
              <TableCell key={`filter_week_${week.epiWeek}`} className={styles.weekColumn}>
                <div className={styles.filterHeader}>
                  {`${strings(stringKeys.dataCollector.performanceList.epiWeek)} ${week.epiWeek}`}
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
                    <span>{`${renderPercentage(week.activeDataCollectors, week.totalDataCollectors)} %`}</span>
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
