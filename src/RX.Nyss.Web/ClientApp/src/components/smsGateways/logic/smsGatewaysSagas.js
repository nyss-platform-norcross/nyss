import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./smsGatewaysConstants";
import * as actions from "./smsGatewaysActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";

export const smsGatewaysSagas = () => [
  takeEvery(consts.OPEN_SMS_GATEWAYS_LIST.INVOKE, openSmsGatewaysList),
  takeEvery(consts.GET_SMS_GATEWAYS.INVOKE, getSmsGateways),
  takeEvery(consts.OPEN_CREATION_SMS_GATEWAY.INVOKE, openSmsGatewayCreation),
  takeEvery(consts.CREATE_SMS_GATEWAY.INVOKE, createSmsGateway),
  takeEvery(consts.OPEN_EDITION_SMS_GATEWAY.INVOKE, openSmsGatewayEdition),
  takeEvery(consts.EDIT_SMS_GATEWAY.INVOKE, editSmsGateway),
  takeEvery(consts.REMOVE_SMS_GATEWAY.INVOKE, removeSmsGateway)
];

function* openSmsGatewaysList({ path, params }) {
  yield put(actions.openList.request());
  try {
    yield call(getSmsGateways, false, params.nationalSocietyId);
    const nationalSocietyResponse = yield call(http.get, `/api/nationalSociety/${params.nationalSocietyId}/get`);

    yield put(appActions.openModule.invoke(path, {
      nationalSocietyId: nationalSocietyResponse.value.id,
      nationalSocietyName: nationalSocietyResponse.value.name,
      nationalSocietyCountry: nationalSocietyResponse.value.countryName
    }));

    yield put(actions.openList.success());
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
};

function* getSmsGateways(force, nationalSocietyId) {
  const currentData = yield select(state => state.smsGateways.listData)

  if (!force && currentData.length) {
    return;
  }

  yield put(actions.getList.request());
  try {
    const response = yield call(http.get, `/api/smsGateway/list/nationalSociety/${nationalSocietyId}`);

    yield put(actions.getList.success(response.value));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
};

function* openSmsGatewayCreation({ path, params }) {
  yield put(actions.openCreation.request());
  try {
    const nationalSocietyResponse = yield call(http.get, `/api/nationalSociety/${params.nationalSocietyId}/get`);

    yield put(appActions.openModule.invoke(path, {
      nationalSocietyId: nationalSocietyResponse.value.id,
      nationalSocietyName: nationalSocietyResponse.value.name,
      nationalSocietyCountry: nationalSocietyResponse.value.countryName
    }));

    yield put(actions.openCreation.success());
  } catch (error) {
    yield put(actions.openCreation.failure(error.message));
  }
};

function* createSmsGateway({ nationalSocietyId, data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(http.post, `/api/smsGateway/add/nationalSociety/${nationalSocietyId}`, data);
    http.ensureResponseIsSuccess(response);
    yield put(actions.create.success(response.value));
    yield put(actions.goToList(nationalSocietyId));
    yield put(appActions.showMessage("The SMS Gateway was added successfully"));
  } catch (error) {
    yield put(actions.create.failure(error.message));
  }
};

function* openSmsGatewayEdition({ path, params }) {
  yield put(actions.openEdition.request());
  try {
    const response = yield call(http.get, `/api/smsGateway/${params.smsGatewayId}/get`);
    const nationalSocietyResponse = yield call(http.get, `/api/nationalSociety/${params.nationalSocietyId}/get`);

    yield put(appActions.openModule.invoke(path, {
      nationalSocietyId: nationalSocietyResponse.value.id,
      nationalSocietyName: nationalSocietyResponse.value.name,
      nationalSocietyCountry: nationalSocietyResponse.value.countryName
    }));

    yield put(actions.openEdition.success(response.value));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
};

function* editSmsGateway({ data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(http.post, `/api/smsGateway/${data.id}/edit`, data);
    http.ensureResponseIsSuccess(response);
    yield put(actions.edit.success(response.value));
    yield put(actions.goToList(data.nationalSocietyId));
  } catch (error) {
    yield put(actions.edit.failure(error.message));
  }
};

function* removeSmsGateway({ smsGatewayId, nationalSocietyId }) {
  yield put(actions.remove.request(smsGatewayId));
  try {
    yield call(http.post, `/api/smsGateway/${smsGatewayId}/remove`);
    yield put(actions.remove.success(smsGatewayId));
    yield call(getSmsGateways, true, nationalSocietyId);
  } catch (error) {
    yield put(actions.remove.failure(smsGatewayId, error.message));
  }
};