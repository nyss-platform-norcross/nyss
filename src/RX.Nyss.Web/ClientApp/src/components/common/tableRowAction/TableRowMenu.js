import styles from "./TableRowAction.module.scss";
import React, { Fragment, useState } from "react";
import { Menu, CircularProgress, MenuItem } from "@material-ui/core";
import { useSelector } from "react-redux";

const TableRowMenuComponent = ({
  id,
  icon,
  items,
  isFetching,
  alwaysShow,
  alwaysHighlighted,
  directionRtl,
}) => {
  const [anchorEl, setAnchorEl] = useState(null);
  const user = useSelector((state) => state.appData.user);

  const handleDropdownClick = (e) => {
    e.stopPropagation();

    filteredItems.length !== 0 && setAnchorEl(e.currentTarget);
  };

  const handleDropdownClose = (e) => {
    e && e.stopPropagation();
    setAnchorEl(null);
  };

  const handleMenuItemClick = (e, action) => {
    e.stopPropagation();
    action();
    handleDropdownClose();
  };

  const filteredItems = items.filter(
    (item) =>
      !item.roles || user.roles.some((role) => item.roles.indexOf(role) > -1),
  );

  if (!filteredItems.length && !alwaysShow) {
    return null;
  }

  return (
    <Fragment>
      <div
        className={`${styles.tableRowAction} ${
          alwaysHighlighted ? styles.alwaysHighlighted : ""
        } ${isFetching ? styles.fetching : ""} ${
          directionRtl ? styles.rtl : ""
        }`}
        title={`more...`}
        onClick={handleDropdownClick}
      >
        {isFetching && <CircularProgress size={20} className={styles.loader} />}
        <div className={styles.icon}>{icon}</div>
      </div>
      <Menu
        key={`tableRowMenu_${id}`}
        anchorEl={anchorEl}
        onClose={handleDropdownClose}
        open={Boolean(anchorEl)}
      >
        {filteredItems.map((menuItem, index) => (
          <MenuItem
            key={`row_${id}_menuItem_${index}`}
            title={menuItem.title}
            disabled={!!menuItem.disabled}
            onClick={(e) => handleMenuItemClick(e, menuItem.action)}
          >
            {menuItem.title}
          </MenuItem>
        ))}
      </Menu>
    </Fragment>
  );
};

export const TableRowMenu = TableRowMenuComponent;
