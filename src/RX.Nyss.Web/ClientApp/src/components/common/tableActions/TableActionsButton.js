import React from "react";
import Button from '@material-ui/core/Button';
import { useAccessRestriction } from "../hasAccess/HasAccess";

const TableActionsButtonComponent = ({ onClick, icon, children }) => (
  <Button onClick={onClick} variant="outlined" color="primary" startIcon={icon}>
    {children}
  </Button>
);

export const TableActionsButton = useAccessRestriction(TableActionsButtonComponent)
