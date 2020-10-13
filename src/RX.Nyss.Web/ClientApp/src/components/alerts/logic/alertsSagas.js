import { call, put, takeEvery, select, delay } from "redux-saga/effects";
import * as consts from "./alertsConstants";
import * as actions from "./alertsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { strings, stringKeys } from "../../../strings";
import dayjs from "dayjs";

export const alertsSagas = () => [
  takeEvery(consts.OPEN_ALERTS_LIST.INVOKE, openAlertsList),
  takeEvery(consts.GET_ALERTS.INVOKE, getAlerts),
  takeEvery(consts.OPEN_ALERTS_ASSESSMENT.INVOKE, openAlertsAssessment),
  takeEvery(consts.OPEN_ALERTS_LOGS.INVOKE, openAlertsLogs),
  takeEvery(consts.ACCEPT_REPORT.INVOKE, acceptReport),
  takeEvery(consts.DISMISS_REPORT.INVOKE, dismissReport),
  takeEvery(consts.ESCALATE_ALERT.INVOKE, escalateAlert),
  takeEvery(consts.DISMISS_ALERT.INVOKE, dismissAlert),
  takeEvery(consts.CLOSE_ALERT.INVOKE, closeAlert),
  takeEvery(consts.RESET_REPORT.INVOKE, resetReport),
  takeEvery(consts.FETCH_RECIPIENTS.INVOKE, fetchRecipients),
  takeEvery(consts.REFRESH_ALERT_STATUS.INVOKE, refreshAlertStatus)
];

