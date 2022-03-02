import styles from './PhoneInputField.module.scss';
import React, { Fragment } from "react";
import PhoneInput from 'react-phone-input-2';
import 'react-phone-input-2/lib/style.css';
import { createFieldComponent } from "./FieldBase";
import { FormHelperText, InputLabel } from '@material-ui/core';
import { useEffect } from 'react';

const DEMO_NS_COUNTRY = "MI"; // demo National Society

const PhoneInputComponent = ({ error, value, defaultCountry, name, label, controlProps, fieldRef, rtl }) => {

  useEffect(() => {
    document.documentElement.style.setProperty('--selected-flag-padding', rtl ? '0 8px 0 0' : '0 0 0 8px');
    document.documentElement.style.setProperty('--flag-arrow-left', rtl ? '0' : '20px');
    document.documentElement.style.setProperty('--flag-arrow-right', rtl ? '20px' : '0');
    document.documentElement.style.setProperty('--phone-input-padding-left', rtl ? '10px' : '48px');
    document.documentElement.style.setProperty('--phone-input-padding-right', rtl ? '48px' : '0');
    document.documentElement.style.setProperty('--selected-flag-option-margin-right', rtl ? '0' : '7px');
    document.documentElement.style.setProperty('--selected-flag-option-margin-left', rtl ? '7px' : '0');

  }, [rtl]);

  return (
    <Fragment>
      <InputLabel className={styles.label}>{label}</InputLabel>
      <PhoneInput
        ref={fieldRef}
        error={!!error}
        name={name}
        containerClass={`${styles.container} ${error ? styles.error : null}`}
        inputClass={styles.phoneInput}
        buttonClass={styles.test}
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