import styles from "./SubmitButton.module.scss";
import React from "react";
import PropTypes from "prop-types";
import Button from "@material-ui/core/Button";
import CircularProgress from "@material-ui/core/CircularProgress";

export const SubmitButton = ({ onClick, isFetching, label }) => {
  return (
    <Button color="primary" variant="outlined" onClick={onClick} disabled={isFetching} type="submit">
      {isFetching && <CircularProgress size={16} className={styles.progressIcon} />}
      {label}
    </Button>
  );
};

SubmitButton.propTypes = {
  onClick: PropTypes.func,
  label: PropTypes.string
};
