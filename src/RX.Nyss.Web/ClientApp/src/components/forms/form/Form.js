import styles from "./Form.module.scss";
import React from "react";
import PropTypes from "prop-types";

export const Form = ({ onSubmit, children, fullWidth, className, style }) => {
  return (
    <form
      className={`${styles.form} ${
        fullWidth ? null : styles.shrinked
      } ${className}`}
      style={style}
      onSubmit={onSubmit}
    >
      {children}
    </form>
  );
};

Form.propTypes = {
  onSubmit: PropTypes.func,
};

export default Form;
