import React, { useState, useEffect } from 'react';
import { validators } from '../../utils/forms';
import Typography from '@material-ui/core/Typography';
import TextInputField from '../forms/TextInputField';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { Card, CardContent } from '@material-ui/core';

export const ProjectsHealthRiskItem = ({ form, healthRisk, projectHealthRisk }) => {
  const [ready, setReady] = useState(false);
  const [reportCountThreshold, setReportCountThreshold] = useState(healthRisk.alertRuleCountThreshold || 0);

  useMount(() => {
    form.addField(`healthRisk_${healthRisk.healthRiskId}_projectHealthRiskId`, projectHealthRisk.id);
    form.addField(`healthRisk_${healthRisk.healthRiskId}_caseDefinition`, healthRisk.caseDefinition, [validators.required, validators.maxLength(500)]);
    form.addField(`healthRisk_${healthRisk.healthRiskId}_feedbackMessage`, healthRisk.feedbackMessage, [validators.required, validators.maxLength(160)]);
    form.addField(`healthRisk_${healthRisk.healthRiskId}_alertRuleCountThreshold`, healthRisk.alertRuleCountThreshold, [validators.nonNegativeNumber]);
    form.addField(`healthRisk_${healthRisk.healthRiskId}_alertRuleDaysThreshold`,
      healthRisk.alertRuleDaysThreshold,
      [
        validators.requiredWhen(f => f[`healthRisk_${healthRisk.healthRiskId}_alertRuleCountThreshold`] > 1),
        validators.inRange(1, 365)
      ]);
    form.addField(`healthRisk_${healthRisk.healthRiskId}_alertRuleKilometersThreshold`,
      healthRisk.alertRuleKilometersThreshold,
      [
        validators.requiredWhen(f => f[`healthRisk_${healthRisk.healthRiskId}_alertRuleCountThreshold`] > 1),
        validators.inRange(1, 9999)
      ]);

    form.fields[`healthRisk_${healthRisk.healthRiskId}_alertRuleCountThreshold`].subscribe(({ newValue }) => setReportCountThreshold(newValue));
    setReady(true);

    return () => {
      form.removeField(`healthRisk_${healthRisk.healthRiskId}_projectHealthRiskId`);
      form.removeField(`healthRisk_${healthRisk.healthRiskId}_caseDefinition`);
      form.removeField(`healthRisk_${healthRisk.healthRiskId}_feedbackMessage`);
      form.removeField(`healthRisk_${healthRisk.healthRiskId}_alertRuleCountThreshold`);
      form.removeField(`healthRisk_${healthRisk.healthRiskId}_alertRuleDaysThreshold`);
      form.removeField(`healthRisk_${healthRisk.healthRiskId}_alertRuleKilometersThreshold`);
    };
  })

  useEffect(() => {
    if (reportCountThreshold <= 1){
      form.fields[`healthRisk_${healthRisk.healthRiskId}_alertRuleDaysThreshold`].update("");
      form.fields[`healthRisk_${healthRisk.healthRiskId}_alertRuleKilometersThreshold`].update("");
    }
    return;
  }, [form, reportCountThreshold, healthRisk])

  if (!ready) {
    return null;
  }

  return (
    <Card key={`healthRisk_${healthRisk.healthRiskId}`}>
      <CardContent>
        <Typography variant="h3">
          <Typography variant="h2" display="inline" style={{ marginRight: "10px" }}>{healthRisk.healthRiskCode}</Typography> {healthRisk.healthRiskName}
        </Typography>
        <Grid container spacing={3}>
          <Grid item xs={12} sm={6}>
            <TextInputField
              label={strings(stringKeys.project.form.caseDefinition)}
              name={`healthRisk_${healthRisk.healthRiskId}_caseDefinition`}
              field={form.fields[`healthRisk_${healthRisk.healthRiskId}_caseDefinition`]}
              multiline
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextInputField
              label={strings(stringKeys.project.form.feedbackMessage)}
              name={`healthRisk_${healthRisk.healthRiskId}_feedbackMessage`}
              field={form.fields[`healthRisk_${healthRisk.healthRiskId}_feedbackMessage`]}
              multiline
            />
          </Grid>
        </Grid>

        <Typography variant="h3">{strings(stringKeys.project.form.alertsSetion)}</Typography>
        <Grid container spacing={3}>
          <Grid item xs={12} sm={4}>
            <TextInputField
              label={strings(stringKeys.project.form.alertRuleCountThreshold)}
              name={`healthRisk_${healthRisk.healthRiskId}_alertRuleCountThreshold`}
              field={form.fields[`healthRisk_${healthRisk.healthRiskId}_alertRuleCountThreshold`]}
            />
          </Grid>
              <Grid item xs={12} sm={4}>
                <TextInputField
                  label={strings(stringKeys.project.form.alertRuleDaysThreshold)}
                  name={`healthRisk_${healthRisk.healthRiskId}_alertRuleDaysThreshold`}
                  field={form.fields[`healthRisk_${healthRisk.healthRiskId}_alertRuleDaysThreshold`]}
                  disabled={!reportCountThreshold || reportCountThreshold <= 1}
                />
              </Grid>
              <Grid item xs={12} sm={4}>
                <TextInputField
                  label={strings(stringKeys.project.form.alertRuleKilometersThreshold)}
                  name={`healthRisk_${healthRisk.healthRiskId}_alertRuleKilometersThreshold`}
                  field={form.fields[`healthRisk_${healthRisk.healthRiskId}_alertRuleKilometersThreshold`]}
                  disabled={!reportCountThreshold || reportCountThreshold <= 1}
                />
              </Grid>
        </Grid>
      </CardContent>
    </Card>
  );
}
