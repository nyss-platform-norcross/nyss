import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./smsGatewaysConstants";
import * as actions from "./smsGatewaysActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { stringKeys } from "../../../strings";

export const smsGatewaysSagas = () => [
  takeEvery(consts.OPEN_SMS_GATEWAYS_LIST.INVOKE, openSmsGatewaysList),
  takeEvery(consts.OPEN_SMS_GATEWAY_CREATION.INVOKE, openSmsGatewayCreation),
  takeEvery(consts.OPEN_SMS_GATEWAY_EDITION.INVOKE, openSmsGatewayEdition),
  takeEvery(consts.CREATE_SMS_GATEWAY.INVOKE, createSmsGateway),
  takeEvery(consts.EDIT_SMS_GATEWAY.INVOKE, editSmsGateway),
  takeEvery(consts.REMOVE_SMS_GATEWAY.INVOKE, removeSmsGateway),
  takeEvery(consts.PING_IOT_DEVICE.INVOKE, pingIotDevice),
  takeEvery(consts.LIST_AVAILABLE_IOT_DEVICES.INVOKE, listAvailableIotDevices),
];

function* openSmsGatewaysList({ nationalSocietyId }) {
  yield put(actions.openList.request());
  try {
    yield openSmsGatewaysModule(nationalSocietyId);

    if (yield select((state) => state.smsGateways.listStale)) {
      yield call(getSmsGateways, nationalSocietyId);
    }

    yield put(actions.openList.success(nationalSocietyId));
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
}

function* openSmsGatewayCreation({ nationalSocietyId }) {
  yield put(actions.openCreation.request());
  try {
    yield openSmsGatewaysModule(nationalSocietyId);
    yield put(actions.openCreation.success());
  } catch (error) {
    yield put(actions.openCreation.failure(error.message));
  }
}

function* openSmsGatewayEdition({ nationalSocietyId, smsGatewayId }) {
  yield put(actions.openEdition.request());
  try {
    const response = yield call(
      http.get,
      `/api/smsGateway/${smsGatewayId}/get`,
    );
    yield openSmsGatewaysModule(nationalSocietyId);
    yield put(actions.openEdition.success(response.value));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
}

function* createSmsGateway({ nationalSocietyId, data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(
      http.post,
      `/api/smsGateway/create?nationalSocietyId=${nationalSocietyId}`,
      data,
    );
    yield put(actions.create.success(response.value));
    yield put(actions.goToList(nationalSocietyId));
    yield put(appActions.showMessage(stringKeys.smsGateway.create.success));
  } catch (error) {
    yield put(actions.create.failure(error));
  }
}

function* editSmsGateway({ data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(
      http.post,
      `/api/smsGateway/${data.id}/edit`,
      data,
    );
    yield put(actions.edit.success(response.value));
    yield put(actions.goToList(data.nationalSocietyId));
    yield put(appActions.showMessage(stringKeys.smsGateway.edit.success));
  } catch (error) {
    yield put(actions.edit.failure(error));
  }
}

function* removeSmsGateway({ nationalSocietyId, smsGatewayId }) {
  yield put(actions.remove.request(smsGatewayId));
  try {
    yield call(http.post, `/api/smsGateway/${smsGatewayId}/delete`);
    yield put(actions.remove.success(smsGatewayId));
    yield call(getSmsGateways, nationalSocietyId);
    yield put(appActions.showMessage(stringKeys.smsGateway.delete.success));
  } catch (error) {
    yield put(actions.remove.failure(smsGatewayId, error.message));
  }
}

function* getSmsGateways(nationalSocietyId) {
  yield put(actions.getList.request());
  try {
    const response = yield call(
      http.get,
      `/api/smsGateway/list?nationalSocietyId=${nationalSocietyId}`,
    );
    yield put(actions.getList.success(response.value));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
}

function* pingIotDevice({ iotDeviceId }) {
  yield put(actions.pingIotDevice.request(iotDeviceId));
  try {
    const response = yield call(
      http.post,
      `api/smsGateway/iotDevices/${iotDeviceId}/ping`,
    );
    yield put(
      actions.pingIotDevice.success(iotDeviceId, {
        isSuccess: true,
        message: JSON.parse(response.value).data,
      }),
    );
  } catch (error) {
    yield put(actions.pingIotDevice.failure(iotDeviceId, error.message));
  }
}

function* listAvailableIotDevices() {
  yield put(actions.listAvailableIotDevices.request());
  try {
    const availableGateways = yield call(
      http.get,
      `/api/smsGateway/iotDevices/list`,
    );
    yield put(actions.listAvailableIotDevices.success(availableGateways.value));
  } catch (error) {
    yield put(actions.listAvailableIotDevices.failure(error.message));
  }
}

function* openSmsGatewaysModule(nationalSocietyId) {
  const nationalSociety = yield call(http.getCached, {
    path: `/api/nationalSociety/${nationalSocietyId}/get`,
    dependencies: [entityTypes.nationalSociety(nationalSocietyId)],
  });

  yield put(
    appActions.openModule.invoke(null, {
      nationalSocietyId: nationalSociety.value.id,
      nationalSocietyName: nationalSociety.value.name,
      nationalSocietyCountry: nationalSociety.value.countryName,
      nationalSocietyIsArchived: nationalSociety.value.isArchived,
      nationalSocietyHasCoordinator: nationalSociety.value.hasCoordinator,
    }),
  );
}
