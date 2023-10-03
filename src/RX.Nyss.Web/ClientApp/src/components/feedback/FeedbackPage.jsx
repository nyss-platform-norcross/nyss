import React, { useState, useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
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
import { sendFeedback } from "../app/logic/appActions";

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
    fontSize: 24,
    textAlign: "center",
  },
});

const Feedback = ({ openModule, match }) => {
  const isSendingFeedback = useSelector(
    (state) => state.appData.feedback.isSending
  );
  const sendFeedbackResult = useSelector(
    (state) => state.appData.feedback.result
  );

  const classes = useStyles();
  const dispatch = useDispatch();

  const [hasSent, setHasSent] = useState(false);
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

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.isValid()) {
      return;
    }

    dispatch(
      sendFeedback.invoke({
        message: form.fields.message.value,
      })
    );
  };

  const resetForm = () => {
    setForm(() => {
      const fields = { message: "" };
      const validation = {
        message: [validators.maxLength(1000), validators.required],
      };
      return createForm(fields, validation);
    });
  };

  useEffect(() => {
    if (sendFeedbackResult === "ok") {
      setHasSent(true);
      resetForm();
    }
  }, [sendFeedbackResult]);

  useEffect(() => {
    setTimeout(() => {
      setHasSent(false);
    }, [4000]);
    
  }, [form])

  return (
    <Grid container justifyContent="center">
      {!hasSent ? (
        <Grid container style={{ width: 600 }} spacing={3}>
          <Grid item xs={12}>
            <Typography variant={"h1"} className={classes.title}>
              {strings(stringKeys.feedback.title)}
            </Typography>
            <TextInputField
              label={strings(stringKeys.feedback.description)}
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
            <SubmitButton isFetching={isSendingFeedback} onClick={handleSubmit}>
              {strings(stringKeys.feedback.submit)}
            </SubmitButton>
          </Grid>
        </Grid>
      ) : (
        <Grid container justifyContent="center" spacing={7}>
          <Grid item>
            <img
              src="/images/feedback-illustration.png"
              alt="Thank you for the feedback illustration"
            />
          </Grid>
          <Grid item>
            <Typography variant={"h1"} className={classes.title}>
              {strings(stringKeys.feedback.thankYouTitle)}
            </Typography>
            <Typography>
              {strings(stringKeys.feedback.thankYouDescription)}
            </Typography>
          </Grid>
        </Grid>
      )}
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