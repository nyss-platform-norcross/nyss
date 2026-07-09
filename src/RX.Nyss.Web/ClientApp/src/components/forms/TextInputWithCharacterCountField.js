import React, { Fragment } from "react";
import PropTypes from "prop-types";
import { createFieldComponent } from "./FieldBase";
import { Typography, Grid, TextField } from "@material-ui/core";
import { strings, stringKeys } from "../../strings";

const TextInputWithCharacterCount = ({
  error,
  name,
  label,
  value,
  controlProps,
  multiline,
  rows,
  autoWidth,
  autoFocus,
  disabled,
  type,
}) => {
  return (
    <Fragment>
      <TextField
        name={name}
        error={!!error}
        helperText={error}
        label={label}
        value={value}
        multiline={multiline}
        rows={rows}
        disabled={disabled}
        fullWidth={autoWidth ? false : true}
        InputLabelProps={{ shrink: true }}
        InputProps={{ ...controlProps }}
        inputProps={{ autoFocus: autoFocus }}
        type={type}
      />
      <Grid container>
        <Grid item xs={6}>
          <Typography variant="subtitle2">{`${strings(
            stringKeys.translations.smsCharacters,
          )} ${value.length}`}</Typography>
        </Grid>
        <Grid item xs={6}>
          <Typography variant="subtitle2">{`${strings(
            stringKeys.translations.smsParts,
          )} ${parseInt(value.length / 160) + 1}`}</Typography>
        </Grid>
      </Grid>
    </Fragment>
  );
};

TextInputWithCharacterCount.propTypes = {
  controlProps: PropTypes.object,
  name: PropTypes.string,
};

export const TextInputWithCharacterCountField = createFieldComponent(
  TextInputWithCharacterCount,
);
export default TextInputWithCharacterCountField;
