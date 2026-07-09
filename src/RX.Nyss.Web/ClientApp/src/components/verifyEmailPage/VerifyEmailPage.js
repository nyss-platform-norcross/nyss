import React, { PureComponent } from "react";
import PropTypes from "prop-types";
import { withLayout } from "../../utils/layout";
import { connect } from "react-redux";
import { AnonymousLayout } from "../layout/AnonymousLayout";
import { Paper, Button, Typography } from "@material-ui/core";
import styles from "./VerifyEmailPage.module.scss";
import { strings, stringKeys } from "../../strings";
import { createForm, validators } from "../../utils/forms";
import PasswordInputField from "../forms/PasswordInputField";
import * as authActions from "../../authentication/authActions";
import queryString from "query-string";
import { ValidationMessage } from "../forms/ValidationMessage";
import { trackPageView } from "../../utils/appInsightsHelper";

class VerifyEmailPageComponent extends PureComponent {
  constructor(props) {
    super(props);

    const fields = {
      password: "",
    };

    const validation = {
      password: [validators.required, validators.minLength(8)],
    };

    this.form = createForm(fields, validation);

    // Track page view
    trackPageView("VerifyEmailPage");
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

    this.props.verifyEmail(values.password, email, token);
  };

  render() {
    return (
      <div className={styles.loginContent}>
        <Paper className={styles.loginPaper}>
          <div className={styles.loginPaperContent}>
            <Typography variant="h1" className={styles.paperHeader}>
              {strings(stringKeys.user.verifyEmail.welcome)}
            </Typography>
            <Typography variant="h2">
              {strings(stringKeys.user.verifyEmail.setPassword)}
            </Typography>

            {this.props.verifyEmailErrorMessage && (
              <ValidationMessage message={this.props.verifyEmailErrorMessage} />
            )}

            <form onSubmit={this.handleSubmit}>
              <PasswordInputField
                label={strings(stringKeys.user.verifyEmail.password)}
                name="password"
                field={this.form.fields.password}
              />

              <div className={styles.actions}>
                <Button
                  type="submit"
                  variant="outlined"
                  color="primary"
                  style={{ padding: "10px 55px" }}
                >
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

VerifyEmailPageComponent.propTypes = {
  verifyEmail: PropTypes.func,
  verifyEmailErrorMessage: PropTypes.string,
};

const mapStateToProps = (state) => ({
  verifyEmailErrorMessage: state.auth.verifyEmailErrorMessage,
});

const mapDispatchToProps = {
  verifyEmail: authActions.verifyEmail.invoke,
};

export const VerifyEmailPage = withLayout(
  AnonymousLayout,
  connect(mapStateToProps, mapDispatchToProps)(VerifyEmailPageComponent),
);
