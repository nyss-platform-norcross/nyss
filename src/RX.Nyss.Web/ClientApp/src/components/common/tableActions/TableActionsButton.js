import styles from "./TableActionsButton.module.scss"

import React from "react";
import { Button, CircularProgress } from '@material-ui/core';
import { withAccessRestriction } from "../hasAccess/HasAccess";

const TableActionsButtonComponent = ({ onClick, icon, isFetching, children }) => (
  <Button onClick={onClick} variant="outlined" color="primary" startIcon={icon} className={styles.button} disabled={isFetching}>
    {isFetching && <CircularProgress size={16} className={styles.progressIcon} />}
    {children}
  </Button>
);

export const TableActionsButton = withAccessRestriction(TableActionsButtonComponent)
