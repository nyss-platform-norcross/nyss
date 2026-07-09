import styles from "./TableActions.module.scss";
import React from "react";

export const TableActions = ({ children }) => (
  <div className={`${styles.tableActions}`}>{children}</div>
);

export default TableActions;
