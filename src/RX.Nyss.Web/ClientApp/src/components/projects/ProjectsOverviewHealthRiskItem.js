import styles from "./ProjectsOverviewHealthRiskItem.module.scss";
import Grid from '@material-ui/core/Grid';
import Typography from '@material-ui/core/Typography';
import React, { Fragment } from 'react';
import { stringKeys, strings } from '../../strings';
import { Card, CardContent } from "@material-ui/core";

export const ProjectsOverviewHealthRiskItem = ({ projectHealthRisk }) => {

  return (
    <Card>
      <CardContent className={styles.healthRisk}>
        <Typography variant="h2" display="inline">{projectHealthRisk.healthRiskCode}</Typography>
        <Typography variant="h3" style={{ marginLeft: "10px" }} display="inline"> {projectHealthRisk.healthRiskName}</Typography>
        <Grid container spacing={3} className={styles.healthRiskTextArea}>
          <Grid item xs={12} sm={6}>
            <Typography variant="h6" >
              {strings(stringKeys.project.form.caseDefinition)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {projectHealthRisk.caseDefinition}
            </Typography>
          </Grid>

          <Grid item xs={12} sm={6}>
            <Typography variant="h6" >
              {strings(stringKeys.project.form.feedbackMessage)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {projectHealthRisk.feedbackMessage}
            </Typography>
          </Grid>
        </Grid>
        <Typography variant="h3">{strings(stringKeys.project.form.alertsSetion)}</Typography>

        {projectHealthRisk.alertRuleCountThreshold === 0 && (
          <Typography variant="body1" style={{ color: "#a0a0a0" }}>{strings(stringKeys.common.boolean.false)}</Typography>
        )}

        {projectHealthRisk.alertRuleCountThreshold > 0 && (
          <Fragment>
            <Grid container spacing={1}>
              <Grid item xs={4}>
                <Typography variant="h6" >
                  {strings(stringKeys.project.form.alertRuleCountThreshold)}
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {projectHealthRisk.alertRuleCountThreshold}
                </Typography>
              </Grid>

              {projectHealthRisk.alertRuleCountThreshold > 1 && (
                <Fragment>
                  <Grid item xs={4}>
                    <Typography variant="h6" >
                      {strings(stringKeys.project.form.alertRuleDaysThreshold)}
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {projectHealthRisk.alertRuleDaysThreshold}
                    </Typography>
                  </Grid>

                  <Grid item xs={4}>
                    <Typography variant="h6" >
                      {strings(stringKeys.project.form.alertRuleKilometersThreshold)}
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {projectHealthRisk.alertRuleKilometersThreshold}
                    </Typography>
                  </Grid>
                </Fragment>
              )}
            </Grid>
          </Fragment>
        )}
      </CardContent>
    </Card>
  );
}
