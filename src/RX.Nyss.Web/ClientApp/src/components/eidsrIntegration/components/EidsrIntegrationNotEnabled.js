import React from "react";
import { Grid, Typography } from "@material-ui/core";
import WarningIcon from "@material-ui/icons/Warning";
import { stringKeys, strings } from "../../../strings";

export class EidsrIntegrationNotEnabled extends React.Component {
  render() {
    return (
      <Grid container spacing={2} alignItems="center">
        <Grid item>
          <Grid container direction="row" spacing={2} alignItems="center">
            <Grid item xs={2} style={{ textAlign: "center" }}>
              <WarningIcon
                color="error"
                style={{ fontSize: "45px", verticalAlign: "bottom" }}
              />
            </Grid>
            <Grid item xs={10}>
              <Typography variant="body1">
                {strings(stringKeys.eidsrIntegration.disabled)}
              </Typography>
            </Grid>
          </Grid>
        </Grid>
      </Grid>
    );
  }
}
