import React, { useState, Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as healthRisksActions from './logic/healthRisksActions';
import * as appActions from '../app/logic/appActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import Typography from '@material-ui/core/Typography';
import TextInputField from '../forms/TextInputField';
import SnackbarContent from '@material-ui/core/SnackbarContent';
import Button from "@material-ui/core/Button";
import { useMount } from '../../utils/lifecycle';
import SelectField from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import { healthRiskTypes } from './logic/healthRisksConstants';
import Grid from '@material-ui/core/Grid';
import { getSaveFormModel } from './logic/healthRisksService';
import { strings } from '../../strings';

const HealthRisksCreatePageComponent = (props) => {
  const [form] = useState(() => {
    let fields = {
      healthRiskCode: "",
      healthRiskType: "",
      alertRuleCountThreshold: "",
      alertRuleDaysThreshold: "",
      alertRuleMetersThreshold: ""
    };

    let validation = {
      healthRiskCode: [validators.required, validators.integer],
      healthRiskType: [validators.required],
      alertRuleCountThreshold: [validators.integer],
      alertRuleDaysThreshold: [validators.integer],
      alertRuleMetersThreshold: [validators.integer]
    };

    const finalFormData = props.contentLanguages.reduce((result, lang) => ({
      fields: {
        ...result.fields,
        [`contentLanguage_${lang.id}_name`]: "",
        [`contentLanguage_${lang.id}_caseDefinition`]: "",
        [`contentLanguage_${lang.id}_feedbackMessage`]: ""
      },
      validation: {
        ...result.validation,
        [`contentLanguage_${lang.id}_name`]: [validators.required, validators.maxLength(100)],
        [`contentLanguage_${lang.id}_caseDefinition`]: [validators.required, validators.maxLength(500)],
        [`contentLanguage_${lang.id}_feedbackMessage`]: [validators.required, validators.maxLength(160)]
      }
    }), { fields, validation });

    return createForm(finalFormData.fields, finalFormData.validation);
  });

  const [healthRiskTypesData] = useState(healthRiskTypes.map(t => ({
    value: t,
    label: strings(`healthRisk.type.${t.toLowerCase()}`)
  })));

  useMount(() => {
    props.openModule(props.match.path, props.match.params)
  })

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    props.create(getSaveFormModel(form.getValues(), props.contentLanguages));
  };

  return (
    <Fragment>
      <Typography variant="h2">Add health risk/event</Typography>

      {props.error &&
        <SnackbarContent
          message={props.error}
        />
      }

      <Form onSubmit={handleSubmit} fullWidth style={{ maxWidth: 800 }}>
        <Grid container spacing={3}>
          <Grid item xs={3}>
            <TextInputField
              label="Health risk number"
              name="healthRiskCode"
              field={form.fields.healthRiskCode}
            />
          </Grid>
          <Grid item xs={9}>
            <SelectField
              label="Health risk type"
              name="healthRiskType"
              field={form.fields.healthRiskType}
            >
              {healthRiskTypesData.map(({ value, label }) => (
                <MenuItem key={`healthRiskType${value}`} value={value}>{label}</MenuItem>
              ))}
            </SelectField>
          </Grid>

          {props.contentLanguages.map(lang => (
            <Fragment>
              <Grid item xs={12}>
                <Typography variant="h3">{lang.name} translations</Typography>

                <Grid container spacing={3}>
                  <Grid item xs={12}>
                    <TextInputField
                      label={`Health risk/event name`}
                      name={`contentLanguage_${lang.id}_name`}
                      field={form.fields[`contentLanguage_${lang.id}_name`]}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <TextInputField
                      label={`Community case definition`}
                      name={`contentLanguage_${lang.id}_caseDefinition`}
                      field={form.fields[`contentLanguage_${lang.id}_caseDefinition`]}
                      multiline
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <TextInputField
                      label={`Feedback message`}
                      name={`contentLanguage_${lang.id}_feedbackMessage`}
                      field={form.fields[`contentLanguage_${lang.id}_feedbackMessage`]}
                      multiline
                    />
                  </Grid>
                </Grid>
              </Grid>
            </Fragment>
          ))}

          <Grid item xs={12}>
            <Typography variant="h3">Alert rule</Typography>
            <Typography variant="subtitle1">
              An alert is triggered when the number of reports in an area exceeds a limit within a given time. Please specify the alert rule for this health risk.
            </Typography>
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              label="Number of reports"
              name="alertRuleCountThreshold"
              field={form.fields.alertRuleCountThreshold}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              label="Timeframe in days"
              name="alertRuleDaysThreshold"
              field={form.fields.alertRuleDaysThreshold}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              label="Distance in km"
              name="alertRuleMetersThreshold"
              field={form.fields.alertRuleMetersThreshold}
            />
          </Grid>
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList()}>
            Cancel
            </Button>

          <SubmitButton isFetching={props.isSaving}>
            Save health risk/event
            </SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

HealthRisksCreatePageComponent.propTypes = {
  getHealthRisks: PropTypes.func,
  openModule: PropTypes.func,
  list: PropTypes.array
};

const mapStateToProps = state => ({
  contentLanguages: state.appData.contentLanguages,
  error: state.healthRisks.formError,
  isSaving: state.healthRisks.formSaving
});

const mapDispatchToProps = {
  getList: healthRisksActions.getList.invoke,
  create: healthRisksActions.create.invoke,
  goToList: healthRisksActions.goToList,
  openModule: appActions.openModule.invoke
};

export const HealthRisksCreatePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(HealthRisksCreatePageComponent)
);
