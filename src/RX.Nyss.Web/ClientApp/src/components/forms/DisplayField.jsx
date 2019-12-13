import styles from './DisplayField.module.scss';
import React from "react";

export const DisplayField = ({ label, value, }) => {
  return (
    <div className={styles.displayField}>
      <div className={styles.label}>{label}</div>
      <div className={styles.value}>{value}</div>
    </div>
  );
};

export default DisplayField;
