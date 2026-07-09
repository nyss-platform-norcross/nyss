import React, { useState, useRef } from "react";
import { connect, useSelector } from "react-redux";
import { push } from "connected-react-router";

import { Button, Typography, makeStyles } from "@material-ui/core";

import ClickAwayListener from "@material-ui/core/ClickAwayListener";
import Popper from "@material-ui/core/Popper";
import MenuItem from "@material-ui/core/MenuItem";
import MenuList from "@material-ui/core/MenuList";
import ExpandMoreIcon from "@material-ui/icons/ExpandMore";
import ExpandLessIcon from "@material-ui/icons/ExpandLess";

export const TabDropdownComponent = ({ projectTabMenuPage, onItemClick }) => {
  const [open, setOpen] = useState(false);
  const anchorRef = useRef(null);
  const useRtlDirection = useSelector(
    (state) => state.appData.direction === "rtl",
  );

  // If the page has only 1 submenu item, then we want to display the submenu as the page
  // Used to display healthRisks instead of settings for Supervisors
  const page =
    projectTabMenuPage.subMenu?.length === 1
      ? projectTabMenuPage.subMenu[0]
      : projectTabMenuPage;

  const useStyles = makeStyles((theme) => ({
    container: {
      position: "relative",
      backgroundColor: "inherit",
      border: open ? "1px solid #E3E3E3" : "1px solid transparent",
      borderRadius: "10px 10px 0px 0px",
    },
    tab: {
      borderBottom: page.isActive
        ? `3px solid ${theme.palette.primary.main}`
        : "3px solid transparent",
      padding: "8px 28px 8px 28px",
    },
    buttonRoot: {
      borderRadius: "10px 10px 0px 0px",
    },
    popper: {
      marginLeft: 1,
      width: anchorRef?.current?.offsetWidth,
      backgroundColor: "inherit",
      zIndex: 1002,
      border: open ? "1px solid #E3E3E3" : "none",
      borderTop: "none",
      borderBottomLeftRadius: 10,
      borderBottomRightRadius: 10,
    },
    menuList: {
      padding: 0,
      width: "100%",
      backgroundColor: "inherit",
      borderRadius: "0px 0px 10px 10px",
    },
    menuItem: {
      display: "flex",
      whiteSpace: "normal",
      padding: 10,
    },
    menuItemActive: {
      backgroundColor: "#E3E3E3",
    },
    lastMenuItem: {
      borderBottomLeftRadius: 10,
      borderBottomRightRadius: 10,
    },
  }));

  const styles = useStyles();

  const handleToggle = () => {
    if (!open && page.subMenu?.length > 1) {
      setOpen(true);
    } else if (open && page.subMenu?.length > 1) {
      setOpen(false);
    } else {
      onItemClick(page);
    }
  };

  const handleClose = (event) => {
    if (anchorRef.current && anchorRef.current.contains(event.target)) {
      return;
    }
    setOpen(false);
  };

  const handleListKeyDown = (event) => {
    if (event.key === "Tab") {
      event.preventDefault();
      setOpen(false);
    }
  };

  const handleMenuItemClick = (menuItem) => {
    onItemClick(menuItem);
  };

  return (
    <div
      className={styles.container}
      onMouseEnter={() => setOpen(true)}
      onMouseLeave={() => setOpen(false)}
    >
      <Button
        ref={anchorRef}
        aria-controls={open ? "menu-list-grow" : undefined}
        aria-haspopup="true"
        onClick={handleToggle}
        className={styles.tab}
        classes={{
          root: styles.buttonRoot,
        }}
      >
        <Typography
          variant="body1"
          style={{ fontWeight: page.isActive ? "bold" : "normal" }}
        >
          {page.title}
        </Typography>
        {page.subMenu?.length > 1 ? (
          open ? (
            <ExpandLessIcon fontSize="small" />
          ) : (
            <ExpandMoreIcon fontSize="small" />
          )
        ) : null}
      </Button>
      {/* No need to display the dropdown if it only has 1 menuitem */}
      {page.subMenu?.length > 1 && (
        <Popper
          className={styles.popper}
          open={open}
          anchorEl={anchorRef?.current}
          placement="bottom"
          disablePortal={true}
          transition
          modifiers={{
            preventOverflow: {
              enabled: false,
              boundariesElement: "scrollParent",
              escapeWithReference: true,
            },
            hide: {
              enabled: false,
            },
          }}
        >
          <ClickAwayListener onClickAway={handleClose}>
            <MenuList
              className={styles.menuList}
              autoFocusItem={open}
              id="menu-list-grow"
              onKeyDown={handleListKeyDown}
            >
              {page.subMenu?.map((menuItem, index) => (
                <MenuItem
                  className={`${styles.menuItem} ${
                    menuItem.isActive && styles.menuItemActive
                  } ${
                    index === page.subMenu?.length - 1 && styles.lastMenuItem
                  }`}
                  key={`menuItem_${menuItem.url}`}
                  onClick={() => handleMenuItemClick(menuItem)}
                >
                  <Typography
                    variant="body1"
                    style={{ textAlign: useRtlDirection ? "right" : "left" }}
                  >
                    {menuItem.title}
                  </Typography>
                </MenuItem>
              ))}
            </MenuList>
          </ClickAwayListener>
        </Popper>
      )}
    </div>
  );
};

const mapDispatchToProps = {
  push: push,
};

export const TabDropdown = connect(
  undefined,
  mapDispatchToProps,
)(TabDropdownComponent);
