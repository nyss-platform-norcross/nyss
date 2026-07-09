import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./nationalSocietiesConstants";
import { entityTypes } from "./nationalSocietiesConstants";
import * as actions from "./nationalSocietiesActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { push } from "connected-react-router";
import { stringKeys } from "../../../strings";

export const nationalSocietiesSagas = () => [
  takeEvery(consts.GET_NATIONAL_SOCIETIES.INVOKE, getNationalSocieties),
  takeEvery(
    consts.OPEN_EDITION_NATIONAL_SOCIETY.INVOKE,
    openNationalSocietyEdition,
  ),
  takeEvery(
    consts.OPEN_NATIONAL_SOCIETY_OVERVIEW.INVOKE,
    openNationalSocietyOverview,
  ),
  takeEvery(consts.EDIT_NATIONAL_SOCIETY.INVOKE, editNationalSociety),
  takeEvery(consts.CREATE_NATIONAL_SOCIETY.INVOKE, createNationalSociety),
  takeEvery(consts.ARCHIVE_NATIONAL_SOCIETY.INVOKE, archiveNationalSociety),
  takeEvery(consts.REOPEN_NATIONAL_SOCIETY.INVOKE, reopenNationalSociety),
];

function* getNationalSocieties(force) {
  const currentData = yield select((state) => state.nationalSocieties.listData);

  if (!force && currentData.length) {
    return;
  }

  yield put(actions.getList.request());
  try {
    const response = yield call(http.get, "/api/nationalSociety/list");
    yield put(actions.getList.success(response.value));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
}

function* openNationalSocietyEdition({ path, params }) {
  yield put(actions.openEdition.request());
  try {
    const response = yield call(
      http.get,
      `/api/nationalSociety/${params.nationalSocietyId}/get`,
    );

    yield put(
      appActions.openModule.invoke(path, {
        nationalSocietyCountry: response.value.countryName,
        nationalSocietyName: response.value.name,
        nationalSocietyId: response.value.id,
      }),
    );

    yield put(actions.openEdition.success(response.value));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
}

function* openNationalSocietyOverview({ path, params }) {
  yield put(actions.openOverview.request());
  try {
    const response = yield call(
      http.get,
      `/api/nationalSociety/${params.nationalSocietyId}/get`,
    );

    yield put(
      appActions.openModule.invoke(path, {
        nationalSocietyCountry: response.value.countryName,
        nationalSocietyName: response.value.name,
        nationalSocietyId: response.value.id,
        nationalSocietyIsArchived: response.value.isArchived,
        nationalSocietyHasCoordinator:
          response.value.nationalSocietyHasCoordinator,
      }),
    );

    yield put(actions.openOverview.success(response.value));
  } catch (error) {
    yield put(actions.openOverview.failure(error.message));
  }
}

function* createNationalSociety({ data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(http.post, "/api/nationalSociety/create", data);
    yield put(actions.create.success(response.value));
    yield put(push(`/nationalsocieties/${response.value}`));
    yield put(
      appActions.showMessage(
        stringKeys.nationalSociety.messages.create.success,
      ),
    );
  } catch (error) {
    yield put(actions.create.failure(error));
  }
}

function* editNationalSociety({ data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(
      http.post,
      `/api/nationalSociety/${data.id}/edit`,
      data,
    );
    yield put(actions.edit.success(response.value));
    yield put(appActions.entityUpdated(entityTypes.nationalSociety(data.id)));
    yield put(push(`/nationalsocieties/${data.id}/overview`));
    yield put(
      appActions.showMessage(stringKeys.nationalSociety.messages.edit.success),
    );
  } catch (error) {
    yield put(actions.edit.failure(error));
  }
}

function* archiveNationalSociety({ id }) {
  yield put(actions.archive.request(id));
  try {
    yield call(http.post, `/api/nationalSociety/${id}/archive`);
    yield put(actions.archive.success(id));
    yield put(appActions.entityUpdated(entityTypes.nationalSociety(id)));
    yield call(getNationalSocieties, true);
    yield put(
      appActions.showMessage(
        stringKeys.nationalSociety.messages.archive.success,
      ),
    );
  } catch (error) {
    yield put(actions.archive.failure(id, error.message));
  }
}

function* reopenNationalSociety({ id }) {
  yield put(actions.reopen.request(id));
  try {
    yield call(http.post, `/api/nationalSociety/${id}/reopen`);
    yield put(actions.reopen.success(id));
    yield put(appActions.entityUpdated(entityTypes.nationalSociety(id)));
    yield call(getNationalSocieties, true);
    yield put(
      appActions.showMessage(
        stringKeys.nationalSociety.messages.reopen.success,
      ),
    );
  } catch (error) {
    yield put(actions.reopen.failure(id, error.message));
  }
}
