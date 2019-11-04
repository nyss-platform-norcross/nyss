import React from "react";
import PropTypes from "prop-types";
import { createFieldComponent } from "./FieldBase";
import TextField from '@material-ui/core/TextField';

const TextInput = ({ error, name, label, value, controlProps, customProps, multiline, rows, autoWidth, autoFocus }) => {
  return (
    <TextField
      name={name}
      error={!!error}
      helperText={error}
      label={label}
      value={value}
      multiline={multiline}
      rows={rows}
      fullWidth={autoWidth ? false : true}
      InputLabelProps={{ shrink: true }}
      InputProps={{ ...controlProps }}
      inputProps={{ autoFocus: autoFocus }}
    />
  );
};

TextInput.propTypes = {
  label: PropTypes.string,
  controlProps: PropTypes.object,
  name: PropTypes.string,
  error: PropTypes.string
};

export const TextInputField = createFieldComponent(TextInput);
export default TextInputField;
