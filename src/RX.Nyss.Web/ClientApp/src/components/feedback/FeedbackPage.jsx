import React, { useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { useMount } from "../../utils/lifecycle";
import * as appActions from "../app/logic/appActions";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import { connect } from "react-redux";
import { Grid, Typography } from "@material-ui/core";
import { strings, stringKeys } from "../../strings";
import { createForm, validators } from "../../utils/forms";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import TextInputField from "../forms/TextInputField";
import { makeStyles } from "@material-ui/core/styles";
import { sendFeedback } from "../app/logic/appActions";
import GoBackToButton from "../common/buttons/goBackToButton/GoBackToButton";
import { goBack } from "connected-react-router";
import { trackPageView } from "../../utils/appInsightsHelper";

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
  container: {
    height: "100%",
  },
});

const Feedback = ({ openModule, match, goBack }) => {
  const isSendingFeedback = useSelector(
    (state) => state.appData.feedback.isSending,
  );
  const sendFeedbackResult = useSelector(
    (state) => state.appData.feedback.result,
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
    setHasSent(false);

    // Track page view
    trackPageView("FeedbackPage");
  });

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.isValid()) {
      return;
    }

    dispatch(
      sendFeedback.invoke({
        message: form.fields.message.value,
      }),
    );

    setHasSent(true);
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

  const handleGoBack = () => {
    resetForm();
    goBack();
  };

  const handleSendNewFeedback = () => {
    resetForm();
    setHasSent(false);
  };

  return (
    <>
      {(!hasSent || isSendingFeedback) && (
        <Grid>
          <GoBackToButton onClick={handleGoBack} size={"large"}>
            {strings(stringKeys.common.buttons.goBack)}
          </GoBackToButton>
        </Grid>
      )}
      <Grid
        container
        className={classes.container}
        justifyContent="center"
        alignItems="center"
      >
        <>
          {(!hasSent || isSendingFeedback) && (
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
              <Grid item container justifyContent={"flex-end"}>
                <SubmitButton
                  isFetching={isSendingFeedback}
                  onClick={handleSubmit}
                >
                  {strings(stringKeys.feedback.submit)}
                </SubmitButton>
              </Grid>
            </Grid>
          )}
          {hasSent && sendFeedbackResult === "ok" && (
            <Grid container justifyContent="center" spacing={7}>
              <Grid container direction="column" alignItems="center">
                <Typography variant={"h1"} className={classes.title}>
                  {strings(stringKeys.feedback.thankYouTitle)}
                </Typography>
                <Typography>
                  {strings(stringKeys.feedback.thankYouDescription)}
                </Typography>
                <Grid item style={{ marginTop: "2rem" }}>
                  <GoBackToButton onClick={handleGoBack}>
                    {strings(stringKeys.common.buttons.goBack)}
                  </GoBackToButton>
                </Grid>
              </Grid>
            </Grid>
          )}

          {hasSent && sendFeedbackResult === "error" && (
            <Grid
              container
              direction="column"
              justifyContent="center"
              alignItems="center"
              spacing={7}
            >
              <Typography variant={"h1"} className={classes.title}>
                {strings(stringKeys.feedback.errorTitle)}
              </Typography>
              <Typography>
                {strings(stringKeys.feedback.errorDescription)}
              </Typography>
              <TableActionsButton
                variant="contained"
                style={{ marginTop: "30px" }}
                onClick={handleSendNewFeedback}
              >
                {strings(stringKeys.common.buttons.tryAgain)}
              </TableActionsButton>
            </Grid>
          )}
        </>
      </Grid>
    </>
  );
};

const mapDispatchToProps = {
  openModule: appActions.openModule.invoke,
  goBack: goBack,
};

export const FeedbackPage = withLayout(
  Layout,
  connect(null, mapDispatchToProps)(Feedback),
  { fillPage: true },
);
