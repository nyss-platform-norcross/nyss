import styles from './CheckboxWithIconField.module.scss';

import React, { Fragment } from "react";
import PropTypes from "prop-types";
import FormControlLabel from "@material-ui/core/FormControlLabel";
import FormHelperText from "@material-ui/core/FormHelperText";
import Checkbox from "@material-ui/core/Checkbox";
import { createFieldComponent } from "./FieldBase";
import { withStyles } from "@material-ui/core";
import { DataCollectorStatusIcon } from "../common/icon/DataCollectorStatusIcon";
import { getIconFromStatus } from "../dataCollectors/logic/dataCollectorsService";
import { performanceStatus } from "../dataCollectors/logic/dataCollectorsConstants";

const FormControlLabelWithIcon = withStyles({
  label: {
    '&::before': {
      content: 'checked',
      fontFamily: 'Material Icons'
    }
  }
})((props) => <FormControlLabel {...props} />);

const CheckboxWithIconInput = ({ error, name, label, value, icon, onChange, controlProps, customProps }) => {
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

CheckboxWithIconInput.propTypes = {
    label: PropTypes.string,
    controlProps: PropTypes.object,
    value: PropTypes.bool,
    icon: PropTypes.string,
    name: PropTypes.string,
    error: PropTypes.string,
    onChange: PropTypes.func
};

export const CheckboxWithIconField = CheckboxWithIconInput;
export default CheckboxWithIconField;