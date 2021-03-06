import styles from './ProjectHealthRiskItem.module.scss';
import React, { useState, Fragment, useEffect } from 'react';
import { validators } from '../../utils/forms';
import TextInputField from '../forms/TextInputField';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { Card, CardContent, Typography, Grid } from '@material-ui/core';

export const ProjectsHealthRiskItem = ({ form, healthRisk, projectHealthRisk }) => {
  const [ready, setReady] = useState(false);
  const [reportCountThreshold, setReportCountThreshold] = useState(healthRisk.alertRuleCountThreshold || 0);

  useMount(() => {
    form.addField(`healthRisk.${healthRisk.healthRiskId}.projectHealthRiskId`, projectHealthRisk.id);
    form.addField(`healthRisk.${healthRisk.healthRiskId}.caseDefinition`, healthRisk.caseDefinition, [validators.required, validators.maxLength(500)]);
    form.addField(`healthRisk.${healthRisk.healthRiskId}.feedbackMessage`, healthRisk.feedbackMessage, [validators.required, validators.maxLength(160)]);
    form.addField(`healthRisk.${healthRisk.healthRiskId}.alertRuleCountThreshold`, healthRisk.alertRuleCountThreshold, [validators.nonNegativeNumber]);
    form.addField(`healthRisk.${healthRisk.healthRiskId}.alertRuleDaysThreshold`,
      healthRisk.alertRuleDaysThreshold,
      [
        validators.requiredWhen(f => f[`healthRisk.${healthRisk.healthRiskId}.alertRuleCountThreshold`] > 1),
        validators.inRange(1, 365)
      ]);
    form.addField(`healthRisk.${healthRisk.healthRiskId}.alertRuleKilometersThreshold`,
      healthRisk.alertRuleKilometersThreshold,
      [
        validators.requiredWhen(f => f[`healthRisk.${healthRisk.healthRiskId}.alertRuleCountThreshold`] > 1),
        validators.inRange(1, 9999)
      ]);

    form.fields[`healthRisk.${healthRisk.healthRiskId}.alertRuleCountThreshold`].subscribe(({ newValue }) => setReportCountThreshold(newValue));
    setReady(true);

    return () => {
      form.removeField(`healthRisk.${healthRisk.healthRiskId}.projectHealthRiskId`);
      form.removeField(`healthRisk.${healthRisk.healthRiskId}.caseDefinition`);
      form.removeField(`healthRisk.${healthRisk.healthRiskId}.feedbackMessage`);
      form.removeField(`healthRisk.${healthRisk.healthRiskId}.alertRuleCountThreshold`);
      form.removeField(`healthRisk.${healthRisk.healthRiskId}.alertRuleDaysThreshold`);
      form.removeField(`healthRisk.${healthRisk.healthRiskId}.alertRuleKilometersThreshold`);
    };
  })

  useEffect(() => {
    if (reportCountThreshold <= 1) {
      form.fields[`healthRisk.${healthRisk.healthRiskId}.alertRuleDaysThreshold`].update("");
      form.fields[`healthRisk.${healthRisk.healthRiskId}.alertRuleKilometersThreshold`].update("");
    }
  }, [form, reportCountThreshold, healthRisk])

  if (!ready) {
    return null;
  }

  return (
    <Card>
      <CardContent>
        <Typography variant="h2" className={styles.header}>{healthRisk.healthRiskCode}</Typography>
        <Typography variant="h3" className={`${styles.header} ${styles.healthRiskName}`}>{healthRisk.healthRiskName}</Typography>
        <Grid container spacing={2} className={styles.content}>
          <Grid item xs={12} sm={6}>
            <TextInputField
              label={strings(stringKeys.project.form.caseDefinition)}
              name={`healthRisk.${healthRisk.healthRiskId}.caseDefinition`}
              field={form.fields[`healthRisk.${healthRisk.healthRiskId}.caseDefinition`]}
              multiline
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextInputField
              label={strings(stringKeys.project.form.feedbackMessage)}
              name={`healthRisk.${healthRisk.healthRiskId}.feedbackMessage`}
              field={form.fields[`healthRisk.${healthRisk.healthRiskId}.feedbackMessage`]}
              multiline
            />
          </Grid>
        </Grid>

        <Typography variant="h3">{strings(stringKeys.project.form.alertsSection)}</Typography>

        {healthRisk.healthRiskType === 'Activity' && (
          <Typography variant="body1" className={styles.disabled}>{strings(stringKeys.healthRisk.form.noAlertRule)}
          </Typography>
        )}

        {healthRisk.healthRiskType !== 'Activity' && (
          <Fragment>
            <Grid container spacing={2}>
              <Grid item xs={12} sm={4}>
                <TextInputField
                  label={strings(stringKeys.project.form.alertRuleCountThreshold)}
                  name={`healthRisk.${healthRisk.healthRiskId}.alertRuleCountThreshold`}
                  field={form.fields[`healthRisk.${healthRisk.healthRiskId}.alertRuleCountThreshold`]}
                  inputMode={"numeric"}
                />
              </Grid>
              <Grid item xs={12} sm={4}>
                <TextInputField
                  label={strings(stringKeys.project.form.alertRuleDaysThreshold)}
                  name={`healthRisk.${healthRisk.healthRiskId}.alertRuleDaysThreshold`}
                  field={form.fields[`healthRisk.${healthRisk.healthRiskId}.alertRuleDaysThreshold`]}
                  disabled={!reportCountThreshold || reportCountThreshold <= 1}
                  inputMode={"numeric"}
                />
              </Grid>
              <Grid item xs={12} sm={4}>
                <TextInputField
                  label={strings(stringKeys.project.form.alertRuleKilometersThreshold)}
                  name={`healthRisk.${healthRisk.healthRiskId}.alertRuleKilometersThreshold`}
                  field={form.fields[`healthRisk.${healthRisk.healthRiskId}.alertRuleKilometersThreshold`]}
                  disabled={!reportCountThreshold || reportCountThreshold <= 1}
                  inputMode={"numeric"}
                />
              </Grid>
            </Grid>
          </Fragment>
        )}

      </CardContent>
    </Card>
  );
}
