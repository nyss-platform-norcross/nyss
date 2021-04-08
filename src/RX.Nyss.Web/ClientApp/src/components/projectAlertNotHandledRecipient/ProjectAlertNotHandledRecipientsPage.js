import styles from './ProjectAlertNotHandledRecipientsPage.module.scss';
import React from 'react';
import { strings, stringKeys } from '../../strings';
import { connect, useSelector } from 'react-redux';
import { useMount } from '../../utils/lifecycle';
import * as projectAlertNotHandledRecipientsActions from './logic/projectAlertNotHandledRecipientsActions';
import { Fragment } from 'react';
import { Administrator } from '../../authentication/roles';
import { ProjectAlertNotHandledRecipientItem } from './components/ProjectAlertNotHandledRecipientItem';
import { Card, CardContent, CircularProgress, Grid, Typography } from '@material-ui/core';

export const ProjectAlertNotHandledRecipientsComponent = ({ openRecipients, projectId, recipients, getFormData, edit, create }) => {
  useMount(() => {
    openRecipients(projectId);
  });

  const currentUserRoles = useSelector(state => state.appData.user.roles);
  const isAdministrator = currentUserRoles.filter(r => r === Administrator).length > 0;
  const isFetchingFormData = useSelector(state => state.projectAlertNotHandledRecipients.formDataFetching);
  const isFetchingList = useSelector(state => state.projectAlertNotHandledRecipients.listFetching);

  return !!recipients && (
    <Fragment>
      <Grid item xs={12} lg={6}>
        <Paper className={styles.container}>
          <Typography variant="h5">
            {strings(stringKeys.projectAlertNotHandledRecipient.title)}
          </Typography>

          <Typography variant="subtitle1" className={styles.description}>
            {strings(stringKeys.projectAlertNotHandledRecipient.description)}
          </Typography>

          {(isFetchingFormData || isFetchingList) && (
            <span className={styles.progressSpinner}>
              <CircularProgress />
            </span>
          )}
          
          <div className={styles.recipientsContainer}>
            {recipients.map(r => (
              <ProjectAlertNotHandledRecipientItem
                key={`alertNotHandledRecipient_${r.userId}`}
                recipient={r}
                isAdministrator={isAdministrator}
                projectId={projectId}
                getFormData={getFormData}
                edit={edit}
                create={create}
              />
            ))}
          </div>
        </Paper>
      </Grid>
    </Fragment>
  );
}

const mapStateToProps = (state) => ({
  recipients: state.projectAlertNotHandledRecipients.listData,
  isListFetching: state.projectAlertNotHandledRecipients.listFetching,
  nationalSocietyIsArchived: state.appData.siteMap.parameters.nationalSocietyIsArchived,
  projectIsClosed: state.appData.siteMap.parameters.projectIsClosed
});

const mapDispatchToProps = {
  openRecipients: projectAlertNotHandledRecipientsActions.openRecipients.invoke,
  create: projectAlertNotHandledRecipientsActions.create.invoke,
  edit: projectAlertNotHandledRecipientsActions.edit.invoke,
  getFormData: projectAlertNotHandledRecipientsActions.getFormData.invoke
};

export const ProjectAlertNotHandledRecipientsPage = connect(mapStateToProps, mapDispatchToProps)(ProjectAlertNotHandledRecipientsComponent);