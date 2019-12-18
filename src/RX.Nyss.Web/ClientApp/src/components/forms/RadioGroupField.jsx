import styles from './RadioGroupField.module.scss';
import React from "react";
import PropTypes from "prop-types";
import FormHelperText from "@material-ui/core/FormHelperText";
import RadioGroup from "@material-ui/core/RadioGroup";
import { createFieldComponent } from "./FieldBase";
import { FormLabel } from '@material-ui/core';

const RadioGroupInput = ({ error, name, value, horizontal, children, controlProps, customProps }) => {
    return (
        <div>
          <FormLabel component="legend">{name}</FormLabel>
          <RadioGroup
              aria-label={name}
              name={name}
              value={value}
              className={horizontal ? styles.horizontal : null}
              {...controlProps}
              {...customProps}
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
