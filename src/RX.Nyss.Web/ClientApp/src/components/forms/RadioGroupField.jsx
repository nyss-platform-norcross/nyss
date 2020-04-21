import styles from './RadioGroupField.module.scss';
import React from "react";
import PropTypes from "prop-types";
import FormHelperText from "@material-ui/core/FormHelperText";
import RadioGroup from "@material-ui/core/RadioGroup";
import { createFieldComponent } from "./FieldBase";
import InputLabel from '@material-ui/core/InputLabel';

const RadioGroupInput = ({ error, name, label, value, horizontal, children, controlProps, customProps }) => {
  return (
    <div>
      {label && <InputLabel component="legend" className={styles.label} shrink>{label}</InputLabel>}
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
  error: PropTypes.string
};

export const RadioGroupField = createFieldComponent(RadioGroupInput);
export default RadioGroupField;
