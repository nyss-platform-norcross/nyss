import React, { useState } from 'react';
import PropTypes from "prop-types";
import { useLayout } from '../../utils/layout';
import { connect } from "react-redux";
import { AnonymousLayout } from '../layout/AnonymousLayout';
import Paper from '@material-ui/core/Paper';
import Typography from '@material-ui/core/Typography';
import Link from '@material-ui/core/Link';
import styles from './LoginPage.module.scss';
import { strings, stringKeys } from '../../strings';
import { createForm, validators } from '../../utils/forms';
import TextInputField from '../forms/TextInputField';
import PasswordInputField from '../forms/PasswordInputField';
import * as authActions from '../../authentication/authActions';
import { getRedirectUrl, redirectToRoot } from '../../authentication/auth';
import Grid from '@material-ui/core/Grid';
import { ValidationMessage } from '../forms/ValidationMessage';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import { useMount } from '../../utils/lifecycle';

const LoginPageComponent = (props) => {
  const [form] = useState(() => {
    const fields = {
      userName: "",
      password: ""
    };

    const validation = {
      userName: [validators.required, validators.email],
      password: [validators.required]
    };

    return createForm(fields, validation);
  })

  useMount(() => {
    if (props.user) {
      redirectToRoot();
    }
  });

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();

    props.login(values.userName, values.password, getRedirectUrl());
  };

  if (props.user) {
    return null;
  }

  return (
    <div className={styles.loginContent}>
      <Paper className={styles.loginPaper}>
        <div className={styles.loginPaperContent}>
          <div className={styles.loginPageLogo}>
            <img src="/images/logo.svg" alt="Nyss logo" />
          </div>
          <Typography variant="h2" className={styles.loginHeader}>{strings(stringKeys.login.title)}</Typography>

          {props.loginResponse && <ValidationMessage message={props.loginResponse} />}

          <form onSubmit={handleSubmit}>
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <TextInputField
                  label={strings(stringKeys.login.email)}
                  name="userName"
                  field={form.fields.userName}
                  autoFocus
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
  );
};

LoginPageComponent.propTypes = {
  login: PropTypes.func,
  loginResponse: PropTypes.string
};

const mapStateToProps = state => ({
  user: state.appData.user,
  loginResponse: state.auth.loginResponse,
  isFetching: state.auth.isFetching
});

const mapDispatchToProps = {
  login: authActions.login.invoke
};

export const LoginPage = useLayout(
  AnonymousLayout,
  connect(mapStateToProps, mapDispatchToProps)(LoginPageComponent)
);
