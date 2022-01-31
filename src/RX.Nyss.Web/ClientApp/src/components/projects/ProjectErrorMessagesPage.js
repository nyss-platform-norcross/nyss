import React, { useEffect, useState } from "react";
import { connect } from "react-redux";
import { Grid, Typography, Card, CardContent, Button } from "@material-ui/core";
import { stringKeys, strings } from "../../strings";
import Form from "../forms/form/Form";
import { validators, createForm } from "../../utils/forms";
import Layout from "../layout/Layout";
import { withLayout } from "../../utils/layout";
import FormActions from "../forms/formActions/FormActions";
import { Loading } from "../common/loading/Loading";
import { TableActionsButton } from "../common/tableActions/TableActionsButton";
import SubmitButton from "../forms/submitButton/SubmitButton";
import { accessMap } from "../../authentication/accessMap";
import TextInputField from "../forms/TextInputField";
import * as http from "../../utils/http";
import { openErrorMessages } from "./logic/projectsActions";
import styles from "./ProjectErrorMessagesPage.module.scss";
import CancelButton from "../forms/cancelButton/CancelButton";

const MESSAGE_MAX_LEN = 320;
const MESSAGE_WARNING_LEN = 160;

const ProjectErrorMessagesPageComponent = (props) => {
  const [errorMessages, setErrorMessages] = useState([]);
  const [isSaving, setIsSaving] = useState(false);
  const [form, setForm] = useState(null);

  async function fetchData() {
    props.openErrorMessages(props.projectId);
    setErrorMessages(await http.get(`/api/project/${props.projectId}/errorMessages`));
  }

  function edit() {
    const fields = {};
    const validation = {};

    errorMessages.forEach((itm) => {
      fields[itm.key] = itm.message;
      validation[itm.key] = [validators.required, validators.maxLength(MESSAGE_MAX_LEN)];
    });

    setForm(createForm(fields, validation));
  }

  function cancelEdit() {
    setForm(null);
  }

  async function onSubmit(e) {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    }

    const url = `/api/project/${props.projectId}/errorMessages`;
    const data = {};

    errorMessages.forEach((itm) => {
      data[itm.key] = form.fields[itm.key].value;
    });

    try {
      setErrorMessages(await http.put(url, data));
      setForm(null);
    } catch (error) {
      console.error(error);
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
    <Form onSubmit={onSubmit} fullWidth>
      <Grid container spacing={4} fixed="true" style={{ maxWidth: 800 }}>
        {errorMessages.map(itm => (
          <Grid item xs={12} key={itm.key}>
            <Card>
              <CardContent>
                <Typography variant="h3">{strings(`${itm.key}.title`)}</Typography>
                {!form && (
                  <Typography variant="body1" gutterBottom>
                    {itm.message}
                  </Typography>
                )}
                {form && (<>
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
        <Grid item xs={12}>
          <FormActions className={styles.formsActions}>
            {form && (
              <>
                <CancelButton variant={"contained"} onClick={cancelEdit}>
                  {strings(stringKeys.form.cancel)}
                </CancelButton>
                <SubmitButton isFetching={isSaving}>
                  {strings(stringKeys.project.form.update)}
                </SubmitButton>
              </>
            )}
            {!form && (
              <TableActionsButton
                variant={"outlined"}
                color={"primary"}
                onClick={edit}
                roles={accessMap.projectErrorMessages.edit}
              >
                {strings(stringKeys.project.edit)}
              </TableActionsButton>
            )}
          </FormActions>
        </Grid>
      </Grid>
    </Form>
  );
};

const InputWarningMessage = ({ formField }) => {
  const [message, setMessage] = useState("");

  function validate(value) {
    const message = value.length > MESSAGE_WARNING_LEN
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

  return (
    <p className={styles.inputWarningMsg}>{message}</p>
  )
}

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
    mapDispatchToProps
  )(ProjectErrorMessagesPageComponent)
);
