import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./projectDashboardConstants";
import * as actions from "./projectDashboardActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import dayjs from "dayjs";
import utc from "dayjs/plugin/utc";
import { stringsFormat, stringKeys } from "../../../strings";
import { generatePdfDocument } from "../../../utils/pdf";
import { trackTrace } from "../../../utils/tracking";

dayjs.extend(utc);

export const projectDashboardSagas = () => [
  takeEvery(consts.OPEN_PROJECT_DASHBOARD.INVOKE, openProjectDashboard),
  takeEvery(consts.GET_PROJECT_DASHBOARD_DATA.INVOKE, getProjectDashboardData),
  takeEvery(consts.GET_PROJECT_DASHBOARD_REPORT_HEALTH_RISKS.INVOKE, getProjectDashboardReportHealthRisks),
  takeEvery(consts.GENERATE_PDF.INVOKE, generatePdf)
];

function* openProjectDashboard({ projectId }) {
  yield put(actions.openDashboard.request());
  try {
    yield call(openProjectDashboardModule, projectId);
    const filtersData = yield call(http.get, `/api/projectDashboard/filters?projectId=${projectId}`);
    const localDate = dayjs();
    const utcOffset = Math.floor(localDate.utcOffset() / 60);
    let endDate = localDate.add(-utcOffset, 'hour');
    endDate = endDate.set('hour', 0);
    endDate = endDate.set('minute', 0);
    endDate = endDate.set('second', 0);
    const filters = (yield select(state => state.projectDashboard.filters)) ||
      {
        healthRisks: [],
        locations: null,
        startDate: endDate.add(-7, 'day'),
        endDate: endDate,
        groupingType: 'Day',
        dataCollectorType: 'all',
        reportStatus: {
          kept: true,
          dismissed: false,
          notCrossChecked: true,
        },
        utcOffset: utcOffset,
        trainingStatus: 'Trained',
      };

    yield call(getProjectDashboardData, { projectId, filters })

    yield put(actions.openDashboard.success(projectId, filtersData.value));
  } catch (error) {
    yield put(actions.openDashboard.failure(error.message));
  }
};

function* getProjectDashboardData({ projectId, filters }) {
  yield put(actions.getDashboardData.request());
  try {
    const response = yield call(http.post, `/api/projectDashboard/data?projectId=${projectId}`, filters);
    yield put(actions.getDashboardData.success(
      filters,
      response.value.summary,
      response.value.reportsGroupedByHealthRiskAndDate,
      response.value.reportsGroupedByFeaturesAndDate,
      response.value.reportsGroupedByVillageAndDate,
      response.value.reportsGroupedByFeatures,
      response.value.reportsGroupedByLocation,
      response.value.dataCollectionPointReportsGroupedByDate
    ));
  } catch (error) {
    yield put(actions.getDashboardData.failure(error.message));
  }
};

function* getProjectDashboardReportHealthRisks({ projectId, latitude, longitude }) {
  yield put(actions.getReportHealthRisks.request());
  try {
    const filters = yield select(state => state.projectDashboard.filters);
    const response = yield call(http.post, `/api/projectDashboard/reportHealthRisks?projectId=${projectId}&latitude=${latitude}&longitude=${longitude}`, filters);
    yield put(actions.getReportHealthRisks.success(response.value));
  } catch (error) {
    yield put(actions.getReportHealthRisks.failure(error.message));
  }
};

function* generatePdf({ containerElement }) {
  const reportFileName = "Report";

  const projectId = yield select(state => state.appData.route.params.projectId);
  const message = "Generate project dashboard pdf";
  const properties = {
    projectId,
  };

  yield call(trackTrace, { message, properties });
  yield put(actions.generatePdf.request());
  try {
    const siteMapParams = yield select(state => state.appData.siteMap.parameters);

    const printTitleParams = {
      nationalSocietyName: siteMapParams.nationalSocietyName,
      projectName: siteMapParams.projectName,
    }

    const title = stringsFormat(stringKeys.dashboard.printTitle, printTitleParams, true);

    yield call(generatePdfDocument, title, containerElement, reportFileName);
    yield put(actions.generatePdf.success());
  } catch (error) {
    yield put(actions.generatePdf.failure(error.message));
  }
};

function* openProjectDashboardModule(projectId) {
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
    projectIsClosed: project.value.isClosed
  }));

  return project.value;
}
