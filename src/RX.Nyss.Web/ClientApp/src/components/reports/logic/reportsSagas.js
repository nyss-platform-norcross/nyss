import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./reportsConstants";
import * as actions from "./reportsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import {downloadFile} from "../../../utils/downloadFile";
import { stringKeys } from "../../../strings";
import { ReportListType } from '../../common/filters/logic/reportFilterConstsants'
import { DateColumnName } from './reportsConstants'

export const reportsSagas = () => [
  takeEvery(consts.OPEN_REPORTS_LIST.INVOKE, openReportsList),
  takeEvery(consts.GET_REPORTS.INVOKE, getReports),
  takeEvery(consts.EXPORT_TO_EXCEL.INVOKE, getExportData),
  takeEvery(consts.MARK_AS_ERROR.INVOKE, markAsError)
];

function* openReportsList({ projectId }) {
  const listStale = yield select(state => state.reports.listStale);

  yield put(actions.openList.request());
  try {
    yield openReportsModule(projectId);

    const filtersData = yield call(http.get, `/api/report/filters?projectId=${projectId}`);
    const filters = (yield select(state => state.reports.filters)) ||
    {
      reportsType: ReportListType.main,
      area: null,
      healthRiskId: null,
      status: true,
      isTraining: false
    };
    const sorting = (yield select(state => state.reports.sorting)) ||
    {
      orderBy: DateColumnName,
      sortAscending: false
    };

    if (listStale) {
      yield call(getReports, { projectId, filters, sorting });
    }

    yield put(actions.openList.success(projectId, filtersData.value));
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
};

function* getReports({ projectId, pageNumber, filters, sorting }) {
  yield put(actions.getList.request());
  try {
    const response = yield call(http.post, `/api/report/list?projectId=${projectId}&pageNumber=${pageNumber || 1}`, { ...filters, ...sorting });
    http.ensureResponseIsSuccess(response);
    yield put(actions.getList.success(response.value.data, response.value.page, response.value.rowsPerPage, response.value.totalRows, filters, sorting));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
};

function* getExportData({ projectId, filters, sorting }) {
  yield put(actions.exportToExcel.request());
  try {
    yield downloadFile({
      url: `/api/report/exportToExcel?projectId=${projectId}`,
      fileName: `reports.csv`,
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
    projectName: project.value.name
  }));
}

function* markAsError({ reportId, projectId, pageNumber, reportListFilter }) {
  yield put(actions.markAsError.request());
  try {    
    yield call(http.post, `/api/report/${reportId}/markAsError`);
    yield put(actions.markAsError.success());
    yield put(appActions.showMessage(stringKeys.reports.list.successfulyMarkedAsError));
        
    yield call(getReports, {projectId, pageNumber, reportListFilter});
  } catch (error) {
    yield put(actions.markAsError.failure());
  }
};