function* openAlertsList({ projectId }) {
  const listProjectId = yield select(state => state.alerts.listProjectId);

  yield put(actions.openList.request());
  try {
    yield openAlertsModule(projectId);

    const filtersData = listProjectId !== projectId
      ? (yield call(http.get, `/api/alert/getFiltersData?projectId=${projectId}`)).value
      : yield select(state => state.alerts.filtersData);

    const filters = (yield select(state => state.alerts.filters)) ||
      { area: null, healthRiskId: null, status: consts.alertStatusFilters.all, orderBy: consts.statusColumn, sortAscending: false };

    yield call(getAlerts, { projectId, filters });

    yield put(actions.openList.success(projectId, filtersData));

  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
};

function* getAlerts({ projectId, pageNumber, filters }) {
  yield put(actions.getList.request());
  try {
    const response = yield call(http.post, `/api/alert/list?projectId=${projectId}&pageNumber=${pageNumber || 1}`, filters);
    yield put(actions.getList.success(response.value.data, response.value.page, response.value.rowsPerPage, response.value.totalRows, filters));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
};

function* fetchRecipients({ alertId }) {
  yield put(actions.fetchRecipients.request());
  try {
    const response = yield call(http.get, `/api/alert/${alertId}/alertRecipients`);
    yield put(actions.fetchRecipients.success(response.value))
  } catch (error) {
    yield put(actions.fetchRecipients.failure(error.message));
  }
}

function* openAlertsAssessment({ projectId, alertId }) {
  yield put(actions.openAssessment.request());
  try {
    const response = yield call(http.get, `/api/alert/${alertId}/get`);
    const data = response.value;

    const title = `${strings(stringKeys.alerts.details.title, true)} - ${data.healthRisk} ${dayjs(data.createdAt).format('YYYY-MM-DD HH:mm')}`;

    yield openAlertsModule(projectId, title);
    yield put(actions.openAssessment.success(alertId, data));
  } catch (error) {
    yield put(actions.openAssessment.failure(error.message));
  }
};

function* openAlertsLogs({ projectId, alertId }) {
  yield put(actions.openLogs.request());
  try {
    const response = yield call(http.get, `/api/alert/${alertId}/getLogs`);
    const data = response.value;

    const title = `${strings(stringKeys.alerts.details.title, true)} - ${data.healthRisk} ${dayjs(data.createdAt).format('YYYY-MM-DD HH:mm')}`;

    yield openAlertsModule(projectId, title);
    yield put(actions.openLogs.success(alertId, data));
  } catch (error) {
    yield put(actions.openLogs.failure(error.message));
  }
};

function* acceptReport({ alertId, reportId }) {
  const previousAssessmentStatus = yield select(state => state.alerts.formData.assessmentStatus);

  yield put(actions.acceptReport.request(reportId));
  try {
    const response = yield call(http.post, `/api/alert/${alertId}/acceptReport?reportId=${reportId}`);
    const newAssessmentStatus = response.value.assessmentStatus;
    yield put(actions.acceptReport.success(reportId, newAssessmentStatus));

    if (previousAssessmentStatus !== newAssessmentStatus && newAssessmentStatus === consts.assessmentStatus.toEscalate) {
      yield put(appActions.showMessage(stringKeys.alerts.assess.alert.escalationPossible, 20000));
    }
  } catch (error) {
    yield put(appActions.showMessage(stringKeys.alerts.assess.alert.dismissalPossible, 20000));
    yield put(actions.acceptReport.failure(reportId, error.message));
  }
};

function* dismissReport({ alertId, reportId }) {
  const previousAssessmentStatus = yield select(state => state.alerts.formData.assessmentStatus);

  yield put(actions.dismissReport.request(reportId));
  try {
    const response = yield call(http.post, `/api/alert/${alertId}/dismissReport?reportId=${reportId}`);
    const newAssessmentStatus = response.value.assessmentStatus;
    yield put(actions.dismissReport.success(reportId, newAssessmentStatus));

    if (previousAssessmentStatus !== newAssessmentStatus && newAssessmentStatus === consts.assessmentStatus.toDismiss) {
      yield put(appActions.showMessage(stringKeys.alerts.assess.alert.dismissalPossible, 20000));
    }
  } catch (error) {
    yield put(actions.dismissReport.failure(reportId, error.message));
  }
};

function* resetReport({ alertId, reportId }) {
  yield put(actions.resetReport.request(reportId));
  try {
    const response = yield call(http.post, `/api/alert/${alertId}/resetReport?reportId=${reportId}`);
    const newAssessmentStatus = response.value.assessmentStatus;
    yield put(actions.resetReport.success(reportId, newAssessmentStatus));

    yield call(refreshAlertStatus, {alertId});
  } catch (error) {
    yield put(actions.resetReport.failure(reportId, error.message));
  }
};

function* escalateAlert({ alertId, sendNotification }) {
  const projectId = yield select(state => state.appData.route.params.projectId);

  yield put(actions.escalateAlert.request());
  try {
    const response = yield call(http.post, `/api/alert/${alertId}/escalate`, { sendNotification });
    yield put(actions.escalateAlert.success());
    yield put(actions.goToList(projectId))
    yield put(appActions.showMessage(response.value));
  } catch (error) {
    yield put(appActions.showMessage(error.message));
    yield put(actions.escalateAlert.failure(error.message));
  }
};

function* dismissAlert({ alertId }) {
  const projectId = yield select(state => state.appData.route.params.projectId);

  yield put(actions.dismissAlert.request());
  try {
    yield call(http.post, `/api/alert/${alertId}/dismiss`);
    yield put(actions.dismissAlert.success());
    yield put(actions.goToList(projectId))
    yield put(appActions.showMessage(stringKeys.alerts.assess.alert.dismissedSuccessfully));
  } catch (error) {
    yield put(appActions.showMessage(error.message));
    yield put(actions.dismissAlert.failure(error.message));
  }
};

function* closeAlert({ alertId, comments, closeOption }) {
  const projectId = yield select(state => state.appData.route.params.projectId);

  yield put(actions.closeAlert.request());
  try {
    yield call(http.post, `/api/alert/${alertId}/close`, { comments, closeOption });
    yield put(actions.closeAlert.success());
    yield put(actions.goToList(projectId))
    yield put(appActions.showMessage(stringKeys.alerts.assess.alert.closedSuccessfully));
  } catch (error) {
    yield put(appActions.showMessage(error.message));
    yield put(actions.closeAlert.failure(error.message));
  }
};

function* openAlertsModule(projectId, title) {
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

function* refreshAlertStatus({ alertId }) {
  yield put(actions.refreshAlertStatus.request());
  try {
    yield delay(3000);
    const response = yield call(http.get, `/api/alert/${alertId}/get`);
    const data = response.value;

    yield put(actions.refreshAlertStatus.success(data));
  } catch (error) {
    yield put(actions.refreshAlertStatus.failure(error.message));
  }
}