import React from "react";
import MuiPhoneNumber from 'material-ui-phone-number'
import { createFieldComponent } from "./FieldBase";

const DEMO_NS_COUNTRY = "MI"; // demo National Society

const PhoneInput = ({ error, value, defaultCountry, name, label, controlProps }) => {

  return (
    <MuiPhoneNumber 
      label={label}
      error={!!error}
      helperText={error}
      name={name}
      value={value}
      fullWidth
      defaultCountry={!!defaultCountry && defaultCountry !== DEMO_NS_COUNTRY ? defaultCountry.toLowerCase() : "ch"}
      autoFormat={false}
      inputMode={"tel"}
      {...controlProps}
      />
  );
};

const PhoneInputField = createFieldComponent(PhoneInput);
export default PhoneInputField;