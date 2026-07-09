import React, { useEffect, useState, Fragment } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { withLayout } from "../../utils/layout";
import { validators, createForm, useCustomErrors } from "../../utils/forms";
import * as nationalSocietiesActions from "./logic/nationalSocietiesActions";
import Layout from "../layout/Layout";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import TextInputField from "../forms/TextInputField";
import SelectField from "../forms/SelectField";
import { MenuItem, Grid } from "@material-ui/core";
import { Loading } from "../common/loading/Loading";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { ValidationMessage } from "../forms/ValidationMessage";
import { EpiWeekStandards } from "./logic/nationalSocietiesConstants";
import CheckboxField from "../forms/CheckboxField";
import { trackPageView } from "../../utils/appInsightsHelper";

const NationalSocietiesEditPageComponent = (props) => {
  const [form, setForm] = useState(null);

  useMount(() => {
    props.openEdition(props.match);

    // Track page view
    trackPageView("NationalSocietiesEditPage");
  });

  useEffect(() => {
    if (!props.data) {
      return;
    }

    const fields = {
      id: props.data.id,
      name: props.data.name,
      contentLanguageId: props.data.contentLanguageId.toString(),
      countryId: props.data.countryId.toString(),
      epiWeekStartDay: props.data.epiWeekStartDay,
      enableEidsrIntegration: props.data.enableEidsrIntegration,
    };

    const validation = {
      id: [validators.required],
      name: [validators.required, validators.minLength(3)],
      contentLanguageId: [validators.required],
      countryId: [validators.required],
    };

    setForm(createForm(fields, validation));
  }, [props.data, props.match]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    }

    const values = form.getValues();

    props.edit({
      id: props.data.id,
      name: values.name,
      contentLanguageId: parseInt(values.contentLanguageId),
      countryId: parseInt(values.countryId),
      epiWeekStartDay: values.epiWeekStartDay,
      enableEidsrIntegration: values.enableEidsrIntegration,
    });
  };

  useCustomErrors(form, props.error);

  if (props.isFetching || !form) {
    return <Loading />;
  }

  return (
    <Fragment>
      {props.error && !props.error.data && (
        <ValidationMessage message={props.error.message} />
      )}

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.nationalSociety.form.name)}
              name="name"
              field={form.fields.name}
              autoFocus
            />
          </Grid>

          <Grid item xs={12}>
            <SelectField
              label={strings(stringKeys.nationalSociety.form.country)}
              name="country"
              field={form.fields.countryId}
            >
              {props.countries.map((country) => (
                <MenuItem
                  key={`country${country.id}`}
                  value={country.id.toString()}
                >
                  {country.name}
                </MenuItem>
              ))}
            </SelectField>
          </Grid>

          <Grid item xs={12}>
            <SelectField
              label={strings(stringKeys.nationalSociety.form.contentLanguage)}
              name="contentLanguage"
              field={form.fields.contentLanguageId}
            >
              {props.contentLanguages.map((language) => (
                <MenuItem
                  key={`contentLanguage${language.id}`}
                  value={language.id.toString()}
                >
                  {language.name}
                </MenuItem>
              ))}
            </SelectField>
          </Grid>

          <Grid item xs={12}>
            <SelectField
              label={strings(
                stringKeys.nationalSociety.form.epiWeekStandard.title,
              )}
              name="epiWeekStartDay"
              field={form.fields.epiWeekStartDay}
            >
              {EpiWeekStandards.map((itm) => (
                <MenuItem key={`epiWeekStartDay${itm}`} value={itm}>
                  {strings(
                    stringKeys.nationalSociety.form.epiWeekStandard[itm].label,
                  )}
                </MenuItem>
              ))}
            </SelectField>
          </Grid>

          <Grid item xs={12}>
            <CheckboxField
              name="enableEidsrIntegration"
              label={strings(
                stringKeys.nationalSociety.form.enableEidsrIntegration,
              )}
              field={form.fields.enableEidsrIntegration}
            />
          </Grid>
        </Grid>

        <FormActions>
          <CancelButton onClick={() => props.goToOverview(props.data.id)}>
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

NationalSocietiesEditPageComponent.propTypes = {
  getNationalSocieties: PropTypes.func,
  list: PropTypes.array,
};

const mapStateToProps = (state) => ({
  contentLanguages: state.appData.contentLanguages,
  countries: state.appData.countries,
  error: state.nationalSocieties.formError,
  isFetching: state.nationalSocieties.formFetching,
  isSaving: state.nationalSocieties.formSaving,
  data: state.nationalSocieties.formData,
});

const mapDispatchToProps = {
  openEdition: nationalSocietiesActions.openEdition.invoke,
  edit: nationalSocietiesActions.edit.invoke,
  goToOverview: nationalSocietiesActions.goToOverview,
};

export const NationalSocietiesEditPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(NationalSocietiesEditPageComponent),
);
