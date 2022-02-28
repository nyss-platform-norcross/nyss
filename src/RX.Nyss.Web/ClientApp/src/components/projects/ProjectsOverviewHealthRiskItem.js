import styles from "./ProjectsOverviewHealthRiskItem.module.scss";
import React, { Fragment } from 'react';
import { stringKeys, strings } from '../../strings';
import { Card, CardContent, Grid, Typography } from "@material-ui/core";

export const ProjectsOverviewHealthRiskItem = ({ projectHealthRisk, rtl }) => {

  return (
    <Card>
      <CardContent className={styles.healthRisk}>
        <Typography variant="h2" className={styles.header}>{projectHealthRisk.healthRiskCode}</Typography>
        <Typography variant="h3" className={`${styles.header} ${styles.healthRiskName} ${rtl ? styles.rtl : ""}`}> {projectHealthRisk.healthRiskName}</Typography>
        <Grid container spacing={2} className={styles.healthRiskTextArea}>
          <Grid item xs={12} sm={6}>
            <Typography variant="h6">
              {strings(stringKeys.project.form.caseDefinition)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {projectHealthRisk.caseDefinition}
            </Typography>
          </Grid>

          <Grid item xs={12} sm={6}>
            <Typography variant="h6">
              {strings(stringKeys.project.form.feedbackMessage)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {projectHealthRisk.feedbackMessage}
            </Typography>
          </Grid>
        </Grid>
        <Typography variant="h3">{strings(stringKeys.project.form.alertsSection)}</Typography>

        {(projectHealthRisk.healthRiskCode === 98 || projectHealthRisk.healthRiskCode === 99) && (
          <Typography variant="body1"
                      style={{ color: "#a0a0a0" }}>{strings(stringKeys.healthRisk.form.noAlertRule)}
          </Typography>
        )}

        {projectHealthRisk.healthRiskCode !== 99 && projectHealthRisk.healthRiskCode !== 98 && (
          <Fragment>

            {projectHealthRisk.alertRuleCountThreshold === 0 && (
              <Typography variant="body1" style={{ color: "#a0a0a0" }}>{strings(stringKeys.common.boolean.false)}</Typography>
            )}

            {projectHealthRisk.alertRuleCountThreshold > 0 && (
              <Fragment>
                <Grid container spacing={2}>
                  <Grid item xs={4}>
                    <Typography variant="h6">
                      {strings(stringKeys.project.form.alertRuleCountThreshold)}
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {projectHealthRisk.alertRuleCountThreshold}
                    </Typography>
                  </Grid>

                  {projectHealthRisk.alertRuleCountThreshold > 1 && (
                    <Fragment>
                      <Grid item xs={4}>
                        <Typography variant="h6">
                          {strings(stringKeys.project.form.alertRuleDaysThreshold)}
                        </Typography>
                        <Typography variant="body1" gutterBottom>
                          {projectHealthRisk.alertRuleDaysThreshold}
                        </Typography>
                      </Grid>

                      <Grid item xs={4}>
                        <Typography variant="h6">
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
          </Fragment>
        )}
      </CardContent>
    </Card>
  );
}
