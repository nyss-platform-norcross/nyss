import styles from './ProjectAlertNotHandledRecipientsPage.module.scss';
import React from 'react';
import { strings, stringKeys } from '../../strings';
import Paper from '@material-ui/core/Paper';
import Typography from '@material-ui/core/Typography';
import Grid from '@material-ui/core/Grid';
import { connect, useSelector } from 'react-redux';
import { useMount } from '../../utils/lifecycle';
import * as projectAlertNotHandledRecipientsActions from './logic/projectAlertNotHandledRecipientsActions';
import { Fragment } from 'react';
import { Administrator } from '../../authentication/roles';
import { ProjectAlertNotHandledRecipientItem } from './components/ProjectAlertNotHandledRecipientItem';
import LinearProgress from '@material-ui/core/LinearProgress';

export const ProjectAlertNotHandledRecipientsComponent = ({ openRecipients, projectId, recipients, getFormData, edit, ...props }) => {
  useMount(() => {
    openRecipients(projectId);
  });

  const currentUserRoles = useSelector(state => state.appData.user.roles);
  const isAdministrator = currentUserRoles.filter(r => r === Administrator).length > 0;
  const isFetchingFormData = useSelector(state => state.projectAlertNotHandledRecipients.formDataFetching);

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

          {isFetchingFormData && <LinearProgress />}
          <div className={styles.recipientsContainer}>
            {recipients.map(r => (
              <ProjectAlertNotHandledRecipientItem
                key={`alertNotHandledRecipient_${r.userId}`}
                recipient={r}
                isAdministrator={isAdministrator}
                projectId={projectId}
                getFormData={getFormData}
                edit={edit}
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