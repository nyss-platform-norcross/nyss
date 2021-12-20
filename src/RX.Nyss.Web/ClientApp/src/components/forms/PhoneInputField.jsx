import styles from './PhoneInputField.module.scss';
import React, { Fragment } from "react";
import PhoneInput from 'react-phone-input-2';
import 'react-phone-input-2/lib/style.css';
import { createFieldComponent } from "./FieldBase";
import { FormHelperText, InputLabel } from '@material-ui/core';

const DEMO_NS_COUNTRY = "MI"; // demo National Society

const PhoneInputComponent = ({ error, value, defaultCountry, name, label, controlProps }) => {

  return (
    <Fragment>
      <InputLabel className={styles.label}>{label}</InputLabel>
      <PhoneInput
        error={!!error}
        containerClass={`${styles.container} ${error ? styles.error : null}`}
        inputClass={styles.phoneInput}
        buttonClass={styles.button}
        searchClass={styles.search}
        specialLabel={label}
        country={!!defaultCountry && defaultCountry !== DEMO_NS_COUNTRY ? defaultCountry.toLowerCase() : "ch"}
        value={value}
        fullWidth
        enableSearch
        inputProps={{
          inputMode: 'tel'
        }}
        {...controlProps}
      />
      {error && <FormHelperText error>{error}</FormHelperText>}
    </Fragment>
  );
};

const PhoneInputField = createFieldComponent(PhoneInputComponent);
export default PhoneInputField;