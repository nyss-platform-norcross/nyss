import styles from "./TableRowAction.module.scss";
import React from "react";
import CircularProgress from "@material-ui/core/CircularProgress";

export const TableRowAction = ({ icon, onClick, title, isFetching }) => (
  <div className={`${styles.tableRowAction} ${(isFetching ? styles.fetching : "")}`} title={title} onClick={onClick}>
    {isFetching && <CircularProgress size={20} className={styles.loader} />}
    <div className={styles.icon}>
      {icon}
    </div>
  </div>
);
