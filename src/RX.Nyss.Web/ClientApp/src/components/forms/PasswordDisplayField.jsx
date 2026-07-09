import React from "react";
import PropTypes from "prop-types";
import { IconButton, Typography } from "@material-ui/core";
import Visibility from "@material-ui/icons/Visibility";
import VisibilityOff from "@material-ui/icons/VisibilityOff";

export const PasswordDisplayField = ({ label, value }) => {
  const [showPassword, setShowPassword] = React.useState(false);

  const togglePassword = () => {
    setShowPassword(!showPassword);
  };

  const getAsterisk = (value) => {
    return Array(value.length).join("*");
  };

  return (
    <>
      <Typography variant="h6">{label}</Typography>
      <Typography variant="body1" gutterBottom>
        {showPassword ? value : getAsterisk(value)}
        <IconButton
          aria-label="toggle password visibility"
          onClick={togglePassword}
          onMouseDown={(event) => event.preventDefault()}
        >
          {showPassword ? <Visibility /> : <VisibilityOff />}
        </IconButton>
      </Typography>
    </>
  );
};

PasswordDisplayField.propTypes = {
  onClick: PropTypes.func,
  label: PropTypes.string,
};

export default PasswordDisplayField;
