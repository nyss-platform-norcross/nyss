import React from "react";
import PropTypes from "prop-types";
import { createFieldComponent } from "./FieldBase";
import { MultiSelect } from "./MultiSelect";

const MultiSelectFieldComponent = ({ error, name, label, value, controlProps, multiline, rows }) => {
  return (
    <MultiSelect
      name={name}
      error={!!error}
      helperText={error}
      label={label}
      value={value}
      InputLabelProps={{ shrink: true }}
      InputProps={{ ...controlProps }}
    />
  );
};

MultiSelectFieldComponent.propTypes = {
  label: PropTypes.string,
  controlProps: PropTypes.object,
  name: PropTypes.string,
  error: PropTypes.string
};

export const MultiSelectField = createFieldComponent(MultiSelectFieldComponent);
export default MultiSelectField;
