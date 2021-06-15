import {call, put, takeEvery} from "redux-saga/effects";
import * as actions from "../../alertEvents/logic/alertEventsActions";
import * as http from "../../../utils/http";
import * as appActions from "../../app/logic/appActions";
import {stringKeys, strings} from "../../../strings";
import * as consts from "../../alertEvents/logic/alertEventsConstants";
import {getUtcOffset} from "../../../utils/date";
import dayjs from "dayjs";
import {entityTypes} from "../../nationalSocieties/logic/nationalSocietiesConstants";

export const alertEventsSagas = () => {
  return [
    takeEvery(consts.OPEN_ALERT_EVENT_LOG.INVOKE, openEventLog),
    takeEvery(consts.GET_ALERT_EVENT_LOG.INVOKE, getEventLog),
    takeEvery(consts.OPEN_ALERT_EVENT_CREATION.INVOKE, openAlertEventCreation),
    takeEvery(consts.CREATE_ALERT_EVENT.INVOKE, createAlertEvent),
    takeEvery(consts.EDIT_ALERT_EVENT.INVOKE, editAlertEvent),
  ];
}

function* openEventLog ({ projectId, alertId }) {
    yield put(actions.openEventLog.request());
  try {
    const response = yield call(http.get, `/api/alertEvents/${alertId}/eventLog?utcOffset=${getUtcOffset()}`);
    const data = response.value;

    const title = `${strings(stringKeys.alerts.logs.title, true)} - ${data.healthRisk} ${dayjs(data.date).format('YYYY-MM-DD HH:mm')}`;

    yield openAlertEventsModule(projectId, title);

    yield put(actions.openEventLog.success(alertId, data));
  } catch (error) {
    yield put(actions.openEventLog.failure(error.message));
  }
};

function* getEventLog ({alertId}) {
  yield put(actions.getEventLog.request());
  try {
    const response = yield call(http.get, `/api/alertEvents/${alertId}/eventLog?utcOffset=${getUtcOffset()}`);
    const data = response.value;

    yield put(actions.getEventLog.success(data));
  } catch (error) {
    yield put(actions.getEventLog.failure(error.message));
  }
};

function* openAlertEventCreation() {
  yield put(actions.openCreation.request());
  try {
    const response = yield call(http.get, `/api/alertEvents/eventLog/formData`);

    yield put(actions.openCreation.success(response.value.eventTypes, response.value.eventSubtypes));
  } catch (error) {
    yield put(actions.openCreation.failure(error.message))
  }
};

function* createAlertEvent({ alertId, data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(http.post, `/api/alertEvents/${alertId}/eventLog/add`, data);
    yield call(getEventLog, {alertId})
    yield put(actions.create.success(response.value));
    yield put(appActions.showMessage(response.message.key));
  } catch (error) {
    yield put(actions.create.failure(error.message));
  }
};

function* editAlertEvent({ alertId, alertEventLogId, text }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(http.post, `/api/alertEvents/${alertId}/eventLog/edit/${alertEventLogId}`,
      {alertEventLogId, text});
    yield call(getEventLog, {alertId})
    yield put(actions.edit.success(response.value));

    yield put(appActions.showMessage(response.message.key));
  } catch (error) {
    yield put(actions.edit.failure(error.message));
  }
};

function* openAlertEventsModule(projectId, title) {
  const project = yield call(http.getCached, {
    path: `/api/project/${projectId}/basicData`,
    dependencies: [entityTypes.project(projectId)]
  });

  yield put(appActions.openModule.invoke(null, {
    nationalSocietyId: project.value.nationalSociety.id,
    nationalSocietyName: project.value.nationalSociety.name,
    nationalSocietyCountry: project.value.nationalSociety.countryName,
    projectId: project.value.id,
    projectName: project.value.name,
    title: title,
    projectIsClosed: project.value.isClosed
  }));
}