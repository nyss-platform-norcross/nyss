import styles from "./TableRowAction.module.scss";

export const TableRowActions = ({ children, directionRtl, ...rest }) => (
  <div
    className={`${styles.tableRowActions} ${directionRtl ? styles.rtl : ""}`}
    {...rest}
  >
    {children}
  </div>
);
