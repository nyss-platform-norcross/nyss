import React, { useEffect, useState, Fragment } from "react";
import { connect } from "react-redux";
import { withLayout } from "../../utils/layout";
import { validators, createForm, useCustomErrors } from "../../utils/forms";
import * as suspectedDiseaseActions from "./logic/suspectedDiseaseActions";
import Layout from "../layout/Layout";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import TextInputField from "../forms/TextInputField";
import { Typography, Grid } from "@material-ui/core";
import { Loading } from "../common/loading/Loading";
import { useMount } from "../../utils/lifecycle";
import { getSaveFormModel } from "./logic/suspectedDiseaseService";
import { strings, stringKeys, stringsFormat } from "../../strings";
import { ValidationMessage } from "../forms/ValidationMessage";
import { trackPageView } from "../../utils/appInsightsHelper";

const SuspectedDiseaseEditPageComponent = (props) => {
  const [form, setForm] = useState(null);

  useMount(() => {
    props.openEdition(props.match);

    // Track page view
    trackPageView("SuspectedDiseaseEditPage");
  });

  useEffect(() => {
    if (!props.data) {
      return;
    }

    let fields = {
      suspectedDiseaseId: props.data.suspectedDiseaseId.toString(),
      suspectedDiseaseCode: props.data.suspectedDiseaseCode.toString(),
    };

    let validation = {
      suspectedDiseaseId: [validators.required, validators.nonNegativeNumber],
      suspectedDiseaseCode: [validators.required, validators.nonNegativeNumber],
    };

    const finalFormData = props.contentLanguages
      .map((lang) => ({
        lang,
        content: props.data.languageContent.find(
          (lc) => lc.languageId === lang.id,
        ),
      }))
      .reduce(
        (result, { lang, content }) => ({
          fields: {
            ...result.fields,
            [`contentLanguage_${lang.id}_name`]: content && content.name,
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
    setForm(newForm);
  }, [props.data, props.contentLanguages]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    }

    props.edit(
      props.data.suspectedDiseaseId,
      getSaveFormModel(form.getValues(), props.contentLanguages),
    );
  };

  useCustomErrors(form, props.formError);

  if (props.isFetching || !form) {
    return <Loading />;
  }

  return (
    <Fragment>
      {props.formError && (
        <ValidationMessage message={props.formError.message} />
      )}

      <Form onSubmit={handleSubmit} fullWidth style={{ maxWidth: 800 }}>
        <Grid container spacing={2}>
          <Grid item xs={3}>
            <TextInputField
              label={strings(
                stringKeys.suspectedDisease.form.suspectedDiseaseCode,
              )}
              name="suspectedDiseaseCode"
              field={form.fields.suspectedDiseaseCode}
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
        </Grid>

        <FormActions>
          <CancelButton onClick={() => props.goToList()}>
            {strings(stringKeys.form.cancel)}
          </CancelButton>
          <SubmitButton isFetching={props.isSaving}>
            {strings(stringKeys.common.buttons.update)}
          </SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
};

SuspectedDiseaseEditPageComponent.propTypes = {};

const mapStateToProps = (state) => ({
  contentLanguages: state.appData.contentLanguages,
  isFetching: state.suspectedDiseases.formFetching,
  isSaving: state.suspectedDiseases.formSaving,
  formError: state.suspectedDiseases.formError,
  data: state.suspectedDiseases.formData,
});

const mapDispatchToProps = {
  openEdition: suspectedDiseaseActions.openEdition.invoke,
  goToList: suspectedDiseaseActions.goToList,
  edit: suspectedDiseaseActions.edit.invoke,
};

export const SuspectedDiseaseEditPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(SuspectedDiseaseEditPageComponent),
);
