import React, { Fragment } from "react";
import { Collapse } from "@material-ui/core";

export const ConditionalCollapse = ({ children, collapsible, expanded }) => (
  <Fragment>
    {collapsible && (
      <Collapse in={expanded} timeout="auto" unmountOnExit>
        {children}
      </Collapse>
    )}

    {!collapsible && children}
  </Fragment>
);
