import styles from "./Table.module.scss";
import React from 'react';
import { Loading } from "../loading/Loading";

export const TableContainer = ({ sticky, children, isFetching }) => (
  <div className={`${styles.tableContainer} ${sticky ? styles.stickyTable : null}`}>
    {isFetching && <Loading absolute top />}
    {children}
  </div>
);