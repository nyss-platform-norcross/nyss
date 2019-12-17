import React, { PureComponent } from 'react';
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
import { getRedirectUrl } from '../../authentication/auth';
import Grid from '@material-ui/core/Grid';
import { ValidationMessage } from '../forms/ValidationMessage';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';

class LoginPageComponent extends PureComponent {
  constructor(props) {
    super(props);

    const fields = {
      userName: "",
      password: ""
    };

    const validation = {
      userName: [validators.required, validators.email],
      password: [validators.required]
    };

    this.form = createForm(fields, validation);
  };

  handleSubmit = (e) => {
    e.preventDefault();

    if (!this.form.isValid()) {
      return;
    };

    const values = this.form.getValues();

    this.props.login(values.userName, values.password, getRedirectUrl());
  };

  render() {
    return (
      <div className={styles.loginContent}>
        <Paper className={styles.loginPaper}>
          <div className={styles.loginPaperContent}>
            <Typography variant="h1" className={styles.paperHeader}>{strings(stringKeys.login.welcome)}</Typography>
            <Typography variant="h2">{strings(stringKeys.login.title)}</Typography>

            {this.props.loginResponse && <ValidationMessage message={this.props.loginResponse} />}

            <form onSubmit={this.handleSubmit}>
              <Grid container spacing={3}>
                <Grid item xs={12}>

                  <TextInputField
                    label={strings(stringKeys.login.email)}
                    name="userName"
                    field={this.form.fields.userName}
                    autoFocus
                  />
                </Grid>

                <Grid item xs={12}>
                  <PasswordInputField
                    label={strings(stringKeys.login.password)}
                    name="password"
                    field={this.form.fields.password}
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
                <SubmitButton wide isFetching={this.props.isFetching}>
                  {strings(stringKeys.login.signIn)}
                </SubmitButton>
              </FormActions>
            </form>
          </div>
        </Paper>
      </div>
    );
  }
}

LoginPageComponent.propTypes = {
  login: PropTypes.func,
  loginResponse: PropTypes.string
};

const mapStateToProps = state => ({
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
