import styles from "./CancelButton.module.scss";
import PropTypes from "prop-types";
import { Button } from "@material-ui/core";

export const CancelButton = ({ onClick, children, variant }) => {
  return (
    <Button
      variant={variant ? variant : "text"}
      color="primary"
      onClick={onClick}
      className={styles.regular}
    >
      {children}
    </Button>
  );
};

CancelButton.propTypes = {
  onClick: PropTypes.func,
  label: PropTypes.string,
};

export default CancelButton;
