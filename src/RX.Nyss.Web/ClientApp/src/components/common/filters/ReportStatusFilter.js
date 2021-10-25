import styles from './ReportFilters.module.scss';
import { Checkbox, FormControl, FormControlLabel, FormGroup, FormLabel } from '@material-ui/core';
import { Fragment } from 'react';
import { stringKeys, strings } from '../../../strings';

export const ReportStatusFilter = ({ filter, onChange, correctReports, showDismissedFilter, doNotWrap }) => {

  return (
    <Fragment>
      <FormControl className={styles.filterItem}>
        <FormLabel component='legend'>{strings(stringKeys.filters.report.status)}</FormLabel>
        <FormGroup className={doNotWrap ? `${styles.filterCheckboxGroup} ${styles.noFlexWrap}` : styles.filterCheckboxGroup}>
          {correctReports && (
            <Fragment>
              <FormControlLabel
                className={styles.checkbox}
                control={<Checkbox checked={filter.kept} onChange={onChange} name='kept' color='primary' />}
                label={strings(stringKeys.filters.report.kept)}
              />
              <FormControlLabel
                className={styles.checkbox}
                control={<Checkbox checked={filter.notCrossChecked} onChange={onChange} name='notCrossChecked' color='primary' />}
                label={strings(stringKeys.filters.report.notCrossChecked)}
              />
              {showDismissedFilter && (
                <FormControlLabel
                  className={styles.checkbox}
                  control={<Checkbox checked={filter.dismissed} onChange={onChange} name='dismissed' color='primary' />}
                  label={strings(stringKeys.filters.report.dismissed)}
                />
              )}
            </Fragment>
          )}
          {!correctReports && (
            <Fragment>
              <FormControlLabel
                className={styles.checkbox}
                control={<Checkbox checked={filter.real} onChange={onChange} name='real' color='primary' />}
                label={strings(stringKeys.filters.report.nonTrainingReports)}
              />
              <FormControlLabel
                className={styles.checkbox}
                control={<Checkbox checked={filter.corrected} onChange={onChange} name='corrected' color='primary' />}
                label={strings(stringKeys.filters.report.correctedReports)}
              />
            </Fragment>
          )}
        </FormGroup>
      </FormControl>
    </Fragment>
  );
}
