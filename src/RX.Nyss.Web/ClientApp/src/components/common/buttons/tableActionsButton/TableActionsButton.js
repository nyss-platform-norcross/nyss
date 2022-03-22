import styles from "./TableActionsButton.module.scss"

import React from "react";
import { Button, CircularProgress } from '@material-ui/core';
import { withAccessRestriction } from "../../hasAccess/HasAccess";
import AddIcon from '@material-ui/icons/Add';

const TableActionsButtonComponent = ({ onClick, add, isFetching, children, variant, rtl }) => (
  <Button
    onClick={onClick}
    startIcon={add && <AddIcon className={rtl ? styles.rtl : null} />}
    variant={variant}
    color="primary"
    className={styles.button}
    disabled={isFetching}
  >
    {isFetching && <CircularProgress size={16} className={styles.progressIcon} />}
    {children}
  </Button>
);

export const TableActionsButton = withAccessRestriction(TableActionsButtonComponent)
