import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./nationalSocietiesConstants";
import * as actions from "./nationalSocietiesActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { push } from "connected-react-router";

export const nationalSocietiesSagas = () => [
  takeEvery(consts.GET_NATIONAL_SOCIETIES.INVOKE, getNationalSocieties),
  takeEvery(consts.OPEN_EDITION_NATIONAL_SOCIETY.INVOKE, openNationalSocietyEdition),
  takeEvery(consts.OPEN_NATIONAL_SOCIETY_DASHBOARD.INVOKE, openNationalSocietyDashboard),
  takeEvery(consts.OPEN_NATIONAL_SOCIETY_OVERVIEW.INVOKE, openNationalSocietyOverview),
  takeEvery(consts.EDIT_NATIONAL_SOCIETY.INVOKE, editNationalSociety),
  takeEvery(consts.CREATE_NATIONAL_SOCIETY.INVOKE, createNationalSociety),
  takeEvery(consts.REMOVE_NATIONAL_SOCIETY.INVOKE, removeNationalSociety)
];

function* getNationalSocieties(force) {
  const currentData = yield select(state => state.nationalSocieties.listData)

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
};

function* openNationalSocietyEdition({ path, params }) {
  yield put(actions.openEdition.request());
  try {
    const response = yield call(http.get, `/api/nationalSociety/${params.nationalSocietyId}/get`);

    yield put(appActions.openModule.invoke(path, {
      nationalSocietyCountry: response.value.countryName,
      nationalSocietyName: response.value.name,
      nationalSocietyId: response.value.id
    }));

    yield put(actions.openEdition.success(response.value));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
};

function* openNationalSocietyOverview({ path, params }) {
  yield put(actions.openOverview.request());
  try {
    const response = yield call(http.get, `/api/nationalSociety/${params.nationalSocietyId}/get`);

    yield put(appActions.openModule.invoke(path, {
      nationalSocietyCountry: response.value.countryName,
      nationalSocietyName: response.value.name,
      nationalSocietyId: response.value.id
    }));

    yield put(actions.openOverview.success(response.value));
  } catch (error) {
    yield put(actions.openOverview.failure(error.message));
  }
};

function* openNationalSocietyDashboard({ path, params }) {
  yield put(actions.openDashbaord.request());
  try {
    const response = yield call(http.get, `/api/nationalSociety/${params.nationalSocietyId}/get`);

    yield put(appActions.openModule.invoke(path, {
      nationalSocietyCountry: response.value.countryName,
      nationalSocietyName: response.value.name,
      nationalSocietyId: response.value.id
    }));

    yield put(actions.openDashbaord.success(response.value.name));
  } catch (error) {
    yield put(actions.openDashbaord.failure(error.message));
  }
};

function* createNationalSociety({ data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(http.post, "/api/nationalSociety/create", data);
    http.ensureResponseIsSuccess(response);
    yield put(actions.create.success(response.value));
    yield put(push(`/nationalsocieties/${response.value}`));
    yield put(appActions.showMessage("The National Society was added successfully"));
  } catch (error) {
    yield put(actions.create.failure(error.message));
  }
};

function* editNationalSociety({ data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(http.post, `/api/nationalSociety/${data.id}/edit`, data);
    http.ensureResponseIsSuccess(response);
    yield put(actions.edit.success(response.value));
    yield put(push(`/nationalsocieties/${data.id}/overview`));
  } catch (error) {
    yield put(actions.edit.failure(error.message));
  }
};

function* removeNationalSociety({ id }) {
  yield put(actions.remove.request(id));
  try {
    yield call(http.post, `/api/nationalSociety/${id}/remove`);
    yield put(actions.remove.success(id));
    yield call(getNationalSocieties, true);
  } catch (error) {
    yield put(actions.remove.failure(id, error.message));
  }
};