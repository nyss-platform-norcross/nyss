import React, { Fragment, useState } from 'react';
import { validators } from '../../utils/forms';
import Typography from '@material-ui/core/Typography';
import TextInputField from '../forms/TextInputField';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import Divider from '@material-ui/core/Divider';

export const ProjectsHealthRiskItem = ({ form, healthRisk, projectHealthRisk }) => {
  const [ready, setReady] = useState(false);

  useMount(() => {
    form.addField(`healthRisk_${healthRisk.healthRiskId}_projectHealthRiskId`, projectHealthRisk.id);
    form.addField(`healthRisk_${healthRisk.healthRiskId}_caseDefinition`, healthRisk.caseDefinition, [validators.required]);
    form.addField(`healthRisk_${healthRisk.healthRiskId}_feedbackMessage`, healthRisk.feedbackMessage, [validators.required]);
    form.addField(`healthRisk_${healthRisk.healthRiskId}_alertRuleCountThreshold`, healthRisk.alertRuleCountThreshold, [validators.required]);
    form.addField(`healthRisk_${healthRisk.healthRiskId}_alertRuleDaysThreshold`, healthRisk.alertRuleDaysThreshold);
    form.addField(`healthRisk_${healthRisk.healthRiskId}_alertRuleKilometersThreshold`, healthRisk.alertRuleKilometersThreshold);

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

  if (!ready) {
    return null;
  }

  return (
    <Fragment key={`healthRisk_${healthRisk.healthRiskId}`}>
      <Grid item xs={12}>
        <Typography variant="h3">{healthRisk.healthRiskCode} - {healthRisk.healthRiskName}</Typography>

        <Grid container spacing={3}>
          <Grid item xs={6}>
            <TextInputField
              label={strings(stringKeys.project.form.caseDefinition)}
              name={`healthRisk_${healthRisk.healthRiskId}_caseDefinition`}
              field={form.fields[`healthRisk_${healthRisk.healthRiskId}_caseDefinition`]}
              multiline
            />
          </Grid>
          <Grid item xs={6}>
            <TextInputField
              label={strings(stringKeys.project.form.feedbackMessage)}
              name={`healthRisk_${healthRisk.healthRiskId}_feedbackMessage`}
              field={form.fields[`healthRisk_${healthRisk.healthRiskId}_feedbackMessage`]}
              multiline
            />
          </Grid>
        </Grid>
      </Grid>

      <Grid item xs={12}>
        <Typography variant="h3">{strings(stringKeys.project.form.alertsSetion)}</Typography>
        <Grid container spacing={3}>
          <Grid item xs={4}>
            <TextInputField
              label={strings(stringKeys.project.form.alertRuleCountThreshold)}
              name={`healthRisk_${healthRisk.healthRiskId}_alertRuleCountThreshold`}
              field={form.fields[`healthRisk_${healthRisk.healthRiskId}_alertRuleCountThreshold`]}
            />
          </Grid>
          <Grid item xs={4}>
            <TextInputField
              label={strings(stringKeys.project.form.alertRuleDaysThreshold)}
              name={`healthRisk_${healthRisk.healthRiskId}_alertRuleDaysThreshold`}
              field={form.fields[`healthRisk_${healthRisk.healthRiskId}_alertRuleDaysThreshold`]}
            />
          </Grid>
          <Grid item xs={4}>
            <TextInputField
              label={strings(stringKeys.project.form.alertRuleKilometersThreshold)}
              name={`healthRisk_${healthRisk.healthRiskId}_alertRuleKilometersThreshold`}
              field={form.fields[`healthRisk_${healthRisk.healthRiskId}_alertRuleKilometersThreshold`]}
            />
          </Grid>
        </Grid>
      </Grid>

      <Grid item xs={12}>
        <Divider />
      </Grid>
    </Fragment>
  );
}
