import React, {useEffect, Fragment, useState} from 'react';
import { connect } from "react-redux";
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import { Loading } from '../common/loading/Loading';
import { useMount } from '../../utils/lifecycle';
import * as alertEventsActions from './logic/alertEventsActions'
import { AlertEventsTable } from './components/AlertEventsTable';
import { TableActionsButton } from "../common/tableActions/TableActionsButton";
import { accessMap } from "../../authentication/accessMap";
import { stringKeys, strings } from "../../strings";
import { CreateAlertEventDialog } from "./components/CreateAlertEventDialog";
import TableActions from "../common/tableActions/TableActions";
import AddIcon from "@material-ui/icons/Add";

const AlertEventsLogPageComponent = ({ alertId, projectId, data, ...props }) => {
  useMount(() => {
    props.openEventLog(projectId, alertId);
  });

  const [createDialogOpened, setCreateDialogOpened] = useState(false);

  useEffect(() => {
    if (!props.data) {
      return;
    }

  }, [props.data, props.match]);

  if (props.isFetching || !data) {
    return <Loading />;
  }

  return (
    <Fragment>
      <TableActions>
        <TableActionsButton
          onClick={() => setCreateDialogOpened(true)}
          variant="outlined"
          color="primary"
          roles={accessMap.alertEvents.add}
          icon={<AddIcon />}>
          {strings(stringKeys.alerts.logs.addNew)}
        </TableActionsButton>
      </TableActions>
      <AlertEventsTable
        list={data}
      />

      <CreateAlertEventDialog
        isOpened={createDialogOpened}
        close={() => setCreateDialogOpened(false)}
        openCreation={props.openCreation}
        create={props.create}
        alertId={alertId}
      />

    </Fragment>
  );
}

AlertEventsLogPageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  alertId: state.appData.siteMap.parameters.alertId,
  isFetching: state.alertEvents.logFetching,
  isSaving: state.alertEvents.formSaving,
  data: state.alertEvents.logItems
});

const mapDispatchToProps = {
  openEventLog: alertEventsActions.openEventLog.invoke,
  openCreation: alertEventsActions.openCreation.invoke,
  create: alertEventsActions.create.invoke,
  edit: alertEventsActions.edit.invoke
};

export const AlertEventsLogPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(AlertEventsLogPageComponent)
);
