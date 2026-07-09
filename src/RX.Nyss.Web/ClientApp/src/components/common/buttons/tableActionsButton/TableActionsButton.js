import styles from "./TableActionsButton.module.scss";

import React from "react";
import { Button, CircularProgress } from "@material-ui/core";
import { withAccessRestriction } from "../../hasAccess/HasAccess";

const TableActionsButtonComponent = ({
  onClick,
  startIcon,
  isFetching,
  children,
  variant,
  style,
}) => (
  <Button
    onClick={onClick}
    startIcon={startIcon}
    variant={variant}
    color="primary"
    className={styles.button}
    disabled={isFetching}
    style={style}
  >
    {isFetching && (
      <CircularProgress size={16} className={styles.progressIcon} />
    )}
    {children}
  </Button>
);

export const TableActionsButton = withAccessRestriction(
  TableActionsButtonComponent,
);
