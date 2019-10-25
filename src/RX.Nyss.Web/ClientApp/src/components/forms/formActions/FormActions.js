import styles from "./FormActions.module.scss";
import React from "react";

export const FormActions = ({ children }) => {
  return (
    <div className={styles.formActions}>
      {children}
    </div>
  );
};
