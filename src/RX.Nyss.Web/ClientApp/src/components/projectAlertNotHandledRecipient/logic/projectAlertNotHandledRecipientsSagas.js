import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./projectAlertNotHandledRecipientsConstants";
import * as actions from "./projectAlertNotHandledRecipientsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { apiUrl } from "../../../utils/variables";

export const projectAlertNotHandledRecipientsSagas = () => [
  takeEvery(consts.OPEN_ALERT_NOT_HANDLED_RECIPIENTS.INVOKE, openProjectAlertNotHandledRecipients),
  takeEvery(consts.CREATE_ALERT_NOT_HANDLED_RECIPIENT.INVOKE, createAlertNotHandledRecipient),
  takeEvery(consts.EDIT_ALERT_NOT_HANDLED_RECIPIENT.INVOKE, editAlertNotHandledRecipient),
  takeEvery(consts.GET_ALERT_NOT_HANDLED_FORM_DATA.INVOKE, getProjectAlertNotHandledRecipientsFormData)
];

function* openProjectAlertNotHandledRecipients({ projectId }) {
  yield put(actions.openRecipients.request());
  try {
    if (yield select(state => state.projectAlertNotHandledRecipients.listStale)) {
      yield call(getProjectAlertNotHandledRecipients, projectId);
    }

    yield put(actions.openRecipients.success(projectId));
  } catch (error) {
    yield put(actions.openRecipients.failure(error.message));
  }
};

function* createAlertNotHandledRecipient({ projectId, data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(http.post, `${apiUrl}/api/projectAlertNotHandledRecipient/create?projectId=${projectId}`, data);
    yield put(actions.create.success(response.value));
    yield put(appActions.showMessage(response.message.key));
    yield call(getProjectAlertNotHandledRecipients, projectId);
  } catch (error) {
    yield put(actions.create.failure(error.message));
  }
};

function* editAlertNotHandledRecipient({ projectId, data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(http.post, `${apiUrl}/api/projectAlertNotHandledRecipient/edit?projectId=${projectId}`, data);
    yield put(actions.edit.success(response.value));
    yield put(appActions.showMessage(response.message.key));
    yield call(getProjectAlertNotHandledRecipients, projectId);
  } catch (error) {
    yield put(actions.edit.failure(error.message));
  }
};

function* getProjectAlertNotHandledRecipients(projectId) {
  yield put(actions.getRecipients.request());
  try {
    const response = yield call(http.get, `${apiUrl}/api/projectAlertNotHandledRecipient/list?projectId=${projectId}`);
    yield put(actions.getRecipients.success(response.value));
  } catch (error) {
    yield put(actions.getRecipients.failure(error.message));
  }
};

function* getProjectAlertNotHandledRecipientsFormData({ projectId }) {
  yield put(actions.getFormData.request());
  try {
    const response = yield call(http.getCached, {
      path: `${apiUrl}/api/projectAlertNotHandledRecipient/formData?projectId=${projectId}`,
      dependencies: entityTypes.project(projectId)
    });
    yield put(actions.getFormData.success(response.value));
  } catch (error) {
    yield put(actions.getFormData.failure(error.message));
  }
}
