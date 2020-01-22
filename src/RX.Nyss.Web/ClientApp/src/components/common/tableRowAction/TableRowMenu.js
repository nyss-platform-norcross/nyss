import styles from "./TableRowAction.module.scss";
import React, { Fragment, useState } from "react";
import CircularProgress from "@material-ui/core/CircularProgress";
import Menu from "@material-ui/core/Menu";
import MenuItem from "@material-ui/core/MenuItem";

export const TableRowMenu = ({ id, icon, items, isFetching }) => {
  const [anchorEl, setAnchorEl] = useState(null);

  const handleDropdownClick = (e) => {
    e.stopPropagation();
    setAnchorEl(e.currentTarget);
  }

  const handleDropdownClose = (e) => {
    e && e.stopPropagation();
    setAnchorEl(null);
  };

  const handleMenuClick = (e, onClick) => {
    e.stopPropagation();
    onClick();
    handleDropdownClose();
  };

  return (
    items.some(item => item.condition) && (
      <Fragment>
        <div className={`${styles.tableRowAction} ${(isFetching ? styles.fetching : "")}`} title={`more...`} onClick={handleDropdownClick}>
          {isFetching && <CircularProgress size={20} className={styles.loader} />}
          <div className={styles.icon}>
            {icon}
          </div>
        </div>
        <Menu
          anchorEl={anchorEl}
          onClose={handleDropdownClose}
          open={Boolean(anchorEl)}>

          {items.map(menuItem => (
            <MenuItem key={`${menuItem.title}-${id}`} onClick={(e) => { handleMenuClick(e, menuItem.action) }}>
              {menuItem.title}
            </MenuItem>
          ))}

        </Menu>
      </Fragment>)
  );
};
