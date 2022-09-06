import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./eidsrIntegrationConstants";
import * as actions from "./eidsrIntegrationActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { stringKeys } from "../../../strings";
import { delay } from 'redux-saga/effects'

export const eidsrIntegrationSagas = () => [
  takeEvery(consts.GET_EIDSR_INTEGRATION.INVOKE, getEidsrIntegration),
];

function* getEidsrIntegration({ nationalSocietyId }) {
  yield delay(2000); // spinner generator
  yield put(actions.get.request());
  try {
    yield getNationalSocietyBaseInfo(nationalSocietyId);
    const eidsrIntegration =  { id: 145, userName: "ewa", apiBaseUrl: null } ;
    yield put(actions.get.success(eidsrIntegration));

  } catch (error) {
    yield put(actions.get.failure(error.message));
  }
};

// gets national society data to feed breadcrumb
function* getNationalSocietyBaseInfo(nationalSocietyId) {
  const nationalSociety = yield call(http.getCached, {
    path: `/api/nationalSociety/${nationalSocietyId}/get`,
    dependencies: [entityTypes.nationalSociety(nationalSocietyId)]
  });

  yield put(appActions.openModule.invoke(null, {
    nationalSocietyId: nationalSociety.value.id,
    nationalSocietyName: nationalSociety.value.name,
    nationalSocietyCountry: nationalSociety.value.countryName,
    nationalSocietyIsArchived: nationalSociety.value.isArchived,
    nationalSocietyHasCoordinator: nationalSociety.value.hasCoordinator
  }));
};