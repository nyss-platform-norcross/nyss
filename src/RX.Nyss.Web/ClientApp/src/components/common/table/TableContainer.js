import styles from "./Table.module.scss";
import React from 'react';

export const TableContainer = ({ children }) => (
  <div className={styles.tableContainer}>
    {children}
  </div>
);