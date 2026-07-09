import React from "react";
import PropTypes from "prop-types";
import {
  Select,
  FormControl,
  FormHelperText,
  InputLabel,
} from "@material-ui/core";
import { createFieldComponent } from "./FieldBase";

const SelectInput = ({
  error,
  disabled,
  disabledlabel,
  name,
  label,
  value,
  onChange,
  controlProps,
  customProps,
  children,
}) => {
  const { fieldRef, ...rest } = customProps;

  return (
    <FormControl
      error={!!error}
      disabled={disabled}
      {...rest}
      fullWidth
      ref={fieldRef}
    >
      <InputLabel htmlFor={name} shrink>
        {label}
      </InputLabel>
      <Select
        value={value}
        {...controlProps}
        inputProps={{
          name: name,
          id: name,
        }}
        onChange={(e) => {
          onChange && onChange(e);
          controlProps.onChange(e);
        }}
      >
        {children}
      </Select>
      {error && <FormHelperText>{error}</FormHelperText>}
      {disabled && <FormHelperText>{disabledlabel}</FormHelperText>}
    </FormControl>
  );
};

SelectInput.propTypes = {
  controlProps: PropTypes.object,
  value: PropTypes.string,
  name: PropTypes.string,
};

export const SelectField = createFieldComponent(SelectInput);
export default SelectField;
