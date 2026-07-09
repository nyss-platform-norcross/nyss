import React, { PureComponent } from "react";
import PropTypes from "prop-types";
import { withLayout } from "../../utils/layout";
import { connect } from "react-redux";
import { AnonymousLayout } from "../layout/AnonymousLayout";
import { Paper, Button, Typography } from "@material-ui/core";
import styles from "./ResetPasswordCallbackPage.module.scss";
import { strings, stringKeys } from "../../strings";
import { createForm, validators } from "../../utils/forms";
import PasswordInputField from "../forms/PasswordInputField";
import * as authActions from "../../authentication/authActions";
import queryString from "query-string";
import { ValidationMessage } from "../forms/ValidationMessage";
import { trackPageView } from "../../utils/appInsightsHelper";

class ResetPasswordCallbackPageComponent extends PureComponent {
  constructor(props) {
    super(props);

    // Track page view
    trackPageView("ResetPasswordCallbackPage");

    const fields = {
      password: "",
    };

    const validation = {
      password: [validators.required, validators.minLength(8)],
    };

    this.form = createForm(fields, validation);
  }

  handleSubmit = (e) => {
    e.preventDefault();

    if (!this.form.isValid()) {
      return;
    }

    const queryStrings = queryString.parse(this.props.location.search);

    const values = this.form.getValues();
    const email = queryStrings.email;
    const token = queryStrings.token;

    this.props.resetPasswordCallback(values.password, email, token);
  };

  render() {
    return (
      <div className={styles.loginContent}>
        <Paper className={styles.loginPaper}>
          <div className={styles.loginPaperContent}>
            <Typography variant="h2">
              {strings(stringKeys.user.resetPassword.enterNewPassword)}
            </Typography>

            {this.props.resetPasswordCallbackErrorMessage && (
              <ValidationMessage
                message={this.props.resetPasswordCallbackErrorMessage}
              />
            )}

            <form onSubmit={this.handleSubmit}>
              <PasswordInputField
                label={strings(stringKeys.user.verifyEmail.password)}
                name="password"
                field={this.form.fields.password}
              />

              <div className={styles.actions}>
                <Button type="submit" variant="outlined" color="primary">
                  {strings(stringKeys.user.verifyEmail.signIn)}
                </Button>
              </div>
            </form>
          </div>
        </Paper>
      </div>
    );
  }
}

ResetPasswordCallbackPageComponent.propTypes = {
  resetPasswordCallback: PropTypes.func,
  resetPasswordCallbackErrorMessage: PropTypes.string,
};

const mapStateToProps = (state) => ({
  resetPasswordCallbackErrorMessage:
    state.auth.resetPasswordCallbackErrorMessage,
});

const mapDispatchToProps = {
  resetPasswordCallback: authActions.resetPasswordCallback.invoke,
};

export const ResetPasswordCallbackPage = withLayout(
  AnonymousLayout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(ResetPasswordCallbackPageComponent),
);
