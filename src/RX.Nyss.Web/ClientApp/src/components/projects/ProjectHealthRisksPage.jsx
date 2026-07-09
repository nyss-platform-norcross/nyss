import styles from "./ProjectsOverviewPage.module.scss";
import React, { Fragment } from "react";
import { connect, useSelector } from "react-redux";
import { stringKeys, strings } from "../../strings";
import { withLayout } from "../../utils/layout";
import { useMount } from "../../utils/lifecycle";
import { Loading } from "../common/loading/Loading";
import FormActions from "../forms/formActions/FormActions";
import Layout from "../layout/Layout";
import * as projectsActions from "./logic/projectsActions";
import { ProjectsOverviewHealthRiskItem } from "./ProjectsOverviewHealthRiskItem";
import { accessMap } from "../../authentication/accessMap";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { Chip, Grid, Typography, makeStyles } from "@material-ui/core";
import EditIcon from "@material-ui/icons/Edit";
import { SubMenuTitle } from "../layout/SubMenuTitle";
import { trackPageView } from "../../utils/appInsightsHelper";

const useStyles = makeStyles({
  formActions: {
    marginTop: 0,
  },
});

const ProjectHealthRisksPageComponent = (props) => {
  const classes = useStyles();

  useMount(() => {
    props.openHealthRisksOverview(props.nationalSocietyId, props.projectId);

    // Track page view
    trackPageView("ProjectHealthRisksPage");
  });

  const useRtlDirection = useSelector(
    (state) => state.appData.user.languageCode === "ar",
  );

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
                    props.openHealthRisksEdition(
                      props.nationalSocietyId,
                      props.projectId,
                    )
                  }
                  roles={accessMap.projects.edit}
                  variant={"contained"}
                >
                  {strings(stringKeys.common.buttons.edit)}
                </TableActionsButton>
              </FormActions>
            )}
          </Grid>
          {props.data.projectHealthRisks.map((hr) => (
            <Chip
              key={`projectsHealthRiskItemIcon_${hr.healthRiskId}`}
              label={hr.healthRiskName}
              className={styles.chip}
            />
          ))}
        </Grid>

        <Grid item xs={12}>
          <Typography variant="h3">
            {strings(stringKeys.project.form.overviewHealthRisksSection)}
          </Typography>
          <Grid container spacing={2}>
            {props.data.projectHealthRisks.map((hr) => (
              <Grid
                item
                xs={12}
                key={`projectsHealthRiskItem_${hr.healthRiskId}`}
              >
                <ProjectsOverviewHealthRiskItem
                  projectHealthRisk={hr}
                  rtl={useRtlDirection}
                />
              </Grid>
            ))}
          </Grid>
        </Grid>
      </Grid>
    </Fragment>
  );
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isFetching: state.projects.formFetching,
  data: state.projects.overviewData,
  isClosed: state.appData.siteMap.parameters.projectIsClosed,
  callingUserRoles: state.appData.user.roles,
});

const mapDispatchToProps = {
  openHealthRisksOverview: projectsActions.openHealthRisksOverview.invoke,
  goToList: projectsActions.goToList,
  openHealthRisksEdition: projectsActions.goToHealthRisksEdition,
};

export const ProjectHealthRisksPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectHealthRisksPageComponent),
);
