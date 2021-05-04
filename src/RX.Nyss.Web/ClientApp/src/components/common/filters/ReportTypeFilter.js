import styles from './ReportFilters.module.scss';
import { Checkbox, FormControl, FormControlLabel, FormGroup, FormLabel } from '@material-ui/core';
import { Fragment } from 'react';
import { stringKeys, strings } from '../../../strings';

export const ReportTypeFilter = ({ filter, onChange, showTrainingFilter }) => {

  return (
    <Fragment>
      <FormControl className={styles.filterItem}>
        <FormLabel component='legend'>{strings(stringKeys.filters.report.reportType)}</FormLabel>
        <FormGroup className={styles.filterCheckboxGroup}>
          <FormControlLabel
            control={<Checkbox checked={filter.real} onChange={onChange} name='real' color='primary' />}
            label={strings(stringKeys.filters.report.nonTrainingReports)}
          />
          {showTrainingFilter && (
            <FormControlLabel
              control={<Checkbox checked={filter.training} onChange={onChange} name='training' color='primary' />}
              label={strings(stringKeys.filters.report.trainingReports)}
            />
          )}
          <FormControlLabel
            control={<Checkbox checked={filter.corrected} onChange={onChange} name='corrected' color='primary' />}
            label={strings(stringKeys.filters.report.correctedReports)}
          />
        </FormGroup>
      </FormControl>
    </Fragment>
  );
}