import React from "react";
import { Button } from '@material-ui/core';
import ArrowBackIcon from '@material-ui/icons/ArrowBack';
import styles from "./GoBackToButton.module.scss"

export const GoBackToButton = ({ onClick, variant, children }) => (
  <Button
    onClick={onClick}
    startIcon={<ArrowBackIcon />}
    variant={variant}
    color="primary"
    className={styles.button}
  >
    {children}
  </Button>
);

export default GoBackToButton;
