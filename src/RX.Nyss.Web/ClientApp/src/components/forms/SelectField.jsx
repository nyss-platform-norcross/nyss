import React from "react";
import PropTypes from "prop-types";
import FormControl from "@material-ui/core/FormControl";
import FormHelperText from "@material-ui/core/FormHelperText";
import InputLabel from "@material-ui/core/InputLabel";
import Select from "@material-ui/core/Select";
import { createFieldComponent } from "./FieldBase";

const SelectInput = ({ error, name, label, value, onChange, controlProps, customProps, children }) => {
  return (
    <FormControl error={!!error} {...customProps} fullWidth>
      <InputLabel htmlFor={name} shrink>{label}</InputLabel>
      <Select
        value={value}
        {...controlProps}
        inputProps={{
          name: name,
          id: name
        }}
        onChange={e => {
          onChange && onChange(e);
          controlProps.onChange(e);
        }}
      >
        {children}
      </Select>
      {error && <FormHelperText>{error}</FormHelperText>}
    </FormControl>
  );
};

SelectInput.propTypes = {
  controlProps: PropTypes.object,
  value: PropTypes.string,
  name: PropTypes.string
};

export const SelectField = createFieldComponent(SelectInput);
export default SelectField;