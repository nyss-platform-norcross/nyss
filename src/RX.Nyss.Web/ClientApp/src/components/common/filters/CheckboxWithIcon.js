import styles from './CheckboxWithIcon.module.scss';

import React, { Fragment } from "react";
import PropTypes from "prop-types";
import FormControlLabel from "@material-ui/core/FormControlLabel";
import Checkbox from "@material-ui/core/Checkbox";

const CheckboxWithIcon = ({ name, label, value, icon, onChange, controlProps, customProps }) => {
  return (
    <Fragment>
      <FormControlLabel
        control={
          <Checkbox checked={value} onChange={onChange} {...controlProps} {...customProps} />
        }
        name={name}
        label={
          <div className={styles.labelWrapper}>
            {icon}
            {label}
          </div>
        }
      />
    </Fragment>
  );
};

CheckboxWithIcon.propTypes = {
  label: PropTypes.string,
  controlProps: PropTypes.object,
  value: PropTypes.bool,
  icon: PropTypes.object,
  name: PropTypes.string,
  onChange: PropTypes.func
};

export default CheckboxWithIcon;