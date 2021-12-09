import React from "react";
import MuiPhoneNumber from 'material-ui-phone-number'

const CountryCode = ({ field, defaultCountry, name, label, onChange }) => {
    const handlePhoneChange = ((input) => {
      field.update(input)
      console.log(field)
    })


  return (
    <MuiPhoneNumber 
      label={label}
      name={name}
      value={field.value}
      onChange={handlePhoneChange}
      fullWidth
      defaultCountry={!!defaultCountry ? defaultCountry.toLowerCase() : "ch"}
      autoFormat={false}
      inputMode={"tel"}
      />
  );
};

export default CountryCode;