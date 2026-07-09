import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./alertsConstants";
import * as actions from "./alertsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { stringKeys } from "../../../strings";
import dayjs from "dayjs";
import { downloadFile } from "../../../utils/downloadFile";
import { formatDate, getUtcOffset} from "../../../utils/date";

// Helper to ensure Locations is always a valid AreaDto object (required by API)
const normalizeLocations = (locations) => {
  if (!locations || typeof locations !== "object") {
    return {
      regionIds: [],
      districtIds: [],
      villageIds: [],
      zoneIds: [],
      includeUnknownLocation: true,
    };
  }
  return locations;
};

const getDefaultFilters = () => {
  const localMidnight = dayjs()
    .set("hour", 0)
    .set("minute", 0)
    .set("second", 0)
    .set("millisecond", 0);

  return {
    locations: normalizeLocations(null),
    healthRiskId: null,
    startDate: localMidnight.add(-30, "day"),
    endDate: localMidnight,
    status: consts.alertStatusFilters.all,
    orderBy: consts.statusColumn,
    sortAscending: false,
    utcOffset: getUtcOffset(),
  };
};

export const alertsSagas = () => [
  takeEvery(consts.OPEN_ALERTS_LIST.INVOKE, openAlertsList),
  takeEvery(consts.GET_ALERTS.INVOKE, getAlerts),
  takeEvery(consts.OPEN_ALERTS_ASSESSMENT.INVOKE, openAlertsAssessment),
  takeEvery(consts.ACCEPT_REPORT.INVOKE, acceptReport),
  takeEvery(consts.DISMISS_REPORT.INVOKE, dismissReport),
  takeEvery(consts.ESCALATE_ALERT.INVOKE, escalateAlert),
  takeEvery(consts.DISMISS_ALERT.INVOKE, dismissAlert),
  takeEvery(consts.CLOSE_ALERT.INVOKE, closeAlert),
  takeEvery(consts.RESET_REPORT.INVOKE, resetReport),
  takeEvery(consts.FETCH_RECIPIENTS.INVOKE, fetchRecipients),
  takeEvery(consts.EXPORT_ALERTS.INVOKE, exportAlerts),
  takeEvery(consts.VALIDATE_EIDSR.INVOKE, validateEidsr),
];

