import styles from "./CancelButton.module.scss";
import React from "react";
import PropTypes from "prop-types";
import { Button } from "@material-ui/core";

export const CancelButton = ({ onClick, children, variant }) => {
  return (
    <Button
      variant={variant}
      onClick={onClick}
      type="submit"
      className={styles.regular}
    >
      {children}
    </Button>
  );
};

CancelButton.propTypes = {
  onClick: PropTypes.func,
  label: PropTypes.string
};

export default CancelButton;