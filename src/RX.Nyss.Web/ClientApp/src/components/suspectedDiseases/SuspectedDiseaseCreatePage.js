import React, { useState, Fragment } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { withLayout } from "../../utils/layout";
import { validators, createForm, useCustomErrors } from "../../utils/forms";
import * as suspectedDiseaseActions from "./logic/suspectedDiseaseActions";
import * as appActions from "../app/logic/appActions";
import Layout from "../layout/Layout";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import { Typography, Grid } from "@material-ui/core";
import TextInputField from "../forms/TextInputField";
import { useMount } from "../../utils/lifecycle";
import { getSaveFormModel } from "./logic/suspectedDiseaseService";
import { strings, stringKeys, stringsFormat } from "../../strings";
import { ValidationMessage } from "../forms/ValidationMessage";
import { trackPageView } from "../../utils/appInsightsHelper";

const SuspectedDiseaseCreatePageComponent = (props) => {
  const [form] = useState(() => {
    let fields = {
      suspectedDiseaseCode: "",
    };

    let validation = {
      suspectedDiseaseCode: [validators.required, validators.nonNegativeNumber],
    };

    const finalFormData = props.contentLanguages.reduce(
      (result, lang) => ({
        fields: {
          ...result.fields,
          [`contentLanguage_${lang.id}_name`]: "",
        },
        validation: {
          ...result.validation,
          [`contentLanguage_${lang.id}_name`]: [
            validators.required,
            validators.maxLength(100),
          ],
        },
      }),
      { fields, validation },
    );

    const newForm = createForm(finalFormData.fields, finalFormData.validation);
    return newForm;
  });

  useMount(() => {
    props.openModule(props.match.path, props.match.params);

    // Track page view
    trackPageView("SuspectedDiseaseCreatePage");
  });

  useCustomErrors(form, props.formError);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    }

    props.create(getSaveFormModel(form.getValues(), props.contentLanguages));
  };

  return (
    <Fragment>
      {props.formError && (
        <ValidationMessage message={props.formError.message} />
      )}

      <Form onSubmit={handleSubmit} fullWidth style={{ maxWidth: 800 }}>
        <Grid item xs={3}>
          <TextInputField
            label={strings(
              stringKeys.suspectedDisease.form.suspectedDiseaseCode,
            )}
            name="suspectedDiseaseCode"
            field={form.fields.suspectedDiseaseCode}
            autoFocus
          />
        </Grid>

        {props.contentLanguages.map((lang) => (
          <Fragment key={`contentLanguage${lang.id}`}>
            <Grid item xs={12}>
              <Typography variant="h3">
                {stringsFormat(stringKeys.healthRisk.form.translationsSection, {
                  language: lang.name,
                })}
              </Typography>

              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <TextInputField
                    label={strings(
                      stringKeys.suspectedDisease.form.contentLanguageName,
                    )}
                    name={`contentLanguage_${lang.id}_name`}
                    field={form.fields[`contentLanguage_${lang.id}_name`]}
                  />
                </Grid>
              </Grid>
            </Grid>
          </Fragment>
        ))}

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

SuspectedDiseaseCreatePageComponent.propTypes = {
  getSuspectedDisease: PropTypes.func,
  openModule: PropTypes.func,
  list: PropTypes.array,
};

const mapStateToProps = (state) => ({
  contentLanguages: state.appData.contentLanguages,
  formError: state.suspectedDiseases.formError,
  isSaving: state.suspectedDiseases.formSaving,
});

const mapDispatchToProps = {
  getList: suspectedDiseaseActions.getList.invoke,
  create: suspectedDiseaseActions.create.invoke,
  goToList: suspectedDiseaseActions.goToList,
  openModule: appActions.openModule.invoke,
};

export const SuspectedDiseaseCreatePage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(SuspectedDiseaseCreatePageComponent),
);
