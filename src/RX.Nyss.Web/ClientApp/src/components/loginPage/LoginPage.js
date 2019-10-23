import React, { PureComponent } from 'react';
import PropTypes from "prop-types";
import Button from '@material-ui/core/Button';
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
import SnackbarContent from '@material-ui/core/SnackbarContent';

class LoginPageComponent extends PureComponent {
  constructor(props) {
    super(props);

    const fields = {
      userName: "",
      password: ""
    };

    const validation = {
      userName: [validators.required, validators.email],
      password: [validators.required, validators.minLength(4)]
    };

    this.form = createForm(fields, validation);
  };

  handleSubmit = (e) => {
    e.preventDefault();
    this.onSubmit();
  };

  onSubmit = () => {
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
            <Typography variant="h1" className={styles.paperHeader}>Welcome to Nyss</Typography>
            <Typography variant="h2">Log in</Typography>

            {this.props.loginResponse &&
              <SnackbarContent
                message={this.props.loginResponse}
              />
            }

            <form onSubmit={this.handleSubmit}>
              <TextInputField
                label="User name"
                name="userName"
                field={this.form.fields.userName}
                autoFocus
              />

              <PasswordInputField
                label={strings(stringKeys.login.password)}
                name="password"
                field={this.form.fields.password}
              />

              <div className={styles.forgotPasswordLink}>
                <Link color="secondary" href={"#"}>
                  {strings(stringKeys.login.forgotPassword)}
                </Link>
              </div>

              <div className={styles.actions}>
                <Button type="submit" variant="outlined" color="primary" style={{ padding: "10px 55px" }}>
                  {strings(stringKeys.login.signIn)}
                </Button>
              </div>
            </form>
          </div>
        </Paper>
      </div>
    );
  }
}

const mapStateToProps = state => ({
  loginResponse: state.auth.loginResponse
});

const mapDispatchToProps = {
  login: authActions.login.invoke
};

LoginPageComponent.propTypes = {
  login: PropTypes.func,
  loginResponse: PropTypes.string
};

export const LoginPage = useLayout(
  AnonymousLayout,
  connect(mapStateToProps, mapDispatchToProps)(LoginPageComponent)
);
