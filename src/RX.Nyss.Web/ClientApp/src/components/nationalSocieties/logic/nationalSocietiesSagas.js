import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./nationalSocietiesConstants";
import * as actions from "./nationalSocietiesActions";
import * as appActions from "../../app/logic/appActions";
import * as httpMock from "../../../utils/httpMock";
import { push } from "connected-react-router";

export const nationalSocietiesSagas = () => [
  takeEvery(consts.GET_NATIONAL_SOCIETIES.INVOKE, getNationalSocieties),
  takeEvery(consts.OPEN_EDITION_NATIONAL_SOCIETY.INVOKE, openNationalSocietyEdition),
  takeEvery(consts.EDIT_NATIONAL_SOCIETY.INVOKE, editNationalSociety),
  takeEvery(consts.CREATE_NATIONAL_SOCIETY.INVOKE, createNationalSociety),
  takeEvery(consts.REMOVE_NATIONAL_SOCIETY.INVOKE, removeNationalSociety)
];

function* getNationalSocieties() {
  const currentData = yield select(state => state.nationalSocieties.listData)

  if (currentData.length) {
    return;
  }

  yield put(actions.getList.request());
  try {
    const response = yield call(httpMock.get, "/api/nationalSocieties/get");

    yield put(actions.getList.success(response.value));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
};

function* openNationalSocietyEdition({ path, params }) {
  yield put(actions.openEdition.request());
  try {
    const response = yield call(httpMock.get, `/api/nationalSocieties/${params.nationalSocietyId}/get`);

    yield put(appActions.openModule.invoke(path, {
      nationalSocietyCountry: response.value.country,
      nationalSocietyName: response.value.name
    }));

    yield put(actions.openEdition.success(response.value));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
};

function* createNationalSociety(data) {
  yield put(actions.create.request());
  try {
    const response = yield call(httpMock.post, "/api/nationalSocieties/create", data);
    yield put(actions.create.success(response.value));
    yield put(push("/nationalsocieties"));
  } catch (error) {
    yield put(actions.create.failure(error.message));
  }
};

function* editNationalSociety(data) {
  yield put(actions.edit.request());
  try {
    const response = yield call(httpMock.post, `/api/nationalSocieties/${data.id}/edit`, data);
    yield put(actions.edit.success(response.value));
    yield put(push("/nationalsocieties"));
  } catch (error) {
    yield put(actions.edit.failure(error.message));
  }
};

function* removeNationalSociety({ id }) {
  yield put(actions.remove.request(id));
  try {
    const response = yield call(httpMock.post, `/api/nationalSocieties/1/remove`);
    yield put(actions.remove.success(id));
  } catch (error) {
    yield put(actions.remove.failure(id, error.message));
  }
};