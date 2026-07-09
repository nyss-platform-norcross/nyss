import React, { Fragment } from "react";
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import * as suspectedDiseaseActions from "./logic/suspectedDiseaseActions";
import * as appActions from "../app/logic/appActions";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import TableActions from "../common/tableActions/TableActions";
import SuspectedDiseaseTable from "./SuspectedDiseaseTable";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { trackPageView } from "../../utils/appInsightsHelper";

const SuspectedDiseaseListPageComponent = (props) => {
  useMount(() => {
    props.openModule(props.match.path, props.match.params);
    props.getList();

    // Track page view
    trackPageView("SuspectedDiseaseListPage");
  });

  const userLanguageCode = useSelector(
    (state) => state.appData.user.languageCode,
  );

  return (
    <Fragment>
      <TableActions>
        <TableActionsButton
          onClick={props.goToCreation}
          add
          variant={"contained"}
          rtl={userLanguageCode === "ar"}
        >
          {strings(stringKeys.common.buttons.add)}
        </TableActionsButton>
      </TableActions>

      <SuspectedDiseaseTable
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

SuspectedDiseaseListPageComponent.propTypes = {
  getSuspectedDisease: PropTypes.func,
  goToCreation: PropTypes.func,
  goToEdition: PropTypes.func,
  remove: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

const mapStateToProps = (state) => ({
  list: state.suspectedDiseases.listData,
  isListFetching: state.suspectedDiseases.listFetching,
  isRemoving: state.suspectedDiseases.listRemoving,
});

const mapDispatchToProps = {
  getList: suspectedDiseaseActions.getList.invoke,
  goToCreation: suspectedDiseaseActions.goToCreation,
  goToEdition: suspectedDiseaseActions.goToEdition,
  remove: suspectedDiseaseActions.remove.invoke,
  openModule: appActions.openModule.invoke,
};

export const SuspectedDiseaseListPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(SuspectedDiseaseListPageComponent),
);
