import React, { Fragment } from "react";
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import * as smsGatewaysActions from "./logic/smsGatewaysActions";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import TableActions from "../common/tableActions/TableActions";
import SmsGatewaysTable from "./SmsGatewaysTable";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { accessMap } from "../../authentication/accessMap";
import * as roles from "../../authentication/roles";
import { trackPageView } from "../../utils/appInsightsHelper";

const SmsGatewaysListPageComponent = (props) => {
  useMount(() => {
    props.openSmsGatewaysList(props.nationalSocietyId);

    // Track page view
    trackPageView("SmsGatewaysListPage");
  });

  const useRtlDirection = useSelector(
    (state) => state.appData.user.languageCode === "ar",
  );

  return (
    <Fragment>
      {!props.nationalSocietyIsArchived && (
        <TableActions>
          <TableActionsButton
            onClick={() => props.goToCreation(props.nationalSocietyId)}
            roles={accessMap.smsGateways.add}
            condition={
              !props.nationalSocietyHasCoordinator ||
              props.callingUserRoles.some(
                (r) => r === roles.Coordinator || r === roles.Administrator,
              )
            }
            add
            variant="contained"
            rtl={useRtlDirection}
          >
            {strings(stringKeys.common.buttons.add)}
          </TableActionsButton>
        </TableActions>
      )}

      <SmsGatewaysTable
        list={props.list}
        goToEdition={props.goToEdition}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        isRemoving={props.isRemoving}
        remove={props.remove}
        nationalSocietyId={props.nationalSocietyId}
        callingUserRoles={props.callingUserRoles}
        rtl={useRtlDirection}
      />
    </Fragment>
  );
};

SmsGatewaysListPageComponent.propTypes = {
  getSmsGateways: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  list: state.smsGateways.listData,
  isListFetching: state.smsGateways.listFetching,
  isRemoving: state.smsGateways.listRemoving,
  callingUserRoles: state.appData.user.roles,
  nationalSocietyIsArchived:
    state.appData.siteMap.parameters.nationalSocietyIsArchived,
  nationalSocietyHasCoordinator:
    state.appData.siteMap.parameters.nationalSocietyHasCoordinator,
});

const mapDispatchToProps = {
  openSmsGatewaysList: smsGatewaysActions.openList.invoke,
  goToCreation: smsGatewaysActions.goToCreation,
  goToEdition: smsGatewaysActions.goToEdition,
  remove: smsGatewaysActions.remove.invoke,
};

export const SmsGatewaysListPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(SmsGatewaysListPageComponent),
);
