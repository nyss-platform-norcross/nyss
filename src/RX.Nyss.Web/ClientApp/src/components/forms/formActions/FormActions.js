import styles from "./FormActions.module.scss";
import React from "react";

export const FormActions = ({ children, className }) => {
  return <div className={`${styles.formActions} ${className}`}>{children}</div>;
};

export default FormActions;
