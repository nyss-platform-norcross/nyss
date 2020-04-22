import React, { Fragment } from "react";
import PropTypes from "prop-types";
import FormControlLabel from "@material-ui/core/FormControlLabel";
import FormHelperText from "@material-ui/core/FormHelperText";
import Checkbox from "@material-ui/core/Checkbox";
import { withStyles } from "@material-ui/core";

const FormControlLabelWithIcon = withStyles({
  label: {
    '&:before': {
      content: 'checked',
      fontFamily: 'Material Icons'
    }
  }
})((props) => <FormControlLabel {...props} />);

const CheckboxWithIconField = ({ error, name, label, value, icon, onChange, controlProps, customProps }) => {
  return (
    <Fragment>
      <FormControlLabelWithIcon
        control={
          <Checkbox checked={value} onChange={onChange} {...controlProps} {...customProps} />
        }
        name={name}
        label={label}
      />
      {error && <FormHelperText>{error}</FormHelperText>}
    </Fragment>
  );
};

CheckboxWithIconField.propTypes = {
  label: PropTypes.string,
  controlProps: PropTypes.object,
  value: PropTypes.bool,
  icon: PropTypes.string,
  name: PropTypes.string,
  error: PropTypes.string,
  onChange: PropTypes.func
};

export default CheckboxWithIconField;