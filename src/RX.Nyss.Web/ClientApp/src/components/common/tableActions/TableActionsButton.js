import styles from './TableActionsButton.module.scss';
import React from "react";
import Button from '@material-ui/core/Button';
import { useAccessRestriction } from "../hasAccess/HasAccess";

const TableActionsButtonComponent = ({ onClick, icon, children }) => (
  <Button onClick={onClick} variant="outlined" color="primary" startIcon={icon} className={styles.button}>
    {children}
  </Button>
);

export const TableActionsButton = useAccessRestriction(TableActionsButtonComponent)
