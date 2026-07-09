import React, { useEffect, Fragment, useState } from "react";
import { connect, useSelector } from "react-redux";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import { Loading } from "../common/loading/Loading";
import { useMount } from "../../utils/lifecycle";
import * as alertEventsActions from "./logic/alertEventsActions";
import { AlertEventsTable } from "./components/AlertEventsTable";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { accessMap } from "../../authentication/accessMap";
import { stringKeys, strings } from "../../strings";
import { CreateAlertEventDialog } from "./components/CreateAlertEventDialog";
import { Grid, Typography } from "@material-ui/core";
import { AlertStatusChip } from "../common/chip/AlertStatusChip";
import * as alertsActions from "../alerts/logic/alertsActions";
import { trackPageView } from "../../utils/appInsightsHelper";
import { TabMenu } from "../layout/TabMenu";

const AlertEventsLogPageComponent = ({
  alertId,
  projectId,
  data,
  alert,
  ...props
}) => {
  const useRtlDirection = useSelector(
    (state) => state.appData.direction === "rtl",
  );
  const [createDialogOpened, setCreateDialogOpened] = useState(false);

  useMount(() => {
    props.openEventLog(projectId, alertId);
    props.openAssessment(projectId, alertId);

    // Track page view
    trackPageView("AlertEventsLogPage");
  });

  useEffect(() => {
    if (!props.data) {
      return;
    }
  }, [props.data, props.match]);

  if (props.isFetching || !data || !alert) {
    return <Loading />;
  }
  return (
    <Fragment>
      <Grid
        container
        justifyContent="space-between"
        style={{ marginBottom: 10 }}
      >
        <Grid item style={{ width: "100%" }}>
          <Grid container alignItems="center">
            <Typography
              variant="h3"
              style={{ marginRight: 10 }}
            >
              {props.title}
            </Typography>
            <Typography
              variant="body1"
              style={{ alignSelf: "center", marginRight: 15 }}
            >{`#${alertId}`}</Typography>
            <AlertStatusChip status={alert.assessmentStatus} />
          </Grid>
          <Typography variant="body1">
            {props.subTitle}
          </Typography>
          <TabMenu/>
        </Grid>
      </Grid>
      <Grid container justifyContent="flex-end" style={{ marginBottom: 10 }}>
        <TableActionsButton
          onClick={() => setCreateDialogOpened(true)}
          variant="contained"
          roles={accessMap.alertEvents.add}
          add
          rtl={useRtlDirection}
          >
          {strings(stringKeys.common.buttons.add)}
        </TableActionsButton>
      </Grid>
      <AlertEventsTable
        alertId={alertId}
        list={data}
        alertEventLogId={props.alertEventLogId}
        isRemoving={props.isRemoving}
        remove={props.remove}
        edit={props.edit}
      />

      {createDialogOpened && (
        <CreateAlertEventDialog
          close={() => setCreateDialogOpened(false)}
          openCreation={props.openCreation}
          create={props.create}
          alertId={alertId}
        />
      )}
    </Fragment>
  );
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  alertId: ownProps.match.params.alertId,
  isFetching: state.alertEvents.logFetching,
  isSaving: state.alertEvents.formSaving,
  isRemoving: state.alertEvents.logRemoving,
  data: state.alertEvents.logItems,
  alert: state.alerts.formData,
  title: state.appData.siteMap.parameters.title,
  subTitle: state.appData.siteMap.parameters.subTitle,
});

const mapDispatchToProps = {
  openAssessment: alertsActions.openAssessment.invoke,
  openEventLog: alertEventsActions.openEventLog.invoke,
  openCreation: alertEventsActions.openCreation.invoke,
  create: alertEventsActions.create.invoke,
  remove: alertEventsActions.remove.invoke,
  edit: alertEventsActions.edit.invoke,
};

export const AlertEventsLogPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(AlertEventsLogPageComponent),
);
