import React, {useEffect, Fragment, useState} from 'react';
import { connect, useSelector } from "react-redux";
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import { Loading } from '../common/loading/Loading';
import { useMount } from '../../utils/lifecycle';
import * as alertEventsActions from './logic/alertEventsActions'
import { AlertEventsTable } from './components/AlertEventsTable';
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { accessMap } from "../../authentication/accessMap";
import { stringKeys, strings } from "../../strings";
import { CreateAlertEventDialog } from "./components/CreateAlertEventDialog";
import TableActions from "../common/tableActions/TableActions";

const AlertEventsLogPageComponent = ({ alertId, projectId, data, ...props }) => {
  const useRtlDirection = useSelector(state => state.appData.direction === 'rtl');
  const [createDialogOpened, setCreateDialogOpened] = useState(false);

  useMount(() => {
    props.openEventLog(projectId, alertId);
  });

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
          variant="contained"
          roles={accessMap.alertEvents.add}
          add
          rtl={useRtlDirection}
        >
          {strings(stringKeys.common.buttons.add)}
        </TableActionsButton>
      </TableActions>

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
}

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  alertId: ownProps.match.params.alertId,
  isFetching: state.alertEvents.logFetching,
  isSaving: state.alertEvents.formSaving,
  isRemoving: state.alertEvents.logRemoving,
  data: state.alertEvents.logItems
});

const mapDispatchToProps = {
  openEventLog: alertEventsActions.openEventLog.invoke,
  openCreation: alertEventsActions.openCreation.invoke,
  create: alertEventsActions.create.invoke,
  remove: alertEventsActions.remove.invoke,
  edit: alertEventsActions.edit.invoke
};

export const AlertEventsLogPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(AlertEventsLogPageComponent)
);
