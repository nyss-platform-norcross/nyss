import React, { useEffect, useState } from "react";
import { connect } from "react-redux";
import { Grid, Typography, Card, CardContent } from "@material-ui/core";
import { stringKeys, strings } from "../../strings";
import Form from "../forms/form/Form";
import { validators, createForm } from "../../utils/forms";
import Layout from "../layout/Layout";
import { withLayout } from "../../utils/layout";
import FormActions from "../forms/formActions/FormActions";
import { Loading } from "../common/loading/Loading";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import { accessMap } from "../../authentication/accessMap";
import TextInputField from "../forms/TextInputField";
import * as http from "../../utils/http";
import { openErrorMessages } from "./logic/projectsActions";
import styles from "./ProjectErrorMessagesPage.module.scss";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import SettingsTableHeader from "../common/settingsTableHeader/SettingsTableHeader";
import EditIcon from "@material-ui/icons/Edit";
import { trackPageView } from "../../utils/appInsightsHelper";
import { useMount } from "../../utils/lifecycle";
import { ValidationMessage } from "../forms/ValidationMessage";

const MESSAGE_MAX_LEN = 320;
const MESSAGE_WARNING_LEN = 160;

const ProjectErrorMessagesPageComponent = (props) => {
  const [errorMessages, setErrorMessages] = useState([]);
  const [isSaving, setIsSaving] = useState(false);
  const [saveError, setSaveError] = useState(null);

  useMount(() => {
    // Track page view
    trackPageView("ProjectErrorMessagesPage");
  });

  const [form, setForm] = useState(null);

  async function fetchData() {
    props.openErrorMessages(props.projectId);
    setErrorMessages(
      await http.get(`/api/project/${props.projectId}/errorMessages`),
    );
  }

  function edit() {
    const fields = {};
    const validation = {};

    errorMessages.forEach((itm) => {
      fields[itm.key] = itm.message;
      validation[itm.key] = [
        validators.required,
        validators.maxLength(MESSAGE_MAX_LEN),
      ];
    });

    setForm(createForm(fields, validation));
  }

  function cancelEdit() {
    setForm(null);
  }

  async function onSubmit(e) {
    e.preventDefault();
    setSaveError(null);

    if (!form) {
      setSaveError("Form is not initialized. Please try editing again.");
      return;
    }

    if (!form.isValid()) {
      // Scroll to first invalid field
      const firstInvalidField = Object.values(form.fields).find(
        (field) => field.error,
      );
      if (firstInvalidField && firstInvalidField.ref) {
        firstInvalidField.ref.current?.scrollIntoView({
          behavior: "smooth",
          block: "center",
        });
      }
      // Show validation errors
      const invalidFields = Object.values(form.fields).filter(
        (field) => field.error,
      );
      if (invalidFields.length > 0) {
        setSaveError(
          `Please fix ${invalidFields.length} validation error(s) before saving.`,
        );
      }
      return;
    }

    setIsSaving(true);
    setSaveError(null);

    const url = `/api/project/${props.projectId}/errorMessages`;
    const data = {};

    errorMessages.forEach((itm) => {
      data[itm.key] = form.fields[itm.key].value;
    });

    try {
      const updatedMessages = await http.put(url, data);
      setErrorMessages(updatedMessages);
      setForm(null);
      setSaveError(null);
    } catch (error) {
      console.error("Error saving error messages:", error);
      setSaveError(
        error.message || "An error occurred while saving. Please try again.",
      );
    } finally {
      setIsSaving(false);
    }
  }

  useEffect(() => {
    fetchData();
  }, []);

  if (errorMessages.length === 0) {
    return <Loading />;
  }

  return (
    <Grid style={{ maxWidth: 800 }}>
      <SettingsTableHeader>
        <FormActions className={styles.formsActions}>
          {!form && (
            <TableActionsButton
              startIcon={<EditIcon />}
              variant={"contained"}
              onClick={edit}
              roles={accessMap.projectErrorMessages.edit}
            >
              {strings(stringKeys.common.buttons.edit)}
            </TableActionsButton>
          )}
        </FormActions>
      </SettingsTableHeader>
      {saveError && <ValidationMessage message={saveError} />}
      <Form onSubmit={onSubmit} fullWidth>
        <Grid container spacing={4} fixed="true">
          {errorMessages.map((itm) => (
            <Grid item xs={12} key={itm.key}>
              <Card>
                <CardContent>
                  <Typography variant="h3">
                    {strings(`${itm.key}.title`)}
                  </Typography>
                  {!form && (
                    <Typography variant="body1" gutterBottom>
                      {itm.message}
                    </Typography>
                  )}
                  {form && (
                    <>
                      <TextInputField
                        className={styles.input}
                        name={itm.key}
                        field={form.fields[itm.key]}
                        multiline
                      />
                      <InputWarningMessage formField={form.fields[itm.key]} />
                    </>
                  )}
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
        {form && (
          <FormActions className={styles.formsActions}>
            <CancelButton onClick={cancelEdit}>
              {strings(stringKeys.form.cancel)}
            </CancelButton>
            <SubmitButton isFetching={isSaving}>
              {strings(stringKeys.common.buttons.update)}
            </SubmitButton>
          </FormActions>
        )}
      </Form>
    </Grid>
  );
};

const InputWarningMessage = ({ formField }) => {
  const [message, setMessage] = useState("");

  function validate(value) {
    const message =
      value.length > MESSAGE_WARNING_LEN
        ? strings(stringKeys.project.errorMessages.tooLongWarning)
        : "";

    setMessage(message);
  }

  useEffect(() => {
    formField.subscribe(({ newValue }) => {
      validate(newValue);
    });

    validate(formField.value);
  }, []);

  return <p className={styles.inputWarningMsg}>{message}</p>;
};

const mapStateToProps = (_, ownProps) => ({
  projectId: ownProps.match.params.projectId,
});

const mapDispatchToProps = {
  openErrorMessages: openErrorMessages,
};

export const ProjectErrorMessagesPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(ProjectErrorMessagesPageComponent),
);
