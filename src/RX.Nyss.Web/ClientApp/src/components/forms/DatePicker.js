import styles from "./DatePicker.module.scss"

import React from 'react';
import PropTypes from "prop-types";
import { KeyboardDatePicker } from '@material-ui/pickers';

export const DatePicker = ({ label, value, onChange, className, fullWidth }) => (
  <KeyboardDatePicker
    className={`${className} ${fullWidth ? '' : styles.datePicker}`}
    autoOk
    disableFuture
    disableToolbar
    variant="inline"
    format="YYYY-MM-DD"
    onChange={onChange}
    label={label}
    value={value}
    InputProps={{ readOnly: true }}
    InputLabelProps={{ shrink: true }}
  />
);

DatePicker.propTypes = {
  label: PropTypes.string,
  value: PropTypes.any,
  onChange: PropTypes.func,
  className: PropTypes.string,
  fullWidth: PropTypes.bool
}