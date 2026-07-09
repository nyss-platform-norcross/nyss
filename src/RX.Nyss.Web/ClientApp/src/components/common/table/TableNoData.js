import styles from "./Table.module.scss";
import React from "react";
import { strings, stringKeys } from "../../../strings";

export const TableNoData = ({ message }) => (
  <div className={styles.noDataInfo}>
    {message || strings(stringKeys.table.noData)}
  </div>
);
