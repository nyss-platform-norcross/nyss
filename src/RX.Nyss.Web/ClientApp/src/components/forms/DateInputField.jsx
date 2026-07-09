import React from "react";
import PropTypes from "prop-types";
import { createFieldComponent } from "./FieldBase";
import { DatePicker } from "./DatePicker";

const DateInput = ({ label, name, className, value, controlProps }) => {
  return (
    <DatePicker
      label={label}
      name={name}
      className={className}
      value={value}
      {...controlProps}
    />
  );
};

DateInput.propTypes = {
  label: PropTypes.string,
  name: PropTypes.string,
  className: PropTypes.string,
  value: PropTypes.object,
  controlProps: PropTypes.object,
};

export const DateInputField = createFieldComponent(DateInput);
export default DateInputField;
