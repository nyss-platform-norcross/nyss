import styles from "./TableActionsButton.module.scss"

import React from "react";
import Button from '@material-ui/core/Button';
import CircularProgress from "@material-ui/core/CircularProgress";
import { useAccessRestriction } from "../hasAccess/HasAccess";

const TableActionsButtonComponent = ({ onClick, icon, isFetching, children }) => (
  <Button onClick={onClick} variant="outlined" color="primary" startIcon={icon} className={styles.button} disabled={isFetching}>
    {isFetching && <CircularProgress size={16} className={styles.progressIcon} />}
    {children}
  </Button>
);

export const TableActionsButton = useAccessRestriction(TableActionsButtonComponent)
