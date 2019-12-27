import React, { Fragment } from 'react';
import { connect } from "react-redux";
import { stringKeys, strings } from '../../strings';
import { useLayout } from '../../utils/layout';
import { useMount } from '../../utils/lifecycle';
import { Loading } from '../common/loading/Loading';
import FormActions from '../forms/formActions/FormActions';
import Layout from '../layout/Layout';
import * as projectsActions from './logic/projectsActions';
import Grid from '@material-ui/core/Grid';
import Typography from '@material-ui/core/Typography';
import { ProjectsOverviewHealthRiskItem } from "./ProjectsOverviewHealthRiskItem";
import { accessMap } from '../../authentication/accessMap';
import { TableActionsButton } from "../common/tableActions/TableActionsButton";
import Chip from "@material-ui/core/Chip";

const ProjectsOverviewPageComponent = (props) => {
  useMount(() => {
    props.openOverview(props.nationalSocietyId, props.projectId);
  });

  if (props.isFetching || !props.data) {
    return <Loading />;
  }

  const selectedTimeZone =  props.data.formData.timeZones.filter( (timeZone) => timeZone.id === props.data.timeZoneId)[0];
  
  return (
    <Fragment>

        <Grid container spacing={4} fixed='true' style={{ maxWidth: 800 }}>
          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.project.form.name)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.name}
            </Typography>

            <Typography variant="h6">
              {strings(stringKeys.project.form.timeZone)}
            </Typography>
            <Typography variant="body1" gutterBottom>           
              {selectedTimeZone.displayName }
            </Typography>

            <Typography variant="h6">
              {strings(stringKeys.project.form.healthRisks)}
            </Typography>
           
            {props.data.projectHealthRisks.map( hr =>
              <Chip  key={`projectsHealthRiskItemIcon_${hr.healthRiskId}`} label={hr.healthRiskName} />
            )}
          </Grid>

            <Grid item xs={12}>
                        
            <Typography variant="h3">{strings(stringKeys.project.form.overviewHealthRisksSection)}</Typography>            
              <Grid container spacing={3}>
                {props.data.projectHealthRisks.map( hr =>
                  <ProjectsOverviewHealthRiskItem 
                    key={`projectsHealthRiskItem_${hr.healthRiskId}`}         
                    projectHealthRisk={hr}
                    />
                  )}
              </Grid>
            </Grid>
                      
            <Grid item xs={9}>
            <Typography variant="h3">{strings(stringKeys.project.form.overviewEmailNotificationsSection)}</Typography>
            <Typography variant="subtitle1" gutterBottom>{strings(stringKeys.project.form.emailNotificationDescription)}</Typography>

            {props.data.emailAlertRecipients.map(emailRecipient => (
              <Typography variant="body1" key={`projectEmailNotificationItem_${emailRecipient.id}`}>           
              {emailRecipient.email }
            </Typography>
            ))}
                         
          </Grid>

          <Grid item xs={9}>
            <Typography variant="h3">{strings(stringKeys.project.form.overviewSmsNotificationsSection)}</Typography>
            <Typography variant="subtitle1" gutterBottom >{strings(stringKeys.project.form.smsNotificationDescription)}</Typography>

             {props.data.smsAlertRecipients.map(smsRecipient => (
              <Typography variant="body1" key={`projectSmsNotificationItem_${smsRecipient.id}`}>           
              {smsRecipient.phoneNumber }
            </Typography>
            ))} 

          </Grid>         
        </Grid>

        <FormActions>
          <TableActionsButton variant="outlined" color="primary" onClick={() => props.openEdition(props.nationalSocietyId, props.projectId)} roles={accessMap.projects.edit}>
            {strings(stringKeys.project.edit)}
          </TableActionsButton>
        </FormActions>
    </Fragment>
  );
}

ProjectsOverviewPageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  healthRisks: state.projects.overviewHealthRisks,
  timeZones: state.projects.overviewTimeZones,
  projectId: ownProps.match.params.projectId,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isFetching: state.projects.formFetching,  
  data: state.projects.overviewData,  
});

const mapDispatchToProps = {
  openOverview: projectsActions.openOverview.invoke,
  openEdition: projectsActions.goToEdition,
  goToList: projectsActions.goToList
};

export const ProjectsOverviewPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsOverviewPageComponent)
);
