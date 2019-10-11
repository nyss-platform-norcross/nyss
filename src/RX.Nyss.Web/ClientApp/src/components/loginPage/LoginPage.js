import React, { PureComponent } from 'react';
import Button from '@material-ui/core/Button';
import { wrapLayout } from '../../utils/layout';
import { AnonymousLayout } from '../layout/AnonymousLayout';
import Paper from '@material-ui/core/Paper';
import Typography from '@material-ui/core/Typography';
import Link from '@material-ui/core/Link';
import styles from './LoginPage.module.scss';
import strings from '../../strings';
import { createForm, validators } from '../../utils/forms';
import TextInputField from '../forms/TextInputField';
import PasswordInputField from '../forms/PasswordInputField';

class LoginPageComponent extends PureComponent {
    constructor(props) {
        super(props);

        const validation = {
            userName: [validators.required, validators.email],
            password: [validators.required, validators.minLength(8)]
        };

        const fields = {
            userName: "",
            password: ""
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

        this.props.addNewUser(this.form.getValues());
    };

    render() {
        return (
            <div className={styles.loginContent}>
                <Paper className={styles.loginPaper}>
                    <div className={styles.loginPaperContent}>
                        <Typography variant="h1" className={styles.paperHeader}>Welcome to Nyss</Typography>
                        <Typography variant="h2">Log in</Typography>
                        <form onSubmit={this.handleSubmit}>
                            <TextInputField
                                label="User name"
                                name="userName"
                                field={this.form.fields.userName}
                                autoFocus
                            />

                            <PasswordInputField
                                label="Password"
                                name="password"
                                field={this.form.fields.password}
                            />

                            <div className={styles.forgotPasswordLink}>
                                <Link color="secondary" href={"#"}>
                                    {strings["login.forgotPassword"]}
                                </Link>
                            </div>
                            <div className={styles.actions}>
                                <Button variant="outlined" color="primary" style={{ padding: "10px 55px" }}>
                                    {strings["login.signIn"]}
                                </Button>
                            </div>
                        </form>
                    </div>
                </Paper>
            </div>
        );
    }
}

export const LoginPage = wrapLayout(AnonymousLayout, LoginPageComponent);
