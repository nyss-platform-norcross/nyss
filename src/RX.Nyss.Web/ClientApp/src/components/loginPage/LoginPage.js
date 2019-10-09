import React from 'react';
import Button from '@material-ui/core/Button';
import { wrapLayout } from '../../utils/layout';
import { AnonymousLayout } from '../layout/AnonymousLayout';
import Paper from '@material-ui/core/Paper';
import Typography from '@material-ui/core/Typography';
import TextField from '@material-ui/core/TextField';
import Link from '@material-ui/core/Link';
import styles from './LoginPage.module.scss';

const LoginPageComponent = () => {
    return (
        <div className={styles.loginContent}>
            <Paper className={styles.loginPaper}>
                <div className={styles.loginPaperContent}>
                    <Typography variant="h1" className={styles.paperHeader}>Welcome to Nyss</Typography>
                    <Typography variant="h2">Log in</Typography>
                    <form>
                        <TextField
                            label="Name"
                            value={"sample"}
                            fullWidth
                        />
                        <TextField
                            label="Password"
                            value={""}
                            InputLabelProps={{ shrink: true }}
                            fullWidth
                        />

                        <div className={styles.forgotPasswordLink}>
                            <Link color="secondary" href={"#"}>Forgot password?</Link>
                        </div>
                        <div className={styles.actions}>
                            <Button variant="outlined" color="primary" style={{ padding: "10px 55px" }}>Log in</Button>
                        </div>
                    </form>
                </div>
            </Paper>
        </div>
    );
}

export const LoginPage = wrapLayout(AnonymousLayout, LoginPageComponent);
