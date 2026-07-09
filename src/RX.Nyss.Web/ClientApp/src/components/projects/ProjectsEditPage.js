import React, { useState, Fragment, useEffect, createRef } from "react";
import { connect } from "react-redux";
import { withLayout } from "../../utils/layout";
import { validators, createForm, useCustomErrors } from "../../utils/forms";
import * as projectsActions from "./logic/projectsActions";
import Layout from "../layout/Layout";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import TextInputField from "../forms/TextInputField";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { Grid } from "@material-ui/core";
import { getSaveFormModel } from "./logic/projectsService";
import { Loading } from "../common/loading/Loading";
import { ValidationMessage } from "../forms/ValidationMessage";
import CheckboxField from "../forms/CheckboxField";
import { SubMenuTitle } from "../layout/SubMenuTitle";
import { trackPageView } from "../../utils/appInsightsHelper";

const ProjectsEditPageComponent = (props) => {
  useMount(() => {
    props.openEdition(props.nationalSocietyId, props.projectId);

    // Track page view
    trackPageView("ProjectsEditPage");
  });

  const [form, setForm] = useState(null);

  useEffect(() => {
    if (!props.data) {
      return;
    }

    let fields = {
      name: props.data.name,
      allowMultipleOrganizations: props.data.allowMultipleOrganizations,
    };

    let validation = {
      name: [
        validators.required,
        validators.minLength(1),
        validators.maxLength(100),
      ],
    };

    const refs = {
      name: createRef(),
    };

    setForm(createForm(fields, validation, refs));
    return () => setForm(null);
  }, [props.data]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      Object.values(form.fields)
        .filter((e) => e.error)[0]
        .scrollTo();
      return;
    }

    props.edit(
      props.nationalSocietyId,
      props.projectId,
      getSaveFormModel(form.getValues()),
    );
  };

  useCustomErrors(form, props.error);

  if (props.isFetching || !form) {
    return <Loading />;
  }

  return (
    <Fragment>
      <SubMenuTitle />
      {props.error && <ValidationMessage message={props.error.message} />}

      <Form onSubmit={handleSubmit} fullWidth style={{ maxWidth: 800 }}>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.project.form.name)}
              name="name"
              field={form.fields.name}
              fieldRef={form.fields.name.ref}
            />
          </Grid>

          <Grid item xs={12}>
            <CheckboxField
              label={strings(
                stringKeys.project.form.allowMultipleOrganizations,
              )}
              name="allowMultipleOrganizations"
              field={form.fields.allowMultipleOrganizations}
              color="primary"
            />
          </Grid>
        </Grid>

        <FormActions>
          <CancelButton
            onClick={() =>
              props.goToOverview(props.nationalSocietyId, props.projectId)
            }
          >
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

ProjectsEditPageComponent.propTypes = {};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isFetching: state.projects.formFetching,
  isSaving: state.projects.formSaving,
  data: state.projects.formData,
  error: state.projects.formError,
});

const mapDispatchToProps = {
  openEdition: projectsActions.openEdition.invoke,
  edit: projectsActions.edit.invoke,
  goToOverview: projectsActions.goToOverview,
};

export const ProjectsEditPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsEditPageComponent),
);
