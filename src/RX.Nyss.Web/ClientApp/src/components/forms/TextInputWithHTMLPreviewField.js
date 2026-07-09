import styles from "./TextInputWithHTMLPreviewField.module.scss";
import React from "react";
import PropTypes from "prop-types";
import { createFieldComponent } from "./FieldBase";
import { Grid, TextField } from "@material-ui/core";

const TextInputWithHTMLPreview = ({
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
    <Grid container spacing={2}>
      <Grid item xs={6}>
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
      </Grid>
      <Grid item xs={6}>
        <div
          dangerouslySetInnerHTML={{ __html: value }}
          className={styles.htmlPreview}
        />
      </Grid>
    </Grid>
  );
};

TextInputWithHTMLPreview.propTypes = {
  controlProps: PropTypes.object,
  name: PropTypes.string,
};

export const TextInputWithHTMLPreviewField = createFieldComponent(
  TextInputWithHTMLPreview,
);
export default TextInputWithHTMLPreviewField;
