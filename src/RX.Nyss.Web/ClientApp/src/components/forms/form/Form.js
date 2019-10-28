import styles from "./Form.module.scss";
import React from "react";
import PropTypes from "prop-types";

export const Form = ({ onSubmit, children }) => {
  return (
    <form className={styles.form} onSubmit={onSubmit}>
      {children}
    </form>
  );
};

Form.propTypes = {
  onSubmit: PropTypes.func
};

export default Form;