import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./eidsrIntegrationConstants";
import * as actions from "./eidsrIntegrationActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import {push} from "connected-react-router";
import {stringKeys} from "../../../strings";

export const eidsrIntegrationSagas = () => [
  takeEvery(consts.GET_EIDSR_INTEGRATION.INVOKE, getEidsrIntegration),
  takeEvery(consts.EDIT_EIDSR_INTEGRATION.INVOKE, editEidsrIntegration),
];

function* getEidsrIntegration({ nationalSocietyId }) {
  yield put(actions.get.request());
  try {

    yield getNationalSocietyBaseInfo(nationalSocietyId);
    const eidsrConfigurationResponse = yield call(http.get, `/api/eidsrconfiguration/${nationalSocietyId}/get`);
    yield put(actions.get.success(eidsrConfigurationResponse.value));

  } catch (error) {
    yield put(actions.get.failure(error.message));
  }
};

function* editEidsrIntegration({ id, data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(http.post, `/api/eidsrconfiguration/${id}/edit`, data);
    yield put(actions.edit.success(response.value));

    yield put(push(`/nationalsocieties/${id}/eidsrintegration`));
    yield put(appActions.showMessage(stringKeys.eidsrIntegration.edit.success));
  } catch (error) {
    yield put(actions.edit.failure(error));
  }
};

// gets national society data to feed breadcrumb
function* getNationalSocietyBaseInfo(nationalSocietyId) {
  const nationalSociety = yield call(http.get, `/api/nationalSociety/${nationalSocietyId}/get`);

  yield put(appActions.openModule.invoke(null, {
    nationalSocietyId: nationalSociety.value.id,
    nationalSocietyName: nationalSociety.value.name,
    nationalSocietyCountry: nationalSociety.value.countryName,
    nationalSocietyIsArchived: nationalSociety.value.isArchived,
    nationalSocietyHasCoordinator: nationalSociety.value.hasCoordinator,
    nationalSocietyEnableEidsrIntegration: nationalSociety.value.enableEidsrIntegration,
  }));
};
