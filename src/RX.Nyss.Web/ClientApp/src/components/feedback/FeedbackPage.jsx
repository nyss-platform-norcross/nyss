import React, { useState } from "react";
import { useSelector } from "react-redux";
import { useMount } from "../../utils/lifecycle";
import * as appActions from "../app/logic/appActions";
import { withLayout } from "../../utils/layout";
import { Layout } from "../layout/Layout";
import { connect } from "react-redux";
import { Grid, Typography } from "@material-ui/core";
import { strings, stringKeys } from "../../strings";
import { createForm, validators } from "../../utils/forms";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import TextInputField from "../forms/TextInputField";
import { makeStyles } from "@material-ui/core/styles";

const useStyles = makeStyles({
  input: {
    height: 200,
  },
  label: {
    textAlign: "center",
    position: "relative",
  },
  title: {
    color: "black",
    fontSize: "1.5rem",
    textAlign: "center",
  },
});

const Feedback = ({ openModule, match }) => {
  const isSendingFeedback = useSelector(
    (state) => state.appData.feedback.isSending
  );

  const classes = useStyles();
  const [form, setForm] = useState(() => {
    const fields = { message: "" };
    const validation = {
      message: [validators.maxLength(1000), validators.required],
    };
    return createForm(fields, validation);
  });

  useMount(() => {
    openModule(match.path, match.params);
  });

  return (
    <Grid container justifyContent="center">
      <Grid container style={{ width: 600 }} spacing={3}>
        <Grid item xs={12}>
          <Typography
            variant={"h1"}
            className={classes.title}
          >
            {strings(stringKeys.feedback.title)}
          </Typography>
          <TextInputField
            label={strings(stringKeys.feedback.dialogDescription)}
            name="feedback"
            multiline
            rows="4"
            disabled={isSendingFeedback}
            field={form.fields.message}
            classNameInput={classes.input}
            classNameLabel={classes.label}
            placeholder={strings(stringKeys.feedback.placeholder)}
          />
        </Grid>
        <Grid item xs={12} style={{ textAlign: "right" }}>
          <SubmitButton isFetching={isSendingFeedback} onClick={() => {}}>
            {strings(stringKeys.feedback.submit)}
          </SubmitButton>
        </Grid>
      </Grid>
    </Grid>
  );
};

const mapDispatchToProps = {
  openModule: appActions.openModule.invoke
};

export const FeedbackPage = withLayout(
  Layout,
  connect(null, mapDispatchToProps)(Feedback)
);