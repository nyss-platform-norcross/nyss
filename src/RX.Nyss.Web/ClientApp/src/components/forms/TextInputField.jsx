import React from "react";
import PropTypes from "prop-types";
import { createFieldComponent } from "./FieldBase";
import TextField from '@material-ui/core/TextField';

const TextInput = ({ error, name, label, value, controlProps, customProps }) => {
    return (
        <TextField
            name={name}
            error={!!error}
            helperText={error}
            label={label}
            value={value}
            fullWidth
            InputLabelProps={{ shrink: true }}
            InputProps={{ ...controlProps }}
            inputProps={{ ...customProps }}
        />
    );
};

TextInput.propTypes = {
    label: PropTypes.string,
    controlProps: PropTypes.object,
    value: PropTypes.string,
    name: PropTypes.string,
    error: PropTypes.string
};

export const TextInputField = createFieldComponent(TextInput);
export default TextInputField;
