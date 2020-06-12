import styles from "./TimePicker.module.scss"

import React from 'react';
import PropTypes from "prop-types";
import { KeyboardTimePicker } from '@material-ui/pickers';

const formatAMPM = (date) => {
  var hours = date.hour();
  var minutes = date.minute();
  var ampm = hours >= 12 ? 'PM' : 'AM';
  hours = hours % 12;
  hours = hours ? hours : 12; // the hour '0' should be '12'
  minutes = minutes < 10 ? '0'+minutes : minutes;
  var strTime = hours + ':' + minutes + ' ' + ampm;
  return strTime;
}

export const TimePicker = ({ label, value, onChange, className, fullWidth }) => (
  <KeyboardTimePicker
    className={`${className} ${fullWidth ? '' : styles.timePicker}`}
    autoOk
    labelFunc={formatAMPM}
    label={label}
    value={value}
    onChange={onChange}
    KeyboardButtonProps={{
      'aria-label': 'change time',
    }}
    InputProps={{ readOnly: true }}
    InputLabelProps={{ shrink: true }}
  />
);

TimePicker.propTypes = {
  label: PropTypes.string,
  value: PropTypes.any,
  onChange: PropTypes.func,
  className: PropTypes.string,
  fullWidth: PropTypes.bool
}