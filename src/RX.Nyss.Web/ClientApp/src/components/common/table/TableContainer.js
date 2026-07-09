import styles from "./Table.module.scss";
import React from "react";
import { LinearProgress } from "@material-ui/core";

export const TableContainer = ({ sticky, children, isFetching }) => (
  <div
    className={`${styles.tableContainer} ${sticky ? styles.stickyTable : null}`}
  >
    {isFetching && <LinearProgress color="primary" />}
    {children}
  </div>
);
