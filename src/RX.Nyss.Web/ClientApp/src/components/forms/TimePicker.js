import React from "react";
import PropTypes from "prop-types";
import { TextField } from "@material-ui/core";

const TimePicker = ({ name, label, value, error, onChange }) => {
  const handleChange = (e) => {
    onChange(e.target.value);
  };

  return (
    <TextField
      name={name}
      error={!!error}
      helperText={error}
      label={label}
      value={value}
      fullWidth
      InputLabelProps={{ shrink: true }}
      type="time"
      pattern="[0-9]{2}:[0-9]{2}"
      onChange={handleChange}
    />
  );
};

TimePicker.propTypes = {
  name: PropTypes.string,
  label: PropTypes.string,
  value: PropTypes.string,
  error: PropTypes.string,
  onChange: PropTypes.func,
};

export default TimePicker;
