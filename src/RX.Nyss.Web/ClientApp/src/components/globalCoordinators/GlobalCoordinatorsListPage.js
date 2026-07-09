import React, { Fragment } from "react";
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import * as globalCoordinatorsActions from "./logic/globalCoordinatorsActions";
import * as appActions from "../app/logic/appActions";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import TableActions from "../common/tableActions/TableActions";
import GlobalCoordinatorsTable from "./GlobalCoordinatorsTable";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { accessMap } from "../../authentication/accessMap";
import { trackPageView } from "../../utils/appInsightsHelper";

const GlobalCoordinatorsListPageComponent = (props) => {
  useMount(() => {
    props.openModule(props.match.path, props.match.params);
    props.getList();

    // Track page view
    trackPageView("GlobalCoordinatorsListPage");
  });

  const userLanguageCode = useSelector(
    (state) => state.appData.user.languageCode,
  );

  return (
    <Fragment>
      <TableActions>
        <TableActionsButton
          onClick={props.goToCreation}
          roles={accessMap.globalCoordinators.add}
          add
          variant="contained"
          rtl={userLanguageCode === "ar"}
        >
          {strings(stringKeys.common.buttons.add)}
        </TableActionsButton>
      </TableActions>

      <GlobalCoordinatorsTable
        list={props.list}
        goToEdition={props.goToEdition}
        goToDashboard={props.goToDashboard}
        isListFetching={props.isListFetching}
        isRemoving={props.isRemoving}
        remove={props.remove}
        rtl={userLanguageCode === "ar"}
      />
    </Fragment>
  );
};

GlobalCoordinatorsListPageComponent.propTypes = {
  getGlobalCoordinators: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

const mapStateToProps = (state) => ({
  list: state.globalCoordinators.listData,
  isListFetching: state.globalCoordinators.listFetching,
  isRemoving: state.globalCoordinators.listRemoving,
});

const mapDispatchToProps = {
  getList: globalCoordinatorsActions.getList.invoke,
  goToCreation: globalCoordinatorsActions.goToCreation,
  goToEdition: globalCoordinatorsActions.goToEdition,
  remove: globalCoordinatorsActions.remove.invoke,
  openModule: appActions.openModule.invoke,
};

export const GlobalCoordinatorsListPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(GlobalCoordinatorsListPageComponent),
);
