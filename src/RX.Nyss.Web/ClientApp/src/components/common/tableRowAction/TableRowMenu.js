import styles from "./TableRowAction.module.scss";
import React, { Fragment, useState } from "react";
import CircularProgress from "@material-ui/core/CircularProgress";
import Menu from "@material-ui/core/Menu";
import { TableRowMenuItem } from "./TableRowMenuItem";


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

  return (
    items.every(item => item.condition) && (
      <Fragment>
        <div className={`${styles.tableRowAction} ${(isFetching ? styles.fetching : "")}`} title={`more...`} onClick={handleDropdownClick}>
          {isFetching && <CircularProgress size={20} className={styles.loader} />}
          <div className={styles.icon}>
            {icon}
          </div>
        </div>
        <Menu
          key={`tableRowMenu_${id}`}
          anchorEl={anchorEl}
          onClose={handleDropdownClose}
          open={Boolean(anchorEl)}
        >
          {
            items.map(menuItem => (
              <TableRowMenuItem id={menuItem.id} title={menuItem.title} roles={menuItem.roles} condition={menuItem.condition}
                action={menuItem.action} handleDropdownClose={handleDropdownClose}>
                {menuItem.title}
              </TableRowMenuItem>
            ))
          }
        </Menu>
      </Fragment>
    )
  );
};
