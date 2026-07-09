import React, { Fragment } from "react";
import PropTypes from "prop-types";
import { Checkbox, FormControlLabel, FormHelperText } from "@material-ui/core";
import { createFieldComponent } from "./FieldBase";

const CheckboxInput = ({
  error,
  name,
  label,
  value,
  controlProps,
  customProps,
}) => {
  return (
    <Fragment>
      <FormControlLabel
        control={
          <Checkbox checked={value} {...controlProps} {...customProps} />
        }
        name={name}
        label={label}
      />
      {error && <FormHelperText error>{error}</FormHelperText>}
    </Fragment>
  );
};

CheckboxInput.propTypes = {
  label: PropTypes.string,
  controlProps: PropTypes.object,
  value: PropTypes.bool,
  name: PropTypes.string,
  error: PropTypes.string,
};

export const CheckboxField = createFieldComponent(CheckboxInput);
export default CheckboxField;
