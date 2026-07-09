import React, { useState, Fragment, useEffect } from "react";
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import { withLayout } from "../../utils/layout";
import { validators, createForm, useCustomErrors } from "../../utils/forms";
import * as healthRisksActions from "./logic/healthRisksActions";
import * as appActions from "../app/logic/appActions";
import Layout from "../layout/Layout";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import { Typography, MenuItem, Grid } from "@material-ui/core";
import TextInputField from "../forms/TextInputField";
import { useMount } from "../../utils/lifecycle";
import SelectField from "../forms/SelectField";
import { healthRiskTypes } from "./logic/healthRisksConstants";
import { getSaveFormModel } from "./logic/healthRisksService";
import { strings, stringKeys, stringsFormat } from "../../strings";
import { ValidationMessage } from "../forms/ValidationMessage";
import { MultiSelect } from "../forms/MultiSelect";
import { trackPageView } from "../../utils/appInsightsHelper";

const HealthRisksCreatePageComponent = (props) => {
  const [suspectedDiseasesDataSource, setSuspectedDiseasesDataSource] =
    useState([]);
  const [selectedSuspectedDiseases, setSelectedSuspectedDiseases] = useState(
    [],
  );
  const useRtlDirection = useSelector(
    (state) => state.appData.user.languageCode === "ar",
  );

  const [reportCountThreshold, setReportCountThreshold] = useState(0);
  const [selectedHealthRiskType, setHealthRiskType] = useState(null);
  const [form] = useState(() => {
    let fields = {
      healthRiskCode: "",
      healthRiskType: "Human",
      alertRuleCountThreshold: "",
      alertRuleDaysThreshold: "",
      alertRuleKilometersThreshold: "",
    };

    let validation = {
      healthRiskCode: [validators.required, validators.nonNegativeNumber],
      healthRiskType: [validators.required],
      alertRuleCountThreshold: [validators.nonNegativeNumber],
      alertRuleDaysThreshold: [
        validators.requiredWhen((f) => f.alertRuleCountThreshold > 1),
        validators.inRange(1, 365),
      ],
      alertRuleKilometersThreshold: [
        validators.requiredWhen((f) => f.alertRuleCountThreshold > 1),
        validators.inRange(1, 9999),
      ],
    };

    const finalFormData = props.contentLanguages.reduce(
      (result, lang) => ({
        fields: {
          ...result.fields,
          [`contentLanguage_${lang.id}_name`]: "",
          [`contentLanguage_${lang.id}_caseDefinition`]: "",
          [`contentLanguage_${lang.id}_feedbackMessage`]: "",
        },
        validation: {
          ...result.validation,
          [`contentLanguage_${lang.id}_name`]: [
            validators.required,
            validators.maxLength(100),
          ],
          [`contentLanguage_${lang.id}_caseDefinition`]: [
            validators.required,
            validators.maxLength(500),
          ],
          [`contentLanguage_${lang.id}_feedbackMessage`]: [
            validators.required,
            validators.maxLength(160),
          ],
        },
      }),
      { fields, validation },
    );

    const newForm = createForm(finalFormData.fields, finalFormData.validation);
    newForm.fields.alertRuleCountThreshold.subscribe(({ newValue }) =>
      setReportCountThreshold(newValue),
    );
    newForm.fields.healthRiskType.subscribe(({ newValue }) =>
      setHealthRiskType(newValue),
    );
    return newForm;
  });

  useEffect(() => {
    props.data &&
      setSuspectedDiseasesDataSource(
        props.data.map((sd) => ({
          value: sd.suspectedDiseaseId,
          label: sd.suspectedDiseaseName,
          data: sd,
        })),
      );
    if (form && reportCountThreshold <= 1) {
      form.fields.alertRuleDaysThreshold.update("");
      form.fields.alertRuleKilometersThreshold.update("");
    }
    return;
  }, [form, reportCountThreshold, props.data]);

  const [healthRiskTypesData] = useState(
    healthRiskTypes.map((t) => ({
      value: t,
      label: strings(
        stringKeys.healthRisk.constants.healthRiskType[t.toLowerCase()],
      ),
    })),
  );

  useMount(() => {
    props.openModule(props.match.path, props.match.params);
    ///Added to handle suspected disease list
    props.openCreation();

    // Track page view
    trackPageView("HealthRisksCreatePage");
  });

  useCustomErrors(form, props.formError);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    }

    console.log("selected SD");
    console.log(selectedSuspectedDiseases);

    props.create(
      getSaveFormModel(
        form.getValues(),
        props.contentLanguages,
        selectedSuspectedDiseases,
      ),
    );
  };

  //Handle add or remove suspected disease values in list
  const onSuspectedDiseaseChange = (value, eventData) => {
    if (eventData.action === "select-option") {
      setSelectedSuspectedDiseases([
        ...selectedSuspectedDiseases,
        eventData.option.data,
      ]);
    } else if (
      eventData.action === "remove-value" ||
      eventData.action === "pop-value"
    ) {
      setSelectedSuspectedDiseases(
        selectedSuspectedDiseases.filter(
          (sd) => sd.suspectedDiseaseId !== eventData.removedValue.value,
        ),
      );
    } else if (eventData.action === "clear") {
      suspectedDiseasesDataSource.sort();
    }
  };

  const getSelectedSuspectedDiseaseValue = () =>
    suspectedDiseasesDataSource.filter((sd) =>
      selectedSuspectedDiseases.some(
        (ssd) => ssd.suspectedDiseaseId === sd.value,
      ),
    );

  return (
    <Fragment>
      {props.formError && (
        <ValidationMessage message={props.formError.message} />
      )}

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
                <MenuItem key={`healthRiskType${value}`} value={value}>
                  {label}
                </MenuItem>
              ))}
            </SelectField>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h3">
              {strings(stringKeys.healthRisk.form.suspectedDiseaseTitle)}
            </Typography>
            <MultiSelect
              label={strings(stringKeys.healthRisk.form.suspectedDiseaseList)}
              options={suspectedDiseasesDataSource}
              onChange={onSuspectedDiseaseChange}
              value={getSelectedSuspectedDiseaseValue()}
              rtl={useRtlDirection}
            />
          </Grid>

          {props.contentLanguages.map((lang) => (
            <Fragment key={`contentLanguage${lang.id}`}>
              <Grid item xs={12}>
                <Typography variant="h3">
                  {stringsFormat(
                    stringKeys.healthRisk.form.translationsSection,
                    { language: lang.name },
                  )}
                </Typography>

                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <TextInputField
                      label={strings(
                        stringKeys.healthRisk.form.contentLanguageName,
                      )}
                      name={`contentLanguage_${lang.id}_name`}
                      field={form.fields[`contentLanguage_${lang.id}_name`]}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <TextInputField
                      label={strings(
                        stringKeys.healthRisk.form
                          .contentLanguageCaseDefinition,
                      )}
                      name={`contentLanguage_${lang.id}_caseDefinition`}
                      field={
                        form.fields[`contentLanguage_${lang.id}_caseDefinition`]
                      }
                      multiline
                      rows={4}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <TextInputField
                      label={strings(
                        stringKeys.healthRisk.form
                          .contentLanguageFeedbackMessage,
                      )}
                      name={`contentLanguage_${lang.id}_feedbackMessage`}
                      field={
                        form.fields[
                          `contentLanguage_${lang.id}_feedbackMessage`
                        ]
                      }
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
                <Typography variant="h3">
                  {strings(stringKeys.healthRisk.form.alertsSection)}
                </Typography>
                <Typography variant="body1" style={{ color: "#a0a0a0" }}>
                  {strings(stringKeys.healthRisk.form.noAlertRule)}
                </Typography>
              </Grid>
            </Fragment>
          )}

          {selectedHealthRiskType !== "Activity" && (
            <Fragment>
              <Grid item xs={12}>
                <Typography variant="h3">
                  {strings(stringKeys.healthRisk.form.alertsSection)}
                </Typography>
                <Typography variant="subtitle1">
                  {strings(stringKeys.healthRisk.form.alertRuleDescription)}
                </Typography>
              </Grid>

              <Grid item xs={4}>
                <TextInputField
                  label={strings(
                    stringKeys.healthRisk.form.alertRuleCountThreshold,
                  )}
                  name="alertRuleCountThreshold"
                  field={form.fields.alertRuleCountThreshold}
                />
              </Grid>

              <Grid item xs={4}>
                <TextInputField
                  label={strings(
                    stringKeys.healthRisk.form.alertRuleDaysThreshold,
                  )}
                  name="alertRuleDaysThreshold"
                  field={form.fields.alertRuleDaysThreshold}
                  disabled={!reportCountThreshold || reportCountThreshold <= 1}
                />
              </Grid>

              <Grid item xs={4}>
                <TextInputField
                  label={strings(
                    stringKeys.healthRisk.form.alertRuleKilometersThreshold,
                  )}
                  name="alertRuleKilometersThreshold"
                  field={form.fields.alertRuleKilometersThreshold}
                  disabled={!reportCountThreshold || reportCountThreshold <= 1}
                />
              </Grid>
            </Fragment>
          )}
        </Grid>

        <FormActions>
          <CancelButton onClick={() => props.goToList()}>
            {strings(stringKeys.form.cancel)}
          </CancelButton>
          <SubmitButton isFetching={props.isSaving}>
            {strings(stringKeys.common.buttons.add)}
          </SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
};

HealthRisksCreatePageComponent.propTypes = {
  getHealthRisks: PropTypes.func,
  openModule: PropTypes.func,
  openCreation: PropTypes.func, //Added
  data: PropTypes.array,
};

const mapStateToProps = (state) => ({
  data: state.healthRisks.formData,
  contentLanguages: state.appData.contentLanguages,
  formError: state.healthRisks.formError,
  isSaving: state.healthRisks.formSaving,
});

const mapDispatchToProps = {
  getList: healthRisksActions.getList.invoke,
  create: healthRisksActions.create.invoke,
  goToList: healthRisksActions.goToList,
  openCreation: healthRisksActions.openCreation.invoke, //Added
  openModule: appActions.openModule.invoke,
};

export const HealthRisksCreatePage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(HealthRisksCreatePageComponent),
);
