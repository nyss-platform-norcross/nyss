import styles from "./ProjectsOverviewHealthRiskItem.module.scss";

import Divider from '@material-ui/core/Divider';
import Grid from '@material-ui/core/Grid';
import Typography from '@material-ui/core/Typography';
import React, { Fragment } from 'react';
import { stringKeys, strings } from '../../strings';

export const ProjectsOverviewHealthRiskItem = ({ projectHealthRisk }) => {

    return (
        <Fragment>

            <Grid item xs={12} className={styles.healthRisk}>
                <Typography variant="h3" >{projectHealthRisk.healthRiskCode} - {projectHealthRisk.healthRiskName}</Typography>

                <Grid container spacing={1} className={styles.healthRiskTextArea}>
                    <Grid item xs={4}>
                        <Typography variant="h6" >
                            {strings(stringKeys.project.form.caseDefinition)}
                        </Typography>
                        <Typography variant="body1" gutterBottom>
                            {projectHealthRisk.caseDefinition}
                        </Typography>
                    </Grid>

                    <Grid item xs={4}>
                        <Typography variant="h6" >
                            {strings(stringKeys.project.form.feedbackMessage)}
                        </Typography>
                        <Typography variant="body1" gutterBottom>
                            {projectHealthRisk.feedbackMessage}
                        </Typography>
                    </Grid>

                </Grid>


                <Typography variant="h3">{strings(stringKeys.project.form.alertsSetion)}</Typography>
                <Grid container>
                    <Grid item xs={4}>
                        <Typography variant="h6" >
                            {strings(stringKeys.project.form.alertRuleCountThreshold)}
                        </Typography>
                        <Typography variant="body1" gutterBottom>
                            {projectHealthRisk.alertRuleCountThreshold}
                        </Typography>
                    </Grid>

                    <Grid item xs={4}>
                        <Typography variant="h6" >
                            {strings(stringKeys.project.form.alertRuleDaysThreshold)}
                        </Typography>
                        <Typography variant="body1" gutterBottom>
                            {projectHealthRisk.alertRuleDaysThreshold}
                        </Typography>
                    </Grid>

                    <Grid item xs={4}>
                        <Typography variant="h6" >
                            {strings(stringKeys.project.form.alertRuleKilometersThreshold)}
                        </Typography>
                        <Typography variant="body1" gutterBottom>
                            {projectHealthRisk.alertRuleKilometersThreshold}
                        </Typography>
                    </Grid>

                </Grid>
            </Grid>

            <Grid item xs={12}>
                <Divider />
            </Grid>
        </Fragment>
    );
}
