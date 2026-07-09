import React, { Fragment, useMemo } from "react";
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import * as organizationsActions from "./logic/organizationsActions";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import TableActions from "../common/tableActions/TableActions";
import OrganizationsTable from "./OrganizationsTable";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { useCallback } from "react";
import * as roles from "../../authentication/roles";
import { trackPageView } from "../../utils/appInsightsHelper";

const OrganizationsListPageComponent = (props) => {
  useMount(() => {
    props.openOrganizationsList(props.nationalSocietyId);

    // Track page view
    trackPageView("OrganizationsListPage");
  });

  const useRtlDirection = useSelector(
    (state) => state.appData.user.languageCode === "ar",
  );

  const hasAnyRole = useCallback(
    (...roles) =>
      props.callingUserRoles.some((userRole) =>
        roles.some((role) => role === userRole),
      ),
    [props.callingUserRoles],
  );

  const canModify = useMemo(
    () =>
      !props.nationalSocietyIsArchived &&
      (hasAnyRole(roles.Administrator, roles.Coordinator) ||
        (!props.nationalSocietyHasCoordinator &&
          props.isCurrentUserHeadManager)),
    [
      props.nationalSocietyHasCoordinator,
      props.isCurrentUserHeadManager,
      props.nationalSocietyIsArchived,
      hasAnyRole,
    ],
  );

  return (
    <Fragment>
      {canModify && (
        <TableActions>
          <TableActionsButton
            onClick={() => props.goToCreation(props.nationalSocietyId)}
            variant="contained"
            add
            rtl={useRtlDirection}
          >
            {strings(stringKeys.common.buttons.add)}
          </TableActionsButton>
        </TableActions>
      )}

      <OrganizationsTable
        list={props.list}
        goToEdition={props.goToEdition}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        isRemoving={props.isRemoving}
        remove={props.remove}
        nationalSocietyId={props.nationalSocietyId}
        canModify={canModify}
        rtl={useRtlDirection}
      />
    </Fragment>
  );
};

OrganizationsListPageComponent.propTypes = {
  getOrganizations: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  list: state.organizations.listData,
  isListFetching: state.organizations.listFetching,
  isRemoving: state.organizations.listRemoving,
  nationalSocietyIsArchived:
    state.appData.siteMap.parameters.nationalSocietyIsArchived,
  nationalSocietyHasCoordinator:
    state.appData.siteMap.parameters.nationalSocietyHasCoordinator,
  isCurrentUserHeadManager:
    state.appData.siteMap.parameters.isCurrentUserHeadManager,
  callingUserRoles: state.appData.user.roles,
});

const mapDispatchToProps = {
  openOrganizationsList: organizationsActions.openList.invoke,
  goToCreation: organizationsActions.goToCreation,
  goToEdition: organizationsActions.goToEdition,
  remove: organizationsActions.remove.invoke,
};

export const OrganizationsListPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(OrganizationsListPageComponent),
);
