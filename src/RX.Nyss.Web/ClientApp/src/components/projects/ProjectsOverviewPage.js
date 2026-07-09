import React, { Fragment } from "react";
import { connect } from "react-redux";
import { stringKeys, strings } from "../../strings";
import { withLayout } from "../../utils/layout";
import { useMount } from "../../utils/lifecycle";
import { Loading } from "../common/loading/Loading";
import FormActions from "../forms/formActions/FormActions";
import Layout from "../layout/Layout";
import * as projectsActions from "./logic/projectsActions";
import { accessMap } from "../../authentication/accessMap";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { Grid, Typography, makeStyles } from "@material-ui/core";
import { SubMenuTitle } from "../layout/SubMenuTitle";
import EditIcon from "@material-ui/icons/Edit";
import { trackPageView } from "../../utils/appInsightsHelper";

const useStyles = makeStyles({
  bodyText: {
    fontSize: 16,
  },
  formActions: {
    marginTop: 0,
  },
});

const ProjectsOverviewPageComponent = (props) => {
  const classes = useStyles();

  useMount(() => {
    props.openOverview(props.nationalSocietyId, props.projectId);

    // Track page view
    trackPageView("ProjectsOverviewPage");
  });

  if (props.isFetching || !props.data) {
    return <Loading />;
  }

  return (
    <Fragment>
      <Grid container spacing={4} fixed="true" style={{ maxWidth: 800 }}>
        <Grid item xs={12}>
          <Grid container justifyContent="space-between" alignItems="center">
            <SubMenuTitle />
            {!props.isClosed && (
              <FormActions className={classes.formActions}>
                <TableActionsButton
                  startIcon={<EditIcon />}
                  onClick={() =>
                    props.openEdition(props.nationalSocietyId, props.projectId)
                  }
                  roles={accessMap.projects.edit}
                  variant={"contained"}
                >
                  {strings(stringKeys.common.buttons.edit)}
                </TableActionsButton>
              </FormActions>
            )}
          </Grid>
          <Typography variant="h5">
            {strings(stringKeys.project.form.name)}
          </Typography>
          <Typography variant="body1" className={classes.bodyText} gutterBottom>
            {props.data.name}
          </Typography>

          <Typography variant="h5">
            {strings(stringKeys.project.form.allowMultipleOrganizations)}
          </Typography>
          <Typography variant="body1" className={classes.bodyText} gutterBottom>
            {strings(
              stringKeys.common.boolean[
                String(props.data.allowMultipleOrganizations)
              ],
            )}
          </Typography>
        </Grid>
      </Grid>
    </Fragment>
  );
};

ProjectsOverviewPageComponent.propTypes = {};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isFetching: state.projects.formFetching,
  data: state.projects.overviewData,
  isClosed: state.appData.siteMap.parameters.projectIsClosed,
  callingUserRoles: state.appData.user.roles,
});

const mapDispatchToProps = {
  openOverview: projectsActions.openOverview.invoke,
  openEdition: projectsActions.goToEdition,
  goToList: projectsActions.goToList,
};

export const ProjectsOverviewPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsOverviewPageComponent),
);
