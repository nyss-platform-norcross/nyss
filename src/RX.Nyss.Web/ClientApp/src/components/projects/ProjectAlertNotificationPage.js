import { Grid, Divider } from "@material-ui/core";
import { Fragment } from "react"
import { connect } from "react-redux";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import { ProjectAlertNotHandledRecipientsPage } from "../projectAlertNotHandledRecipient/ProjectAlertNotHandledRecipientsPage";
import { ProjectAlertRecipientsListPage } from "../projectAlertRecipients/ProjectAlertRecipientsListPage";

export const ProjectAlertNotificationsComponent = ({ projectId }) => {

  return (
    <Fragment>
      <Grid container spacing={2}>
        <Grid item xs={12}>
          <ProjectAlertNotHandledRecipientsPage projectId={projectId} />
        </Grid>
        <Grid item xs={12}>
          <Divider />
        </Grid>
        <Grid item xs={12}>
          <ProjectAlertRecipientsListPage projectId={projectId} />
        </Grid>
      </Grid>
    </Fragment>
  );
}

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId
});
export const ProjectAlertNotificationsPage = withLayout(
  Layout,
  connect(mapStateToProps)(ProjectAlertNotificationsComponent)
);