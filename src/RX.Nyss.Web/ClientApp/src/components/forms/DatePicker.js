import React from 'react';
import { KeyboardDatePicker } from '@material-ui/pickers';

export const DatePicker = ({ label, value, onChange, className }) => (
  <KeyboardDatePicker
    className={className}
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