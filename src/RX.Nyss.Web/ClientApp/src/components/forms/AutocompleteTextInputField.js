import React from "react";
import PropTypes from "prop-types";
import { createFieldComponent } from "./FieldBase";
import TextField from '@material-ui/core/TextField';
import Autocomplete from '@material-ui/lab/Autocomplete';

const AutocompleteTextInput = ({ error, name, label, value, options, freeSolo, controlProps, autoWidth, disabled, type }) => {
  return (
    <Autocomplete
      name={name}
      value={value}
      freeSolo={freeSolo}
      disabled={disabled}
      options={options}
      {...controlProps}
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          error={!!error}
          helperText={error}
          fullWidth={autoWidth ? false : true}
          InputLabelProps={{ shrink: true }}
          type={type}
        />
      )}
    />
  );
};

AutocompleteTextInput.propTypes = {
  controlProps: PropTypes.object,
  name: PropTypes.string
};

export const AutocompleteTextInputField = createFieldComponent(AutocompleteTextInput);
export default AutocompleteTextInputField;
