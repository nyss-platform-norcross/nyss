import styles from './TextInputWithHTMLPreviewField.module.scss';
import React, { Fragment } from "react";
import PropTypes from "prop-types";
import { createFieldComponent } from "./FieldBase";
import TextField from '@material-ui/core/TextField';

const TextInputWithHTMLPreview = ({ error, name, label, value, controlProps, multiline, rows, autoWidth, autoFocus, disabled, type }) => {
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
      <div dangerouslySetInnerHTML={{ __html: value }} className={styles.htmlPreview} />
    </Fragment>
  );
};

TextInputWithHTMLPreview.propTypes = {
  controlProps: PropTypes.object,
  name: PropTypes.string
};

export const TextInputWithHTMLPreviewField = createFieldComponent(TextInputWithHTMLPreview);
export default TextInputWithHTMLPreviewField;
