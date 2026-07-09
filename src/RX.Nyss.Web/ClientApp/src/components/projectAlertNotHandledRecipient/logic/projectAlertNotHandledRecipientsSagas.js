import { call, put, takeEvery } from "redux-saga/effects";
import * as consts from "./projectAlertNotHandledRecipientsConstants";
import * as actions from "./projectAlertNotHandledRecipientsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";

export const projectAlertNotHandledRecipientsSagas = () => [
  takeEvery(
    consts.OPEN_ALERT_NOT_HANDLED_RECIPIENTS.INVOKE,
    openProjectAlertNotHandledRecipients,
  ),
  takeEvery(
    consts.CREATE_ALERT_NOT_HANDLED_RECIPIENT.INVOKE,
    createAlertNotHandledRecipient,
  ),
  takeEvery(
    consts.EDIT_ALERT_NOT_HANDLED_RECIPIENT.INVOKE,
    editAlertNotHandledRecipient,
  ),
  takeEvery(
    consts.REMOVE_ALERT_NOT_HANDLED_RECIPIENT.INVOKE,
    removeAlertNotHandledRecipient,
  ),
  takeEvery(
    consts.GET_ALERT_NOT_HANDLED_FORM_DATA.INVOKE,
    getProjectAlertNotHandledRecipientsFormData,
  ),
];

function* openProjectAlertNotHandledRecipients({ projectId }) {
  yield put(actions.openRecipients.request());
  try {
    yield getProjectAlertNotHandledRecipients(projectId);

    yield put(actions.openRecipients.success(projectId));
  } catch (error) {
    yield put(actions.openRecipients.failure(error.message));
  }
}

function* createAlertNotHandledRecipient({ projectId, data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(
      http.post,
      `/api/projectAlertNotHandledRecipient/create?projectId=${projectId}`,
      data,
    );
    yield put(actions.create.success(response.value));
    yield put(appActions.showMessage(response.message.key));
    yield call(getProjectAlertNotHandledRecipients, projectId);
  } catch (error) {
    yield put(actions.create.failure(error.message));
  }
}

function* editAlertNotHandledRecipient({ projectId, data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(
      http.post,
      `/api/projectAlertNotHandledRecipient/edit?projectId=${projectId}`,
      { recipients: data },
    );
    yield put(actions.edit.success(response.value));
    yield put(appActions.showMessage(response.message.key));
    yield call(getProjectAlertNotHandledRecipients, projectId);
  } catch (error) {
    yield put(actions.edit.failure(error.message));
  }
}

function* removeAlertNotHandledRecipient({ projectId, data }) {
  yield put(actions.remove.request());
  try {
    const response = yield call(
      http.post,
      `/api/projectAlertNotHandledRecipient/delete?projectId=${projectId}`,
      data,
    );
    yield put(actions.remove.success(response.value));
    yield put(appActions.showMessage(response.message.key));
    yield call(getProjectAlertNotHandledRecipients, projectId);
  } catch (error) {
    yield put(actions.remove.failure(error.message));
  }
}

function* getProjectAlertNotHandledRecipients(projectId) {
  yield put(actions.getRecipients.request());
  try {
    const response = yield call(
      http.get,
      `/api/projectAlertNotHandledRecipient/list?projectId=${projectId}`,
    );
    yield put(actions.getRecipients.success(response.value));

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
  } catch (error) {
    yield put(actions.getRecipients.failure(error.message));
  }
}

function* getProjectAlertNotHandledRecipientsFormData({ projectId }) {
  yield put(actions.getFormData.request());
  try {
    const response = yield call(
      http.get,
      `/api/projectAlertNotHandledRecipient/formData?projectId=${projectId}`,
    );
    yield put(actions.getFormData.success(response.value));
  } catch (error) {
    yield put(actions.getFormData.failure(error.message));
  }
}
