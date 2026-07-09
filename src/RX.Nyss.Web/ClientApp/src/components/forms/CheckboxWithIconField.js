import styles from "./CheckboxWithIconField.module.scss";

import React, { Fragment } from "react";
import PropTypes from "prop-types";
import { Checkbox, FormControlLabel } from "@material-ui/core";
import { createFieldComponent } from "./FieldBase";

const CheckboxWithIcon = ({
  name,
  label,
  value,
  labelicon,
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
        label={
          <span className={styles.labelWrapper}>
            {labelicon}
            {label}
          </span>
        }
      />
    </Fragment>
  );
};

CheckboxWithIcon.propTypes = {
  label: PropTypes.string,
  controlProps: PropTypes.object,
  value: PropTypes.bool,
  labelicon: PropTypes.node,
  name: PropTypes.string,
};

export const CheckboxWithIconField = createFieldComponent(CheckboxWithIcon);
export default CheckboxWithIconField;
