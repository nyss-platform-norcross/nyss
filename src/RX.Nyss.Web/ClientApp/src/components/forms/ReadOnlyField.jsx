import styles from './ReadOnlyField.module.scss';

import React from "react";
import PropTypes from "prop-types";
import TextField from '@material-ui/core/TextField';

const ReadOnlyField = ({ name, label, value, }) => {
  return (
    <TextField
      className={styles.readOnlyField}
      name={name}
      label={label}
      value={value}
      fullWidth
      InputLabelProps={{ shrink: true }}
      InputProps={{
        readOnly: true
      }}
    />
  );
};

ReadOnlyField.propTypes = {
  label: PropTypes.string,
  controlProps: PropTypes.object,
  value: PropTypes.string,
  name: PropTypes.string,
  error: PropTypes.string
};

export default ReadOnlyField;
