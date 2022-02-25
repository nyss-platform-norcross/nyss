import styles from "./TableRowAction.module.scss";

export const TableRowActions = ({ children, directionRightToLeft, ...rest }) => (
  <div className={directionRightToLeft ? `${styles.tableRowActions} ${styles.rightToLeft}` : styles.tableRowActions} {...rest}>
    {children}
  </div>
);
