import styles from "./TableRowAction.module.scss";
import React from "react";
import CircularProgress from "@material-ui/core/CircularProgress";
import ConfirmationAction from "../confirmationAction/ConfirmationAction";

export const TableRowAction = ({ icon, onClick, title, isFetching, confirmationText }) => {
  const handleClick = (e) => {
    e.stopPropagation();
    onClick();
  };

  if (confirmationText) {
    return (
      <div className={`${styles.tableRowAction} ${(isFetching ? styles.fetching : "")}`} title={title}>
        <ConfirmationAction confirmationText={confirmationText} onClick={onClick} icon={icon}>
          {isFetching && <CircularProgress size={20} className={styles.loader} />}
          <div className={styles.icon}>
            {icon}
          </div>
        </ConfirmationAction>
      </div>
      );
  }

  return (
    <div className={`${styles.tableRowAction} ${(isFetching ? styles.fetching : "")}`} title={title} onClick={handleClick}>
      {isFetching && <CircularProgress size={20} className={styles.loader} />}
      <div className={styles.icon}>
        {icon}
      </div>
    </div>
  );
}
