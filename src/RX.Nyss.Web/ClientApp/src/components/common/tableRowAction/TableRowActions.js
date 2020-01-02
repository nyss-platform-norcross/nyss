import styles from "./TableRowAction.module.scss";
import React from "react";

export const TableRowActions = ({ children }) => (
  <div className={styles.tableRowActions}>
    {children}
  </div>
);
