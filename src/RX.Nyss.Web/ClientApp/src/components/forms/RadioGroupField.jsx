import React from "react";
import PropTypes from "prop-types";
import FormHelperText from "@material-ui/core/FormHelperText";
import RadioGroup from "@material-ui/core/RadioGroup";
import { createFieldComponent } from "./FieldBase";

const RadioGroupInput = ({ error, name, value, children, controlProps, customProps }) => {
    return (
        <div>
            <RadioGroup
                aria-label={name}
                name={name}
                value={value}
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
    name: PropTypes.string,
    error: PropTypes.string
};

export const RadioGroupField = createFieldComponent(RadioGroupInput);
export default RadioGroupField;