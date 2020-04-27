import styles from "./TableRowAction.module.scss";
import React from "react";

export const TableRowActions = ({ children, ...rest }) => (
  <div className={styles.tableRowActions} {...rest}>
    {children}
  </div>
);