function* openAlertsList({ projectId, listState }) {
  yield put(actions.openList.request());
  try {
    yield call(openAlertsModule, projectId);

    const alertsState = yield select((state) => state.alerts);
    const hasCachedList =
      alertsState.listProjectId === projectId &&
      !!alertsState.listData &&
      !!alertsState.filters &&
      !!alertsState.filtersData;

    if (hasCachedList) {
      const cachedPage = alertsState.listData.page;
      const requestedPage = listState?.page;
      if (requestedPage && requestedPage !== cachedPage) {
        yield call(getAlerts, {
          projectId,
          pageNumber: requestedPage,
          filters: alertsState.filters,
        });
      }
      yield put(actions.openList.success(projectId, alertsState.filtersData));
      return;
    }

    const filtersData = yield call(
      http.get,
      `/api/alert/getFiltersData?projectId=${projectId}`,
    );

    const filters = getDefaultFilters();
    const initialPage = listState?.page || 1;
    yield call(getAlerts, { projectId, pageNumber: initialPage, filters });

    yield put(actions.openList.success(projectId, filtersData.value));
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
}

function* getAlerts({ projectId, pageNumber, filters }) {
  yield put(actions.getAlerts.request());
  try {
    const normalizedFilters = {
      ...filters,
      locations: normalizeLocations(filters?.locations),
    };
    const response = yield call(
      http.post,
      `/api/alert/list?projectId=${projectId}&pageNumber=${pageNumber || 1}`,
      normalizedFilters,
    );
    yield put(
      actions.getAlerts.success(
        response.value.data,
        response.value.page,
        response.value.rowsPerPage,
        response.value.totalRows,
        normalizedFilters,
      ),
    );
  } catch (error) {
    yield put(actions.getAlerts.failure(error.message));
  }
}

function* fetchRecipients({ alertId }) {
  yield put(actions.fetchRecipients.request());
  try {
    const response = yield call(
      http.get,
      `/api/alert/${alertId}/alertRecipients`,
    );
    yield put(actions.fetchRecipients.success(response.value));
  } catch (error) {
    yield put(actions.fetchRecipients.failure(error.message));
  }
}

function* fetchAssessmentData(alertId) {
  return yield call(
    http.get,
    `/api/alert/${alertId}/get?utcOffset=${getUtcOffset()}`,
  );
}

function* refreshAssessment(alertId) {
  const data = yield call(fetchAssessmentData, alertId);
  yield put(actions.openAssessment.success(alertId, data));
}

function* refreshAlertsListIfCached(projectId) {
  const alertsState = yield select((state) => state.alerts);
  const hasCachedList =
    alertsState.listProjectId === projectId &&
    !!alertsState.listData &&
    !!alertsState.filters;

  if (!hasCachedList) {
    return;
  }

  yield call(getAlerts, {
    projectId,
    pageNumber: alertsState.listData.page,
    filters: alertsState.filters,
  });
}

function* refreshAssessmentAndList(projectId, alertId) {
  try {
    yield call(refreshAssessment, alertId);
    yield call(refreshAlertsListIfCached, projectId);
  } catch (error) {
    // The mutation succeeded; keep the optimistic update if refresh fails.
  }
}

function* openAlertsAssessment({ projectId, alertId }) {
  yield put(actions.openAssessment.request());
  try {
    const data = yield call(fetchAssessmentData, alertId);

    const title = `${data.healthRisk}`;
    const subTitle = `${dayjs(data.createdAt).format("YYYY-MM-DD HH:mm")}`;

    yield openAlertsModule(projectId, title, subTitle);
    yield put(actions.openAssessment.success(alertId, data));
  } catch (error) {
    yield put(actions.openAssessment.failure(error.message));
  }
}

function* acceptReport({ alertId, reportId }) {
  const previousAssessmentStatus = yield select(
    (state) => state.alerts.formData.assessmentStatus,
  );
  const projectId = yield select(
    (state) => state.appData.route.params.projectId,
  );

  yield put(actions.acceptReport.request(reportId));
  try {
    const response = yield call(
      http.post,
      `/api/alert/${alertId}/acceptReport?reportId=${reportId}`,
    );
    const newAssessmentStatus = response.value.assessmentStatus;
    yield put(actions.acceptReport.success(reportId, newAssessmentStatus));
    yield call(refreshAssessmentAndList, projectId, alertId);

    if (
      previousAssessmentStatus !== newAssessmentStatus &&
      newAssessmentStatus === consts.assessmentStatus.toEscalate
    ) {
      yield put(
        appActions.showMessage(
          stringKeys.alerts.assess.alert.escalationPossible,
          20000,
        ),
      );
    }
  } catch (error) {
    yield put(
      appActions.showMessage(
        stringKeys.alerts.assess.alert.dismissalPossible,
        20000,
      ),
    );
    yield put(actions.acceptReport.failure(reportId, error.message));
  }
}

function* dismissReport({ alertId, reportId }) {
  const previousAssessmentStatus = yield select(
    (state) => state.alerts.formData.assessmentStatus,
  );
  const projectId = yield select(
    (state) => state.appData.route.params.projectId,
  );

  yield put(actions.dismissReport.request(reportId));
  try {
    const response = yield call(
      http.post,
      `/api/alert/${alertId}/dismissReport?reportId=${reportId}`,
    );
    const newAssessmentStatus = response.value.assessmentStatus;
    yield put(actions.dismissReport.success(reportId, newAssessmentStatus));
    yield call(refreshAssessmentAndList, projectId, alertId);

    if (
      previousAssessmentStatus !== newAssessmentStatus &&
      newAssessmentStatus === consts.assessmentStatus.toDismiss
    ) {
      yield put(
        appActions.showMessage(
          stringKeys.alerts.assess.alert.dismissalPossible,
          20000,
        ),
      );
    }
  } catch (error) {
    yield put(actions.dismissReport.failure(reportId, error.message));
  }
}

function* resetReport({ alertId, reportId }) {
  const projectId = yield select(
    (state) => state.appData.route.params.projectId,
  );

  yield put(actions.resetReport.request(reportId));
  try {
    const response = yield call(
      http.post,
      `/api/alert/${alertId}/resetReport?reportId=${reportId}`,
    );
    const newAssessmentStatus = response.value.assessmentStatus;
    yield put(actions.resetReport.success(reportId, newAssessmentStatus));
    yield call(refreshAssessmentAndList, projectId, alertId);
  } catch (error) {
    yield put(actions.resetReport.failure(reportId, error.message));
  }
}

function* escalateAlert({ alertId, sendNotification }) {
  const projectId = yield select(
    (state) => state.appData.route.params.projectId,
  );

  yield put(actions.escalateAlert.request());
  try {
    const response = yield call(http.post, `/api/alert/${alertId}/escalate`, {
      sendNotification,
    });
    yield put(actions.escalateAlert.success());
    yield call(refreshAssessmentAndList, projectId, alertId);
    yield put(appActions.showMessage(response.value));
  } catch (error) {
    yield put(appActions.showMessage(error.message));
    yield put(actions.escalateAlert.failure(error.message));
  }
}

function* dismissAlert({ alertId }) {
  const projectId = yield select(
    (state) => state.appData.route.params.projectId,
  );

  yield put(actions.dismissAlert.request());
  try {
    yield call(http.post, `/api/alert/${alertId}/dismiss`);
    yield put(actions.dismissAlert.success());
    yield call(refreshAlertsListIfCached, projectId);
    yield put(actions.goToList(projectId));
    yield put(
      appActions.showMessage(
        stringKeys.alerts.assess.alert.dismissedSuccessfully,
      ),
    );
  } catch (error) {
    yield put(appActions.showMessage(error.message));
    yield put(actions.dismissAlert.failure(error.message));
  }
}

function* closeAlert({ alertId }) {
  const projectId = yield select(
    (state) => state.appData.route.params.projectId,
  );

  yield put(actions.closeAlert.request());
  try {
    yield call(http.post, `/api/alert/${alertId}/close`);
    yield put(actions.closeAlert.success());
    yield put(actions.goToEventLog(projectId, alertId));
    yield put(
      appActions.showMessage(stringKeys.alerts.assess.alert.closedSuccessfully),
    );
  } catch (error) {
    yield put(appActions.showMessage(error.message));
    yield put(actions.closeAlert.failure(error.message));
  }
}

function* exportAlerts({ projectId, filters }) {
  yield put(actions.exportAlerts.request());

  try {
    const date = new Date(Date.now());
    yield downloadFile({
      url: `/api/alert/export?projectId=${projectId}`,
      fileName: `alerts_${formatDate(date)}.xlsx`,
      data: { ...filters },
    });

    yield put(actions.exportAlerts.success());
  } catch (error) {
    yield put(actions.exportAlerts.failure(error.message));
  }
}

function* openAlertsModule(projectId, title, subTitle) {
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
      title: title,
      subTitle: subTitle,
      projectIsClosed: project.value.isClosed,
    }),
  );
}

function* validateEidsr({ alertId }) {
  yield put(actions.validateEidsr.request());
  try {
    const response = yield call(
      http.get,
      `/api/alert/${alertId}/eidsr-validate`,
    );
    yield put(actions.validateEidsr.success(response.value));
  } catch (error) {
    yield put(actions.validateEidsr.failure(error.message));
  }
}
