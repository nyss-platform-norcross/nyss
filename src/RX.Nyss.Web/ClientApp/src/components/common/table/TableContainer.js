import styles from "./Table.module.scss";
import React from 'react';

export const TableContainer = ({ sticky, children }) => (
  <div className={`${styles.tableContainer} ${sticky ? styles.stickyTable : null}`}>
    {children}
  </div>
);