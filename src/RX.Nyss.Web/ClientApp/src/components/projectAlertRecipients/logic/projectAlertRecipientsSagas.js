import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./projectAlertRecipientsConstants";
import * as actions from "./projectAlertRecipientsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { stringKeys } from "../../../strings";

export const projectAlertRecipientsSagas = () => [
  takeEvery(
    consts.OPEN_ALERT_RECIPIENTS_LIST.INVOKE,
    openProjectAlertRecipientsList,
  ),
  takeEvery(
    consts.OPEN_ALERT_RECIPIENT_CREATION.INVOKE,
    openAlertRecipientCreation,
  ),
  takeEvery(
    consts.OPEN_ALERT_RECIPIENT_EDITION.INVOKE,
    openAlertRecipientEdition,
  ),
  takeEvery(consts.CREATE_ALERT_RECIPIENT.INVOKE, createAlertRecipient),
  takeEvery(consts.EDIT_ALERT_RECIPIENT.INVOKE, editAlertRecipient),
  takeEvery(consts.REMOVE_ALERT_RECIPIENT.INVOKE, removeAlertRecipient),
];

function* openProjectAlertRecipientsList({ projectId }) {
  yield put(actions.openList.request());
  try {
    yield openProjectAlertRecipientsModule(projectId);

    if (yield select((state) => state.projectAlertRecipients.listStale)) {
      yield call(getProjectAlertRecipients, projectId);
    }

    yield put(actions.openList.success(projectId));
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
}

function* openAlertRecipientCreation({ projectId }) {
  yield put(actions.openCreation.request());
  try {
    yield openProjectAlertRecipientsModule(projectId);
    if (yield select((state) => state.projectAlertRecipients.listStale)) {
      yield call(getProjectAlertRecipients, projectId);
    }
    const formData = yield call(
      http.get,
      `/api/projectAlertRecipient/formData?projectId=${projectId}`,
    );
    yield put(actions.openCreation.success(formData.value));
  } catch (error) {
    yield put(actions.openCreation.failure(error.message));
  }
}

function* createAlertRecipient({ projectId, data }) {
  yield put(actions.create.request());
  try {
    const nationalSocietyId = yield select(
      (state) => state.appData.siteMap.parameters.nationalSocietyId,
    );
    const response = yield call(
      http.post,
      `/api/projectAlertRecipient/create?nationalSocietyId=${nationalSocietyId}&projectId=${projectId}`,
      data,
    );
    yield put(actions.create.success(response.value));
    yield put(actions.goToList(projectId));
    yield put(
      appActions.showMessage(stringKeys.projectAlertRecipient.create.success),
    );
  } catch (error) {
    yield put(actions.create.failure(error.message));
  }
}

function* openAlertRecipientEdition({ alertRecipientId }) {
  yield put(actions.openEdition.request());
  try {
    const projectId = yield select(
      (state) => state.appData.route.params.projectId,
    );
    const recipient = yield call(
      http.get,
      `/api/projectAlertRecipient/${alertRecipientId}/get`,
    );
    yield openProjectAlertRecipientsModule(projectId);
    const formData = yield call(
      http.get,
      `/api/projectAlertRecipient/formData?projectId=${projectId}`,
    );
    yield put(actions.openEdition.success(recipient.value, formData.value));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
}

function* editAlertRecipient({ projectId, data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(
      http.post,
      `/api/projectAlertRecipient/${data.id}/edit`,
      data,
    );
    yield put(actions.edit.success(response.value));
    yield put(actions.goToList(projectId));
    yield put(
      appActions.showMessage(stringKeys.projectAlertRecipient.edit.success),
    );
  } catch (error) {
    yield put(actions.edit.failure(error.message));
  }
}

function* removeAlertRecipient({ projectId, alertRecipientId }) {
  yield put(actions.remove.request(alertRecipientId));
  try {
    yield call(
      http.post,
      `/api/projectAlertRecipient/${alertRecipientId}/delete`,
    );
    yield put(actions.remove.success(alertRecipientId));
    yield call(getProjectAlertRecipients, projectId);
    yield put(
      appActions.showMessage(stringKeys.projectAlertRecipient.delete.success),
    );
  } catch (error) {
    yield put(actions.remove.failure(alertRecipientId, error.message));
  }
}

function* getProjectAlertRecipients(projectId) {
  yield put(actions.getList.request());
  try {
    const nationalSocietyId = yield select(
      (state) => state.appData.siteMap.parameters.nationalSocietyId,
    );
    const response = yield call(
      http.get,
      `/api/projectAlertRecipient/list?nationalSocietyId=${nationalSocietyId}&projectId=${projectId}`,
    );
    yield put(actions.getList.success(response.value));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
}

function* openProjectAlertRecipientsModule(projectId) {
  const project = yield call(http.getCached, {
    path: `/api/project/${projectId}/basicData`,
    dependencies: [entityTypes.project(projectId)],
  });

  yield put(
    appActions.openModule.invoke(null, {
      nationalSocietyId: project.value.nationalSociety.id,
      nationalSocietyName: project.value.nationalSociety.name,
      nationalSocietyCountry: project.value.nationalSociety.countryName,
      projectId: project.value.id,
      projectName: project.value.name,
      projectIsClosed: project.value.isClosed,
      allowMultipleOrganizations: project.value.allowMultipleOrganizations,
    }),
  );
}
