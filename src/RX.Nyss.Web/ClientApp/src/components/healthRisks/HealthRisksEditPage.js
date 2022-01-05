import React, { useEffect, useState, Fragment } from 'react';
import { connect } from "react-redux";
import { withLayout } from '../../utils/layout';
import { validators, createForm, useCustomErrors } from '../../utils/forms';
import * as healthRisksActions from './logic/healthRisksActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import { Button, Typography, Grid, MenuItem } from "@material-ui/core";
import { Loading } from '../common/loading/Loading';
import { useMount } from '../../utils/lifecycle';
import SelectField from '../forms/SelectField';
import { healthRiskTypes } from './logic/healthRisksConstants';
import { getSaveFormModel } from './logic/healthRisksService';
import { strings, stringKeys, stringsFormat } from '../../strings';
import { ValidationMessage } from '../forms/ValidationMessage';

const HealthRisksEditPageComponent = (props) => {
  const [form, setForm] = useState(null);
  const [selectedHealthRiskType, setHealthRiskType] = useState(null);
  const [reportCountThreshold, setReportCountThreshold] = useState(null);

  useMount(() => {
    props.openEdition(props.match);
  });

  useEffect(() => {
    if (form && reportCountThreshold <= 1) {
      form.fields.alertRuleDaysThreshold.update("");
      form.fields.alertRuleKilometersThreshold.update("");
    }
    return;
  }, [form, reportCountThreshold])

  useEffect(() => {
    if (!props.data) {
      return;
    }

    setReportCountThreshold(props.data.alertRuleCountThreshold);
    setHealthRiskType(props.data.healthRiskType);

    let fields = {
      healthRiskCode: props.data.healthRiskCode.toString(),
      healthRiskType: props.data.healthRiskType,
      alertRuleCountThreshold: props.data.alertRuleCountThreshold,
      alertRuleDaysThreshold: props.data.alertRuleDaysThreshold,
      alertRuleKilometersThreshold: props.data.alertRuleKilometersThreshold
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

    const finalFormData = props.contentLanguages
      .map(lang => ({ lang, content: props.data.languageContent.find(lc => lc.languageId === lang.id) }))
      .reduce((result, { lang, content }) => ({
        fields: {
          ...result.fields,
          [`contentLanguage_${lang.id}_name`]: content && content.name,
          [`contentLanguage_${lang.id}_caseDefinition`]: content && content.caseDefinition,
          [`contentLanguage_${lang.id}_feedbackMessage`]: content && content.feedbackMessage
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
    newForm.fields.healthRiskType.subscribe(({ newValue }) => setHealthRiskType(newValue));
    setForm(newForm);

  }, [props.data, props.contentLanguages]);

  const [healthRiskTypesData] = useState(healthRiskTypes.map(t => ({
    value: t,
    label: strings(stringKeys.healthRisk.constants.healthRiskType[t.toLowerCase()])
  })));

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    props.edit(props.data.id, getSaveFormModel(form.getValues(), props.contentLanguages));
  };

  useCustomErrors(form, props.formError);

  if (props.isFetching || !form) {
    return <Loading />;
  }

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
                <Typography variant="h3">{stringsFormat(stringKeys.healthRisk.form.translationsSection, { language: lang.name })}</Typography>

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
                      rows={4}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <TextInputField
                      label={strings(stringKeys.healthRisk.form.contentLanguageFeedbackMessage)}
                      name={`contentLanguage_${lang.id}_feedbackMessage`}
                      field={form.fields[`contentLanguage_${lang.id}_feedbackMessage`]}
                      multiline
                      rows={4}
                    />
                  </Grid>
                </Grid>
              </Grid>
            </Fragment>
          ))}

          {selectedHealthRiskType === "Activity" && (
            <Fragment>
              <Grid item xs={12}>
                <Typography variant="h3">{strings(stringKeys.healthRisk.form.alertsSection)}</Typography>
                <Typography variant="body1"
                  style={{ color: "#a0a0a0" }}>{strings(stringKeys.healthRisk.form.noAlertRule)}
                </Typography>
              </Grid>
            </Fragment>
          )}

          {selectedHealthRiskType !== "Activity" && (
            <Fragment>
              <Grid item xs={12}>
                <Typography variant="h3">{strings(stringKeys.healthRisk.form.alertsSection)}</Typography>
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
            </Fragment>
          )}
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList()}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.healthRisk.form.update)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

HealthRisksEditPageComponent.propTypes = {
};

const mapStateToProps = state => ({
  contentLanguages: state.appData.contentLanguages,
  isFetching: state.healthRisks.formFetching,
  isSaving: state.healthRisks.formSaving,
  formError: state.healthRisks.formError,
  data: state.healthRisks.formData
});

const mapDispatchToProps = {
  openEdition: healthRisksActions.openEdition.invoke,
  goToList: healthRisksActions.goToList,
  edit: healthRisksActions.edit.invoke
};

export const HealthRisksEditPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(HealthRisksEditPageComponent)
);
