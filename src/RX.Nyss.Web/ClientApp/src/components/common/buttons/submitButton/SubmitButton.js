import styles from "./SubmitButton.module.scss";
import React from "react";
import PropTypes from "prop-types";
import { Button, CircularProgress } from "@material-ui/core";

export const SubmitButton = ({
  onClick,
  regular,
  isFetching,
  children,
  wide,
  disabled,
  underline,
}) => {
  return (
    <Button
      color="primary"
      variant={regular ? undefined : "contained"}
      onClick={onClick}
      disabled={isFetching || disabled}
      type="submit"
      className={`${wide && styles.wide} ${underline && styles.underline}`}
    >
      {isFetching && (
        <CircularProgress size={16} className={styles.progressIcon} />
      )}
      {children}
    </Button>
  );
};

SubmitButton.propTypes = {
  onClick: PropTypes.func,
  label: PropTypes.string,
};

export default SubmitButton;
