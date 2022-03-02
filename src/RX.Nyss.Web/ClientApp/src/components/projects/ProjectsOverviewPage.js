import styles from "./ProjectsOverviewPage.module.scss";
import React, { Fragment } from 'react';
import { connect } from "react-redux";
import { stringKeys, strings } from '../../strings';
import { withLayout } from '../../utils/layout';
import { useMount } from '../../utils/lifecycle';
import { Loading } from '../common/loading/Loading';
import FormActions from '../forms/formActions/FormActions';
import Layout from '../layout/Layout';
import * as projectsActions from './logic/projectsActions';
import { ProjectsOverviewHealthRiskItem } from "./ProjectsOverviewHealthRiskItem";
import { accessMap } from '../../authentication/accessMap';
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { Chip, Grid, Typography } from "@material-ui/core";
import { Coordinator, Administrator } from "../../authentication/roles";

const ProjectsOverviewPageComponent = (props) => {
  useMount(() => {
    props.openOverview(props.nationalSocietyId, props.projectId);
  });

  if (props.isFetching || !props.data) {
    return <Loading />;
  }

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
            {strings(stringKeys.project.form.allowMultipleOrganizations)}
          </Typography>
          <Typography variant="body1" gutterBottom>
            {strings(stringKeys.common.boolean[String(props.data.allowMultipleOrganizations)])}
          </Typography>

          <Typography variant="h6">
            {strings(stringKeys.project.form.healthRisks)}
          </Typography>

          {props.data.projectHealthRisks.map(hr =>
            <Chip key={`projectsHealthRiskItemIcon_${hr.healthRiskId}`} label={hr.healthRiskName} className={styles.chip} />
          )}
        </Grid>

        <Grid item xs={12}>
          <Typography variant="h3">{strings(stringKeys.project.form.overviewHealthRisksSection)}</Typography>
          <Grid container spacing={2}>
            {props.data.projectHealthRisks.map(hr =>
              <Grid item xs={12} key={`projectsHealthRiskItem_${hr.healthRiskId}`}>
                <ProjectsOverviewHealthRiskItem projectHealthRisk={hr} />
              </Grid>
            )}
          </Grid>
        </Grid>
      </Grid>

      {!props.isClosed && (
        <FormActions>
          {(!props.data.allowMultipleOrganizations || (props.data.hasCoordinator && props.callingUserRoles.some(r => r === Coordinator || r === Administrator))) && (
            <TableActionsButton
              onClick={() => props.openEdition(props.nationalSocietyId, props.projectId)}
              roles={accessMap.projects.edit}
              variant={"contained"}
            >
              {strings(stringKeys.common.buttons.edit)}
            </TableActionsButton>
          )}
        </FormActions>
      )}
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
  isClosed: state.appData.siteMap.parameters.projectIsClosed,
  callingUserRoles: state.appData.user.roles
});

const mapDispatchToProps = {
  openOverview: projectsActions.openOverview.invoke,
  openEdition: projectsActions.goToEdition,
  goToList: projectsActions.goToList
};

export const ProjectsOverviewPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsOverviewPageComponent)
);
