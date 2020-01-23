import React from "react";
import MenuItem from "@material-ui/core/MenuItem";
import { useAccessRestriction } from "../hasAccess/HasAccess";

const TableRowMenuItemComponent = ({ id, title, action, handleDropdownClose }) => {
    const handleMenuItemClick = (e, action) => {
        e.stopPropagation();
        action();
        handleDropdownClose();
    };

    return (
        <MenuItem key={`${title}-${id}`} onClick={(e) => { handleMenuItemClick(e, action) }}>
            {title}
        </MenuItem>
    );
}

export const TableRowMenuItem = useAccessRestriction(TableRowMenuItemComponent)