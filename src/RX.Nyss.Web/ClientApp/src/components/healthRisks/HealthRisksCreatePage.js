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
import Button from "@material-ui/core/Button";
import { useMount } from '../../utils/lifecycle';
import SelectField from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import { healthRiskTypes } from './logic/healthRisksConstants';
import Grid from '@material-ui/core/Grid';
import { getSaveFormModel } from './logic/healthRisksService';
import { strings, stringKeys } from '../../strings';
import { ValidationMessage } from '../forms/ValidationMessage';

const HealthRisksCreatePageComponent = (props) => {
  const [form] = useState(() => {
    let fields = {
      healthRiskCode: "",
      healthRiskType: "Human",
      alertRuleCountThreshold: "",
      alertRuleDaysThreshold: "",
      alertRuleKilometersThreshold: ""
    };

    let validation = {
      healthRiskCode: [validators.required, validators.integer],
      healthRiskType: [validators.required],
      alertRuleCountThreshold: [validators.integer],
      alertRuleDaysThreshold: [validators.integer],
      alertRuleKilometersThreshold: [validators.integer]
    };

    const finalFormData = props.contentLanguages.reduce((result, lang) => ({
      fields: {
        ...result.fields,
        [`contentLanguage_${lang.id}_name`]: "",
        [`contentLanguage_${lang.id}_caseDefinition`]: "",
        [`contentLanguage_${lang.id}_feedbackMessage`]: ""
      },
      validation: lang.name.toLowerCase() === "english"
        ? {
          ...result.validation,
          [`contentLanguage_${lang.id}_name`]: [validators.required, validators.maxLength(100)],
          [`contentLanguage_${lang.id}_caseDefinition`]: [validators.required, validators.maxLength(500)],
          [`contentLanguage_${lang.id}_feedbackMessage`]: [validators.required, validators.maxLength(160)]
        }
        : result.validation
    }), { fields, validation });

    return createForm(finalFormData.fields, finalFormData.validation);
  });

  const [healthRiskTypesData] = useState(healthRiskTypes.map(t => ({
    value: t,
    label: strings(stringKeys.healthRisk.constants.healthRiskType[t.toLowerCase()])
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
      <Form onSubmit={handleSubmit} fullWidth style={{ maxWidth: 800 }}>
        <Grid container spacing={3}>
          {props.formError && (
            <Grid item xs={12}>
              <ValidationMessage message={props.formError} />
            </Grid>
          )}

          <Grid item xs={3}>
            <TextInputField
              label={strings(stringKeys.healthRisk.form.healthRiskCode)}
              name="healthRiskCode"
              field={form.fields.healthRiskCode}
              autoFocus
            />
          </Grid>
          <Grid item xs={9}>
            <SelectField
              label={strings(stringKeys.healthRisk.form.healthRiskType)}
              name="healthRiskType"
              field={form.fields.healthRiskType}
            >
              {healthRiskTypesData.map(({ value, label }) => (
                <MenuItem key={`healthRiskType${value}`} value={value}>{label}</MenuItem>
              ))}
            </SelectField>
          </Grid>

          {props.contentLanguages.map(lang => (
            <Fragment key={`contentLanguage${lang.id}`}>
              <Grid item xs={12}>
                <Typography variant="h3">{strings(stringKeys.healthRisk.form.translationsSetion, true).replace("{language}", lang.name)}</Typography>

                <Grid container spacing={3}>
                  <Grid item xs={12}>
                    <TextInputField
                      label={strings(stringKeys.healthRisk.form.contentLanguageName)}
                      name={`contentLanguage_${lang.id}_name`}
                      field={form.fields[`contentLanguage_${lang.id}_name`]}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <TextInputField
                      label={strings(stringKeys.healthRisk.form.contentLanguageCaseDefinition)}
                      name={`contentLanguage_${lang.id}_caseDefinition`}
                      field={form.fields[`contentLanguage_${lang.id}_caseDefinition`]}
                      multiline
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <TextInputField
                      label={strings(stringKeys.healthRisk.form.contentLanguageFeedbackMessage)}
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
            <Typography variant="h3">{strings(stringKeys.healthRisk.form.alertsSetion)}</Typography>
            <Typography variant="subtitle1">{strings(stringKeys.healthRisk.form.alertRuleDescription)}</Typography>
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              label={strings(stringKeys.healthRisk.form.alertRuleCountThreshold)}
              name="alertRuleCountThreshold"
              field={form.fields.alertRuleCountThreshold}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              label={strings(stringKeys.healthRisk.form.alertRuleDaysThreshold)}
              name="alertRuleDaysThreshold"
              field={form.fields.alertRuleDaysThreshold}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              label={strings(stringKeys.healthRisk.form.alertRuleKilometersThreshold)}
              name="alertRuleKilometersThreshold"
              field={form.fields.alertRuleKilometersThreshold}
            />
          </Grid>
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList()}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.healthRisk.form.create)}</SubmitButton>
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
  formError: state.healthRisks.formError,
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
