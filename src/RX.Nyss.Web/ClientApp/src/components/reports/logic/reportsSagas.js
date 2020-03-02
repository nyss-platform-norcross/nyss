import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./reportsConstants";
import * as actions from "./reportsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { downloadFile } from "../../../utils/downloadFile";
import { stringKeys } from "../../../strings";
import { ReportListType } from '../../common/filters/logic/reportFilterConstsants'
import { DateColumnName } from './reportsConstants'

export const reportsSagas = () => [
  takeEvery(consts.OPEN_REPORTS_LIST.INVOKE, openReportsList),
  takeEvery(consts.GET_REPORTS.INVOKE, getReports),
  takeEvery(consts.OPEN_REPORT_EDITION.INVOKE, openReportEdition),
  takeEvery(consts.EDIT_REPORT.INVOKE, editReport),
  takeEvery(consts.EXPORT_TO_EXCEL.INVOKE, getExcelExportData),
  takeEvery(consts.EXPORT_TO_CSV.INVOKE, getCsvExportData),
  takeEvery(consts.MARK_AS_ERROR.INVOKE, markAsError)
];

function* openReportsList({ projectId }) {
  const listStale = yield select(state => state.reports.listStale);

  yield put(actions.openList.request());
  try {
    yield openReportsModule(projectId);

    const filtersData = yield call(http.get, `/api/report/filters?projectId=${projectId}`);

    if (listStale) {
      yield call(getReports, { projectId });
    }

    yield put(actions.openList.success(projectId, filtersData.value));
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
};

function* getReports({ projectId, pageNumber, filters, sorting }) {
  const requestFilters = filters || (yield select(state => state.reports.filters)) ||
  {
    reportsType: ReportListType.main,
    area: null,
    healthRiskId: null,
    status: true,
    isTraining: false
  };

  const requestSorting = sorting || (yield select(state => state.reports.sorting)) ||
  {
    orderBy: DateColumnName,
    sortAscending: false
  };

  const page = pageNumber || (yield select(state => state.reports.paginatedListData && state.reports.paginatedListData.page)) || 1;

  yield put(actions.getList.request());
  try {
    const response = yield call(http.post, `/api/report/list?projectId=${projectId}&pageNumber=${page}`, { ...requestFilters, ...requestSorting });
    http.ensureResponseIsSuccess(response);
    yield put(actions.getList.success(response.value.data, response.value.page, response.value.rowsPerPage, response.value.totalRows, requestFilters, requestSorting));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
};

function* openReportEdition({ projectId, reportId }) {
  yield put(actions.openEdition.request());
  try {
    const humanHealthRisksforProject = yield call(http.get, `/api/report/humanHealthRisksForProject/${projectId}/get`);
    const response = yield call(http.get, `/api/report/${reportId}/get`);
    yield openReportsModule(projectId);
    yield put(actions.openEdition.success(response.value, humanHealthRisksforProject.value.healthRisks));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
};

function* editReport({ projectId, reportId, data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(http.post, `/api/report/${reportId}/edit`, data);
    yield put(actions.edit.success(response.value));
    yield put(actions.goToList(projectId));
    yield put(appActions.showMessage(stringKeys.reports.list.editedSuccesfully));
  } catch (error) {
    yield put(actions.edit.failure(error.message));
  }
};

function* getCsvExportData({ projectId, filters, sorting }) {
  yield put(actions.exportToExcel.request());
  try {
    yield downloadFile({
      url: `/api/report/exportToCsv?projectId=${projectId}`,
      fileName: `reports.csv`,
      data: { ...filters, ...sorting }
    });

    yield put(actions.exportToExcel.success());
  } catch (error) {
    yield put(actions.exportToExcel.failure(error.message));
  }
};

function* getExcelExportData({ projectId, filters, sorting }) {
  yield put(actions.exportToExcel.request());
  try {
    yield downloadFile({
      url: `/api/report/exportToExcel?projectId=${projectId}`,
      fileName: `reports.xlsx`,
      data: { ...filters, ...sorting }
    });

    yield put(actions.exportToExcel.success());
  } catch (error) {
    yield put(actions.exportToExcel.failure(error.message));
  }
};

function* openReportsModule(projectId) {
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
}

function* markAsError({ reportId }) {
  const projectId = yield select(state => state.appData.route.params.projectId)

  yield put(actions.markAsError.request());
  try {
    yield call(http.post, `/api/report/${reportId}/markAsError`);
    yield put(actions.markAsError.success());
    yield put(appActions.showMessage(stringKeys.reports.list.successfulyMarkedAsError));
    yield call(getReports, { projectId });
  } catch (error) {
    yield put(actions.markAsError.failure());
  }
};

