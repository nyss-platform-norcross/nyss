import styles from "./ProjectsOverviewHealthRiskItem.module.scss";
import React, { Fragment } from "react";
import { stringKeys, strings } from "../../strings";
import {
  Card,
  CardContent,
  Grid,
  Typography,
  makeStyles,
} from "@material-ui/core";

const useStyles = makeStyles({
  cardBodyText: {
    fontSize: 16,
  },
  cardInfoText: {
    color: "#a0a0a0",
  },
});

export const ProjectsOverviewHealthRiskItem = ({ projectHealthRisk, rtl }) => {
  const classes = useStyles();
  return (
    <Card>
      <CardContent className={styles.healthRisk}>
        <Typography variant="h5" className={`${styles.header}`}>
          {projectHealthRisk.healthRiskCode}
        </Typography>
        <Typography
          className={`${styles.header} ${styles.healthRiskName} ${
            rtl ? styles.rtl : ""
          }`}
        >
          {" "}
          {projectHealthRisk.healthRiskName}
        </Typography>
        <Grid container spacing={2} className={styles.healthRiskTextArea}>
          <Grid item xs={12} sm={6}>
            <Typography variant={"h5"}>
              {strings(stringKeys.project.form.caseDefinition)}
            </Typography>
            <Typography
              variant={"body1"}
              className={classes.cardBodyText}
              gutterBottom
            >
              {projectHealthRisk.caseDefinition}
            </Typography>
          </Grid>

          <Grid item xs={12} sm={6}>
            <Typography variant={"h5"}>
              {strings(stringKeys.project.form.feedbackMessage)}
            </Typography>
            <Typography
              variant={"body1"}
              className={classes.cardBodyText}
              gutterBottom
            >
              {projectHealthRisk.feedbackMessage}
            </Typography>
          </Grid>
        </Grid>
        <Typography variant={"h5"}>
          {strings(stringKeys.project.form.alertsSection)}
        </Typography>

        {(projectHealthRisk.healthRiskCode === 98 ||
          projectHealthRisk.healthRiskCode === 99) && (
          <Typography
            variant="body1"
            className={`${classes.cardBodyText} ${classes.cardInfoText}`}
          >
            {strings(stringKeys.healthRisk.form.noAlertRule)}
          </Typography>
        )}

        {projectHealthRisk.healthRiskCode !== 99 &&
          projectHealthRisk.healthRiskCode !== 98 && (
            <Fragment>
              {projectHealthRisk.alertRuleCountThreshold === 0 && (
                <Typography
                  variant="body1"
                  className={`${classes.cardBodyText} ${classes.cardInfoText}`}
                >
                  {strings(stringKeys.common.boolean.false)}
                </Typography>
              )}

              {projectHealthRisk.alertRuleCountThreshold > 0 && (
                <Fragment>
                  <Grid container spacing={2}>
                    <Grid item xs={4}>
                      <Grid container>
                        <Typography className={classes.cardBodyText}>
                          {strings(
                            stringKeys.project.form.alertRuleCountThreshold,
                          )}
                          :
                        </Typography>
                        <Typography
                          className={styles.alertRuleData}
                          gutterBottom
                        >
                          {projectHealthRisk.alertRuleCountThreshold}
                        </Typography>
                      </Grid>
                    </Grid>

                    {projectHealthRisk.alertRuleCountThreshold > 1 && (
                      <Fragment>
                        <Grid item xs={4}>
                          <Grid container>
                            <Typography className={classes.cardBodyText}>
                              {strings(
                                stringKeys.project.form.alertRuleDaysThreshold,
                              )}
                              :
                            </Typography>
                            <Typography
                              className={styles.alertRuleData}
                              gutterBottom
                            >
                              {projectHealthRisk.alertRuleDaysThreshold}
                            </Typography>
                            <Typography
                              className={styles.alertRuleData}
                              gutterBottom
                            >
                              {projectHealthRisk.alertRuleDaysThreshold == 1
                                ? strings(stringKeys.project.form.alertRuleDay)
                                : strings(
                                    stringKeys.project.form.alertRuleDays,
                                  )}
                            </Typography>
                          </Grid>
                        </Grid>

                        <Grid item xs={4}>
                          <Grid container>
                            <Typography className={classes.cardBodyText}>
                              {strings(
                                stringKeys.project.form
                                  .alertRuleKilometersThreshold,
                              )}
                              :
                            </Typography>
                            <Typography
                              className={styles.alertRuleData}
                              gutterBottom
                            >
                              {projectHealthRisk.alertRuleKilometersThreshold}
                            </Typography>
                            <Typography
                              className={styles.alertRuleData}
                              gutterBottom
                            >
                              {strings(
                                stringKeys.project.form.alertRuleKilometer,
                              )}
                            </Typography>
                          </Grid>
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
};
