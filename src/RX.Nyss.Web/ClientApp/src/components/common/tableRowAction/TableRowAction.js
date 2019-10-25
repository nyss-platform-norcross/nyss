import styles from "./TableRowAction.module.scss";
import React from "react";

export const TableRowAction = ({ icon, onClick, title }) => (
  <div className={`${styles.tableRowAction}`} title={title} onClick={onClick}>
    {icon}
  </div>
);
