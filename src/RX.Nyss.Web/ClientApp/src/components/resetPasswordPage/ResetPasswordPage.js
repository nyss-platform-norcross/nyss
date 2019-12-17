import React, { PureComponent } from 'react';
import PropTypes from "prop-types";
import { useLayout } from '../../utils/layout';
import { connect } from "react-redux";
import { AnonymousLayout } from '../layout/AnonymousLayout';
import Paper from '@material-ui/core/Paper';
import Button from '@material-ui/core/Button';
import TextInputField from '../forms/TextInputField';
import Typography from '@material-ui/core/Typography';
import styles from './ResetPasswordPage.module.scss';
import { strings, stringKeys } from '../../strings';
import { createForm, validators } from '../../utils/forms';
import * as authActions from '../../authentication/authActions';
import Grid from '@material-ui/core/Grid';
import { ValidationMessage } from '../forms/ValidationMessage';

class ResetPasswordPageComponent extends PureComponent {
  constructor(props) {
    super(props);

    const fields = {
      emailAddress: ""
    };

    const validation = {
      emailAddress: [validators.required, validators.email],
    };

    this.form = createForm(fields, validation);
  };

  handleSubmit = (e) => {
    e.preventDefault();

    if (!this.form.isValid()) {
      return;
    };

    const values = this.form.getValues();

    this.props.resetPassword(values.emailAddress);
  };

  render() {
    return (
      <div className={styles.loginContent}>
        <Paper className={styles.loginPaper}>
          <div className={styles.loginPaperContent}>
            <Typography variant="h2">{strings(stringKeys.user.resetPassword.enterEmail)}</Typography>

            {this.props.resetPasswordErrorMessage && <ValidationMessage message={this.props.resetPasswordErrorMessage} />}

            <form onSubmit={this.handleSubmit}>
              <Grid container spacing={3}>
                <Grid item xs={12}>
                  <TextInputField
                    label={strings(stringKeys.user.resetPassword.emailAddress)}
                    name="emailAddress"
                    field={this.form.fields.emailAddress}
                    autoFocus
                  />
                </Grid>
              </Grid>

              <div className={styles.actions}>
                <Button type="submit" variant="outlined" color="primary">
                  {strings(stringKeys.user.resetPassword.submit)}
                </Button>
              </div>
            </form>
          </div>
        </Paper>
      </div>
    );
  }
}

ResetPasswordPageComponent.propTypes = {
  resetPassword: PropTypes.func,
  resetPasswordErrorMessage: PropTypes.string
};

const mapStateToProps = state => ({
  resetPasswordErrorMessage: state.auth.resetPasswordErrorMessage
});

const mapDispatchToProps = {
  resetPassword: authActions.resetPassword.invoke
};

export const ResetPasswordPage = useLayout(
  AnonymousLayout,
  connect(mapStateToProps, mapDispatchToProps)(ResetPasswordPageComponent)
);
