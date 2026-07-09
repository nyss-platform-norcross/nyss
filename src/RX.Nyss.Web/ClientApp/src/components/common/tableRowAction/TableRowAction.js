import styles from "./TableRowAction.module.scss";
import React from "react";
import { CircularProgress } from "@material-ui/core";
import ConfirmationAction from "../confirmationAction/ConfirmationAction";
import { withAccessRestriction } from "../hasAccess/HasAccess";

const TableRowActionComponent = ({
  icon,
  onClick,
  title,
  isFetching,
  confirmationText,
  directionRtl,
}) => {
  const handleClick = (e) => {
    e.stopPropagation();
    onClick();
  };

  if (confirmationText) {
    return (
      <div
        className={`${styles.tableRowAction} ${
          isFetching ? styles.fetching : ""
        } ${directionRtl ? styles.rtl : ""}`}
        title={title}
      >
        <ConfirmationAction
          confirmationText={confirmationText}
          onClick={onClick}
        >
          {isFetching && (
            <CircularProgress size={20} className={styles.loader} />
          )}
          <div className={styles.icon}>{icon}</div>
        </ConfirmationAction>
      </div>
    );
  }

  return (
    <div
      className={`${styles.tableRowAction} ${
        isFetching ? styles.fetching : ""
      } ${directionRtl ? styles.rtl : ""}`}
      title={title}
      onClick={handleClick}
    >
      {isFetching && <CircularProgress size={20} className={styles.loader} />}
      <div className={styles.icon}>{icon}</div>
    </div>
  );
};

export const TableRowAction = withAccessRestriction(TableRowActionComponent);
