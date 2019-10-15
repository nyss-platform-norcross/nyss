import React from "react";
import PropTypes from "prop-types";
import FormControl from "@material-ui/core/FormControl";
import FormHelperText from "@material-ui/core/FormHelperText";
import InputLabel from "@material-ui/core/InputLabel";
import Select from "@material-ui/core/Select";
import { createFieldComponent } from "./FieldBase";

const SelectInput = ({ error, name, label, value, controlProps, customProps, children }) => {
    return (
        <FormControl error={!!error} {...customProps}>
            <InputLabel htmlFor={name}>{label}</InputLabel>
            <Select
                value={value}
                {...controlProps}
                inputProps={{
                    name: name,
                    id: name
                }}
            >
                {children}
            </Select>
            {error && <FormHelperText>{error}</FormHelperText>}
        </FormControl>
    );
};

SelectInput.propTypes = {
    label: PropTypes.string,
    controlProps: PropTypes.object,
    value: PropTypes.string,
    name: PropTypes.string,
    error: PropTypes.string
};

export const SelectField = createFieldComponent(SelectInput);
export default SelectField;