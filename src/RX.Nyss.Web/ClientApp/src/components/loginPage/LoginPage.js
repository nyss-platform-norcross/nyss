import React, { useState } from "react";
import PropTypes from "prop-types";
import { withLayout } from "../../utils/layout";
import { connect } from "react-redux";
import { AnonymousLayout } from "../layout/AnonymousLayout";
import { Paper, Typography, Link, Grid } from "@material-ui/core";
import styles from "./LoginPage.module.scss";
import { strings, stringKeys } from "../../strings";
import { createForm, validators } from "../../utils/forms";
import TextInputField from "../forms/TextInputField";
import PasswordInputField from "../forms/PasswordInputField";
import * as authActions from "../../authentication/authActions";
import { getRedirectUrl, redirectToRoot } from "../../authentication/auth";
import { ValidationMessage } from "../forms/ValidationMessage";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import { useMount } from "../../utils/lifecycle";
import { trackPageView } from "../../utils/appInsightsHelper";

const LoginPageComponent = (props) => {
  const [form] = useState(() => {
    const fields = {
      userName: "",
      password: "",
    };

    const validation = {
      userName: [validators.required, validators.email],
      password: [validators.required],
    };

    return createForm(fields, validation);
  });

  const [logoClick, setLogoClick] = useState(0);

  useMount(() => {
    if (props.user) {
      redirectToRoot();
    }

    // Track page view
    trackPageView("LoginPage");
  });

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    }

    const values = form.getValues();

    props.login(values.userName, values.password, getRedirectUrl());
  };

  if (props.user) {
    return null;
  }

  return (
    <Grid container direction="row">
      <Grid item xs={12}>
        <div className={styles.loginContent}>
          <Paper className={styles.loginPaper}>
            <div className={styles.loginPageLogo}>
              <svg
                viewBox="0 0 382.46815 140.69596"
                version="1.1"
                id="svg28"
                width="382.46814"
                height="140.69597"
                onClick={() => setLogoClick(logoClick + 1)}
              >
                <title id="title14">Nyss logo</title>
                <g id="Layer_2" data-name="Layer 2">
                  <g id="Layer_1-2" data-name="Layer 1">
                    <path
                      className={styles.redFill}
                      d="M 102.19,125.11 H 71.28 V 50.38 c 0,-6.92 0,-19.8 -15.65,-19.8 -15.42,0 -23.24,10.85 -23.24,32.26 v 62.27 H 0.48 V 38.43 c 0,-9 0,-17.45 -0.21,-25.5 L 0,2.45 h 30.31 l 0.47,3.66 Q 41.47,0 56.38,0 c 12.87,0 23.92,4 31.94,11.49 9.2,8.62 13.87,21.12 13.87,37.16 z"
                      id="path16"
                    />
                    <path
                      className={styles.redFill}
                      d="m 126.82,127.49 a 60.58,60.58 0 0 1 -8.1,-0.52 l -9,-1.12 V 97.79 l 11.66,1.54 a 39.57,39.57 0 0 0 5.31,0.35 c 3.57,0 4.77,0 8.62,-10.11 L 103.19,2.48 h 30.17 l 2.42,6.81 c 6,16.9 10.71,30 14.24,39.76 3.44,-9.92 7.91,-22.78 13.79,-39.69 l 2.39,-6.88 h 29.63 L 163.07,94 c -4.71,13.06 -8.67,20.39 -13.66,25.32 -7.29,7.24 -15.9,8.17 -22.59,8.17 z"
                      id="path18"
                    />
                    <path
                      className={styles.redFill}
                      d="m 237.71,125.5 c -26.58,0 -44.69,-14.43 -48.45,-38.59 l -1.79,-11.54 h 29.64 l 1.95,7.47 c 2.43,9.28 8.24,13.42 18.85,13.42 15.11,0 15.11,-6.4 15.11,-9.48 0,-4.4 -0.92,-5.21 -1.81,-6 -2.6,-2.27 -8.61,-4.56 -17.85,-6.79 -14.63,-3.51 -24,-7.63 -30.26,-13.33 -6.55,-5.94 -9.74,-13.7 -9.74,-23.71 A 34.57,34.57 0 0 1 204.7,11 c 7.75,-7 18.75,-10.75 31.81,-10.75 30.2,0 43.16,19.93 45.08,37 l 1.25,6.86 -30.08,0.67 -0.93,-4.31 c -1.56,-7.29 -6.85,-11 -15.73,-11 -12.49,0 -12.49,4.85 -12.49,6.67 0,2.29 0.34,2.56 1.09,3.15 2.36,1.86 7.73,3.81 16,5.81 30.53,7.36 43,19.2 43,40.85 -0.04,23.3 -18.93,39.55 -45.99,39.55 z"
                      id="path20"
                    />
                    <path
                      className={styles.redFill}
                      d="m 336.06,125.5 c -26.57,0 -44.68,-14.43 -48.44,-38.59 l -1.8,-11.54 h 29.64 l 2,7.47 c 2.43,9.28 8.24,13.42 18.84,13.42 15.11,0 15.11,-6.4 15.11,-9.48 0,-4.4 -0.92,-5.21 -1.81,-6 C 347,78.52 341,76.23 331.71,74 317.08,70.49 307.71,66.37 301.45,60.67 294.9,54.73 291.72,47 291.72,37 a 34.53,34.53 0 0 1 11.34,-26 c 7.75,-7 18.74,-10.75 31.8,-10.75 30.2,0 43.16,19.93 45.09,37 l 1.25,6.86 -30.09,0.67 -0.92,-4.31 c -1.56,-7.29 -6.86,-11 -15.73,-11 -12.5,0 -12.5,4.85 -12.5,6.67 0,2.29 0.34,2.56 1.09,3.15 2.36,1.86 7.73,3.81 16,5.81 30.54,7.36 43,19.2 43,40.85 -0.05,23.3 -18.93,39.55 -45.99,39.55 z"
                      id="path22"
                    />
                    <path
                      className={styles.logoCurve}
                      d="m 157.84,126.19 q 4.84,-7.34 9.68,-14.67 c 1.65,-2.49 4.51,-8.22 8.36,-6.75 1.85,0.7 3.26,2.53 4.37,4.08 1.53,2.12 3,4.32 4.52,6.43 a 46.71,46.71 0 0 0 11.1,11.08 49,49 0 0 0 15.2,6.82 c 3.11,0.82 4.44,-4 1.32,-4.82 a 44.78,44.78 0 0 1 -13.83,-6.21 43.71,43.71 0 0 1 -10.93,-11.43 c -2.82,-4 -5.6,-9.31 -10.63,-10.82 -5.28,-1.6 -9.34,2.49 -12.05,6.47 -3.89,5.71 -7.62,11.53 -11.43,17.29 -1.77,2.7 2.55,5.21 4.32,2.53 z"
                    />
                  </g>
                </g>
                <path
                  key={logoClick}
                  className={styles.animationCurve}
                  d="m 0.38207769,137.47286 c 0,0 107.55487231,0.57311 121.30967231,0.57311 13.75479,0 27.49288,-4.70899 33.18715,-11.85597 7.98172,-10.01797 12.66262,-23.94733 19.53957,-23.86829 16.62038,0.19104 -1.05413,36.13803 97.71636,36.58395 30.57545,0.13803 110.32493,-0.47761 110.32493,-0.47761"
                  id="fullcurve"
                />
              </svg>
            </div>
            <div className={styles.loginPaperContent}>
              <Typography variant="h2" className={styles.loginHeader}>
                {strings(stringKeys.login.title)}
              </Typography>

              {props.loginResponse && (
                <ValidationMessage message={props.loginResponse} />
              )}

              <form onSubmit={handleSubmit}>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <TextInputField
                      label={strings(stringKeys.login.email)}
                      name="userName"
                      field={form.fields.userName}
                      autoFocus
                      inputMode={"email"}
                    />
                  </Grid>

                  <Grid item xs={12}>
                    <PasswordInputField
                      label={strings(stringKeys.login.password)}
                      name="password"
                      field={form.fields.password}
                    />
                  </Grid>
                </Grid>

                <div className={styles.forgotPasswordLink}>
                  <Link color="secondary" href="/resetPassword">
                    {strings(stringKeys.login.forgotPassword)}
                  </Link>
                </div>

                <FormActions>
                  <div />
                  <SubmitButton wide isFetching={props.isFetching}>
                    {strings(stringKeys.login.signIn)}
                  </SubmitButton>
                </FormActions>
              </form>
            </div>
          </Paper>
        </div>
      </Grid>
      <Grid item xs={12}>
        <Grid container className={styles.loginFooter}>
          <Grid item xs={12}>
            <Typography
              align="center"
              variant="h6"
              className={styles.supportLogoHeader}
            >
              Nyss was developed with the support of:
            </Typography>
          </Grid>
          <Grid item>
            <img
              className={styles.supportLogo}
              src="/images/logo-ifrc.svg"
              alt="IFRC logo"
            />
          </Grid>
          <Grid item>
            <img
              className={styles.supportLogo}
              src="/images/logo-nrc.svg"
              alt="Norwegian Red Cross logo"
            />
          </Grid>
          <Grid item>
            <img
              className={styles.supportLogo}
              src="/images/logo-crb.svg"
              alt="Croix-Rouge Belgium logo"
            />
          </Grid>
        </Grid>
      </Grid>
    </Grid>
  );
};

LoginPageComponent.propTypes = {
  login: PropTypes.func,
  loginResponse: PropTypes.string,
};

const mapStateToProps = (state) => ({
  user: state.appData.user,
  loginResponse: state.auth.loginResponse,
  isFetching: state.auth.isFetching,
});

const mapDispatchToProps = {
  login: authActions.login.invoke,
};

export const LoginPage = withLayout(
  AnonymousLayout,
  connect(mapStateToProps, mapDispatchToProps)(LoginPageComponent),
);
