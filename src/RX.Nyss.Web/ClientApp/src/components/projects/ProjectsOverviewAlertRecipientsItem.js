import Divider from '@material-ui/core/Divider';
import Grid from '@material-ui/core/Grid';
import Typography from '@material-ui/core/Typography';
import React, { Fragment } from 'react';
import { stringKeys, strings } from '../../strings';

export const ProjectsOverviewAlertRecipientItem = ({ alertRecipient }) => {

  return (
    <Fragment>

      <Grid container spacing={1}>
        <Grid item xs={3}>
          <Typography variant="h6" >
            {strings(stringKeys.project.form.alertNotificationsRole)}
          </Typography>
          <Typography variant="body1" gutterBottom>
            {alertRecipient.role}
          </Typography>
        </Grid>

        <Grid item xs={3}>
          <Typography variant="h6" >
            {strings(stringKeys.project.form.alertNotificationsOrganization)}
          </Typography>
          <Typography variant="body1" gutterBottom>
            {alertRecipient.organization}
          </Typography>
        </Grid>

        <Grid item xs={3}>
          <Typography variant="h6" >
            {strings(stringKeys.project.form.alertNotificationsEmail)}
          </Typography>
          <Typography variant="body1" gutterBottom>
            {alertRecipient.email}
          </Typography>
        </Grid>

        <Grid item xs={3}>
          <Typography variant="h6" >
            {strings(stringKeys.project.form.alertNotificationsPhoneNumber)}
          </Typography>
          <Typography variant="body1" gutterBottom>
            {alertRecipient.phoneNumber}
          </Typography>
        </Grid>

      </Grid>

      <Grid item xs={12}>
        <Divider />
      </Grid>
    </Fragment>
  );
}
