import styles from "./RadioGroupField.module.scss";
import React from "react";
import PropTypes from "prop-types";
import { RadioGroup, FormHelperText, InputLabel } from "@material-ui/core";
import { createFieldComponent } from "./FieldBase";

const RadioGroupInput = ({
  error,
  name,
  label,
  boldLabel,
  value,
  horizontal,
  children,
  controlProps,
  customProps,
}) => {
  return (
    <div>
      {label && (
        <InputLabel
          component="legend"
          className={!!boldLabel ? styles.labelBold : styles.label}
          shrink
        >
          {label}
        </InputLabel>
      )}
      <RadioGroup
        aria-label={label}
        name={name}
        value={value}
        className={horizontal ? styles.horizontal : null}
        {...controlProps}
      >
        {children}
      </RadioGroup>
      {error && <FormHelperText error={!!error}>{error}</FormHelperText>}
    </div>
  );
};

RadioGroupInput.propTypes = {
  label: PropTypes.string,
  controlProps: PropTypes.object,
  value: PropTypes.string,
  horizontal: PropTypes.bool,
  name: PropTypes.string,
  error: PropTypes.string,
};

export const RadioGroupField = createFieldComponent(RadioGroupInput);
export default RadioGroupField;
