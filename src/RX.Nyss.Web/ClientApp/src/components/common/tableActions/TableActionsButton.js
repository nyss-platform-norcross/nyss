import React from "react";
import Button from '@material-ui/core/Button';
import { useAccessRestriction } from "../hasAccess/HasAccess";

const TableActionsButtonComponent = ({ onClick, icon, children, className }) => (
  <Button onClick={onClick} variant="outlined" color="primary" startIcon={icon} className={className}>
    {children}
  </Button>
);

export const TableActionsButton = useAccessRestriction(TableActionsButtonComponent)
