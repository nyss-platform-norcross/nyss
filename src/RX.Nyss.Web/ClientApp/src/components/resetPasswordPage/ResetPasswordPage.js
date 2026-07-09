import React, { PureComponent } from "react";
import PropTypes from "prop-types";
import { withLayout } from "../../utils/layout";
import { connect } from "react-redux";
import { AnonymousLayout } from "../layout/AnonymousLayout";
import TextInputField from "../forms/TextInputField";
import styles from "./ResetPasswordPage.module.scss";
import { strings, stringKeys } from "../../strings";
import { createForm, validators } from "../../utils/forms";
import * as authActions from "../../authentication/authActions";
import { ValidationMessage } from "../forms/ValidationMessage";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import FormActions from "../forms/formActions/FormActions";
import { Link, Paper, Typography, Grid } from "@material-ui/core";
import { trackPageView } from "../../utils/appInsightsHelper";

class ResetPasswordPageComponent extends PureComponent {
  constructor(props) {
    super(props);

    const fields = {
      emailAddress: "",
    };

    const validation = {
      emailAddress: [validators.required, validators.email],
    };

    this.form = createForm(fields, validation);

    // Track page view
    trackPageView("ResetPasswordPage");
  }

  handleSubmit = (e) => {
    e.preventDefault();

    if (!this.form.isValid()) {
      return;
    }

    const values = this.form.getValues();

    this.props.resetPassword(values.emailAddress);
  };

  render() {
    return (
      <div className={styles.loginContent}>
        <Paper className={styles.loginPaper}>
          <div className={styles.loginPaperContent}>
            <Typography variant="h2">
              {strings(stringKeys.user.resetPassword.enterEmail)}
            </Typography>

            {this.props.resetPasswordErrorMessage && (
              <ValidationMessage
                message={this.props.resetPasswordErrorMessage}
              />
            )}

            <form onSubmit={this.handleSubmit}>
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <TextInputField
                    label={strings(stringKeys.user.resetPassword.emailAddress)}
                    name="emailAddress"
                    field={this.form.fields.emailAddress}
                    autoFocus
                    inputMode={"email"}
                  />
                </Grid>
              </Grid>

              <FormActions>
                <Grid container spacing={2}>
                  <Grid item xs={12} lg={6}>
                    <Link href="/">
                      {strings(stringKeys.user.resetPassword.goToLoginPage)}
                    </Link>
                  </Grid>
                  <Grid item xs={12} lg={6}>
                    <SubmitButton isFetching={this.props.isFetching}>
                      {strings(stringKeys.user.resetPassword.submit)}
                    </SubmitButton>
                  </Grid>
                </Grid>
              </FormActions>
            </form>
          </div>
        </Paper>
      </div>
    );
  }
}

ResetPasswordPageComponent.propTypes = {
  resetPassword: PropTypes.func,
  resetPasswordErrorMessage: PropTypes.string,
};

const mapStateToProps = (state) => ({
  resetPasswordErrorMessage: state.auth.resetPasswordErrorMessage,
  isFetching: state.requests.isFetching,
});

const mapDispatchToProps = {
  resetPassword: authActions.resetPassword.invoke,
};

export const ResetPasswordPage = withLayout(
  AnonymousLayout,
  connect(mapStateToProps, mapDispatchToProps)(ResetPasswordPageComponent),
);
