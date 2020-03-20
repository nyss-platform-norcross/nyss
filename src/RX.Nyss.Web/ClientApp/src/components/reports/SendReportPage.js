import styles from "./ReportsEditPage.module.scss";

import React, { useEffect, useState, Fragment } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as reportsActions from './logic/reportsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import Button from "@material-ui/core/Button";
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { ValidationMessage } from '../forms/ValidationMessage';
import dayjs from 'dayjs';

const SendReportPageComponent = (props) => {
    const [form, setForm] = useState(null);

    useEffect(() => {
        const fields = {
            phoneNumber: "",
            message: "",
            apiKey: ""
        };

        const validation = {
            phoneNumber: [validators.required, validators.phoneNumber],
            message: [validators.required],
            apiKey: [validators.required]
        };

        setForm(createForm(fields, validation));
    }, [props.data, props.match]);

    const handleSubmit = (e) => {
        e.preventDefault();

        if (!form.isValid()) {
            return;
        };

        const values = form.getValues();
        var date = new Date();
        props.sendReport({
            sender: values.phoneNumber,
            text: values.message,
            timestamp: dayjs(Date.UTC(date.getFullYear(), date.getMonth(), date.getDay(), date.getHours(), date.getMinutes(), date.getSeconds())).format("YYYYMMDDHHmmss"),
            apiKey: values.apiKey
        });
    };

    if (!form) {
        return <Loading />;
    }

    return (
        <Fragment>
            {props.error && <ValidationMessage message={props.error} />}
            <Form onSubmit={handleSubmit}>
                <Grid container spacing={3}>
                    <Grid item xs={12}>
                        <TextInputField
                            className={styles.fullWidth}
                            label={strings(stringKeys.reports.sendReport.phoneNumber)}
                            name="phoneNumber"
                            field={form.fields.phoneNumber}
                        />
                    </Grid>
                </Grid>
                <Grid container spacing={3}>
                    <Grid item xs={12}>
                        <TextInputField
                            className={styles.fullWidth}
                            label={strings(stringKeys.reports.sendReport.message)}
                            name="message"
                            field={form.fields.message}
                        />
                    </Grid>
                </Grid>
                <Grid container spacing={3}>
                    <Grid item xs={12}>
                        <TextInputField
                            className={styles.fullWidth}
                            label={strings(stringKeys.reports.sendReport.apiKey)}
                            name="apiKey"
                            field={form.fields.apiKey}
                        />
                    </Grid>
                </Grid>

                <FormActions>
                    {/* <Button onClick={() => props.goToList(props.projectId)}>{strings(stringKeys.reports.sendReport.goToReportList)}</Button> */}
                    <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.reports.sendReport.sendReport)}</SubmitButton>
                </FormActions>
            </Form>
        </Fragment>
    );
}

SendReportPageComponent.propTypes = {
};

const mapStateToProps = (state) => ({
    isSaving: state.reports.formSaving,
    data: state.reports.formData,
    error: state.reports.formError
});

const mapDispatchToProps = {
    sendReport: reportsActions.sendReport.invoke,
    goToList: reportsActions.goToList
};

export const SendReportPage = useLayout(
    Layout,
    connect(mapStateToProps, mapDispatchToProps)(SendReportPageComponent)
);
