import React from "react";
import PropTypes from "prop-types";
import { createFieldComponent } from "./FieldBase";
import { TextField, IconButton, InputAdornment } from "@material-ui/core";

const TextActionInput = ({
  error,
  name,
  label,
  value,
  controlProps,
  customProps,
  icon,
  onButtonClick,
}) => {
  const button = (
    <InputAdornment position="end">
      <IconButton
        onClick={onButtonClick}
        onMouseDown={(event) => event.preventDefault()}
      >
        {icon}
      </IconButton>
    </InputAdornment>
  );

  return (
    <TextField
      name={name}
      error={!!error}
      helperText={error}
      label={label}
      value={value}
      fullWidth
      InputLabelProps={{ shrink: true }}
      InputProps={{
        ...controlProps,
        endAdornment: button,
      }}
    />
  );
};

TextActionInput.propTypes = {
  label: PropTypes.string,
  controlProps: PropTypes.object,
  value: PropTypes.string,
  name: PropTypes.string,
  error: PropTypes.string,
};

export const TextActionInputField = createFieldComponent(TextActionInput);
export default TextActionInputField;
