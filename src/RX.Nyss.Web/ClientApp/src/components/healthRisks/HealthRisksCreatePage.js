import React, { useState, Fragment, useEffect } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { withLayout } from '../../utils/layout';
import { validators, createForm, useCustomErrors } from '../../utils/forms';
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
import { strings, stringKeys, stringsFormat } from '../../strings';
import { ValidationMessage } from '../forms/ValidationMessage';

const HealthRisksCreatePageComponent = (props) => {
  const [reportCountThreshold, setReportCountThreshold] = useState(0);
  const [form] = useState(() => {
    let fields = {
      healthRiskCode: "",
      healthRiskType: "Human",
      alertRuleCountThreshold: "",
      alertRuleDaysThreshold: "",
      alertRuleKilometersThreshold: ""
    };

    let validation = {
      healthRiskCode: [validators.required, validators.nonNegativeNumber],
      healthRiskType: [validators.required],
      alertRuleCountThreshold: [validators.nonNegativeNumber],
      alertRuleDaysThreshold: [
        validators.requiredWhen(f => f.alertRuleCountThreshold > 1),
        validators.inRange(1, 365)
      ],
      alertRuleKilometersThreshold: [
        validators.requiredWhen(f => f.alertRuleCountThreshold > 1),
        validators.inRange(1, 9999)
      ]
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

    const newForm = createForm(finalFormData.fields, finalFormData.validation);
    newForm.fields.alertRuleCountThreshold.subscribe(({ newValue }) => setReportCountThreshold(newValue));
    return newForm;
  });

  useEffect(() => {
    if (form && reportCountThreshold <= 1) {
      form.fields.alertRuleDaysThreshold.update("");
      form.fields.alertRuleKilometersThreshold.update("");
    }
    return;
  }, [form, reportCountThreshold])

  const [healthRiskTypesData] = useState(healthRiskTypes.map(t => ({
    value: t,
    label: strings(stringKeys.healthRisk.constants.healthRiskType[t.toLowerCase()])
  })));

  useMount(() => {
    props.openModule(props.match.path, props.match.params)
  })

  useCustomErrors(form, props.formError);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    props.create(getSaveFormModel(form.getValues(), props.contentLanguages));
  };

  return (
    <Fragment>
      {props.formError && <ValidationMessage message={props.formError.message} />}

      <Form onSubmit={handleSubmit} fullWidth style={{ maxWidth: 800 }}>
        <Grid container spacing={2}>

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
                <Typography variant="h3">{stringsFormat(stringKeys.healthRisk.form.translationsSetion, { language: lang.name })}</Typography>

                <Grid container spacing={2}>
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
              disabled={!reportCountThreshold || reportCountThreshold <= 1}
            />
          </Grid>

          <Grid item xs={4}>
            <TextInputField
              label={strings(stringKeys.healthRisk.form.alertRuleKilometersThreshold)}
              name="alertRuleKilometersThreshold"
              field={form.fields.alertRuleKilometersThreshold}
              disabled={!reportCountThreshold || reportCountThreshold <= 1}
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

export const HealthRisksCreatePage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(HealthRisksCreatePageComponent)
);
