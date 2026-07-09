import React, { useState, Fragment } from "react";
import { connect } from "react-redux";
import { withLayout } from "../../utils/layout";
import { validators, createForm, useCustomErrors } from "../../utils/forms";
import * as organizationsActions from "./logic/organizationsActions";
import Layout from "../layout/Layout";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import TextInputField from "../forms/TextInputField";
import { Grid } from "@material-ui/core";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { ValidationMessage } from "../forms/ValidationMessage";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import { trackPageView } from "../../utils/appInsightsHelper";

const OrganizationsCreatePageComponent = (props) => {
  const [form] = useState(() => {
    const fields = {
      name: "",
    };

    const validation = {
      name: [
        validators.required,
        validators.minLength(1),
        validators.maxLength(100),
      ],
    };

    return createForm(fields, validation);
  });

  useCustomErrors(form, props.error);

  useMount(() => {
    props.openCreation(props.nationalSocietyId);

    // Track page view
    trackPageView("OrganizationsCreatePage");
  });

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    }

    const values = form.getValues();
    props.create(props.nationalSocietyId, {
      name: values.name,
      nationalSocietyId: parseInt(props.nationalSocietyId),
    });
  };

  return (
    <Fragment>
      {props.error && !props.error.data && (
        <ValidationMessage message={props.error.message} />
      )}

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.common.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>
        </Grid>
        <FormActions>
          <CancelButton onClick={() => props.goToList(props.nationalSocietyId)}>
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

OrganizationsCreatePageComponent.propTypes = {};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isSaving: state.organizations.formSaving,
  error: state.organizations.formError,
});

const mapDispatchToProps = {
  openCreation: organizationsActions.openCreation.invoke,
  create: organizationsActions.create.invoke,
  goToList: organizationsActions.goToList,
};

export const OrganizationsCreatePage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(OrganizationsCreatePageComponent),
);
