import styles from "./TableRowAction.module.scss";
import React, { Fragment, useState } from "react";
import CircularProgress from "@material-ui/core/CircularProgress";
import Menu from "@material-ui/core/Menu";
import { TableRowMenuItem } from "./TableRowMenuItem";
import { useUser } from "../hasAccess/HasUser";


const TableRowMenuComponent = ({ id, icon, items, isFetching, user }) => {
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
    items.some(item => item.condition === undefined || item.condition) && items.some(item => !item.roles || user.roles.some(role => item.roles.indexOf(role) > -1)) && (
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
              <TableRowMenuItem 
                key={menuItem.id}
                id={menuItem.id} title={menuItem.title} roles={menuItem.roles} condition={menuItem.condition}
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

export const TableRowMenu = useUser(TableRowMenuComponent);