import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./reportsConstants";
import * as actions from "./reportsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { downloadFile } from "../../../utils/downloadFile";
import { stringKeys } from "../../../strings";
import { ReportErrorFilterType, DataCollectorType } from '../../common/filters/logic/reportFilterConstsants'
import { DateColumnName } from './reportsConstants'
import { formatDate, getUtcOffset } from "../../../utils/date";

export const reportsSagas = () => [
  takeEvery(consts.OPEN_CORRECT_REPORTS_LIST.INVOKE, openCorrectReportsList),
  takeEvery(consts.OPEN_INCORRECT_REPORTS_LIST.INVOKE, openIncorrectReportsList),
  takeEvery(consts.GET_CORRECT_REPORTS.INVOKE, getCorrectReports),
  takeEvery(consts.GET_INCORRECT_REPORTS.INVOKE, getIncorrectReports),
  takeEvery(consts.OPEN_REPORT_EDITION.INVOKE, openReportEdition),
  takeEvery(consts.EDIT_REPORT.INVOKE, editReport),
  takeEvery(consts.EXPORT_TO_EXCEL.INVOKE, getExcelExportData),
  takeEvery(consts.EXPORT_TO_CSV.INVOKE, getCsvExportData),
  takeEvery(consts.MARK_AS_ERROR.INVOKE, markAsError),
  takeEvery(consts.OPEN_SEND_REPORT.INVOKE, openSendReport),
  takeEvery(consts.SEND_REPORT.INVOKE, sendReport),
  takeEvery(consts.ACCEPT_REPORT.INVOKE, acceptReport),
  takeEvery(consts.DISMISS_REPORT.INVOKE, dismissReport)
];

function* openCorrectReportsList({ projectId }) {
  const listStale = yield select(state => state.reports.correctReportsListStale);

  yield put(actions.openCorrectReportsList.request());
  try {
    yield openReportsModule(projectId);

    const filtersData = yield call(http.get, `/api/report/filters?projectId=${projectId}`);

    if (listStale) {
      yield call(getCorrectReports, { projectId });
    }

    yield put(actions.openCorrectReportsList.success(projectId, filtersData.value));
  } catch (error) {
    yield put(actions.openCorrectReportsList.failure(error.message));
  }
};

function* openIncorrectReportsList({ projectId }) {
  const listStale = yield select(state => state.reports.incorrectReportsListStale);

  yield put(actions.openIncorrectReportsList.request());
  try {
    yield openReportsModule(projectId);

    if (listStale) {
      yield call(getIncorrectReports, { projectId });
    }

    yield put(actions.openIncorrectReportsList.success(projectId));
  } catch (error) {
    yield put(actions.openIncorrectReportsList.failure(error.message));
  }
};

function* getCorrectReports({ projectId, pageNumber, filters, sorting }) {
  const requestFilters = filters || (yield select(state => state.reports.correctReportsFilters)) ||
  {
    dataCollectorType: DataCollectorType.human,
    errorType: null,
    area: null,
    healthRiskId: null,
    formatCorrect: true,
    isTraining: false,
    reportStatus: {
      kept: true,
      dismissed: true,
      notCrossChecked: true,
    },
    reportType: null,
    utcOffset: getUtcOffset(),
    dataCollectorStatus: "Trained",
  };

  const requestSorting = sorting || (yield select(state => state.reports.correctReportsSorting)) ||
  {
    orderBy: DateColumnName,
    sortAscending: false
  };

  const page = pageNumber || (yield select(state => state.reports.correctReportsPaginatedListData && state.reports.correctReportsPaginatedListData.page)) || 1;

  yield put(actions.getCorrectList.request());
  try {
    const response = yield call(http.post, `/api/report/list?projectId=${projectId}&pageNumber=${page}`, { ...requestFilters, ...requestSorting });
    yield put(actions.getCorrectList.success(response.value.data, response.value.page, response.value.rowsPerPage, response.value.totalRows, requestFilters, requestSorting));
  } catch (error) {
    yield put(actions.getCorrectList.failure(error.message));
  }
};

function* getIncorrectReports({ projectId, pageNumber, filters, sorting }) {
  const requestFilters = filters || (yield select(state => state.reports.incorrectReportsFilters)) ||
  {
    dataCollectorType: DataCollectorType.human,
    errorType: ReportErrorFilterType.all,
    area: null,
    healthRiskId: null,
    formatCorrect: false,
    isTraining: false,
    reportStatus: null,
    reportType: {
      real: true,
      corrected: false
    },
    utcOffset: getUtcOffset(),
    dataCollectorStatus: "Trained",
  };
  const requestSorting = sorting || (yield select(state => state.reports.incorrectReportsSorting)) ||
  {
    orderBy: DateColumnName,
    sortAscending: false
  };

  const page = pageNumber || (yield select(state => state.reports.incorrectReportsPaginatedListData && state.reports.incorrectReportsPaginatedListData.page)) || 1;

  yield put(actions.getIncorrectList.request());
  try {
    const response = yield call(http.post, `/api/report/list?projectId=${projectId}&pageNumber=${page}`, { ...requestFilters, ...requestSorting });
    yield put(actions.getIncorrectList.success(response.value.data, response.value.page, response.value.rowsPerPage, response.value.totalRows, requestFilters, requestSorting));
  } catch (error) {
    yield put(actions.getIncorrectList.failure(error.message));
  }
};

function* openReportEdition({ projectId, reportId }) {
  yield put(actions.openEdition.request());
  try {
    const humanHealthRisksforProject = yield call(http.get, `/api/report/humanHealthRisksForProject/${projectId}/get`);
    const response = yield call(http.get, `/api/report/${reportId}/get`);
    const filters = {
      area: null,
      sex: null,
      supervisorId: null,
      trainingStatus: null,
      name: null,
      dataCollectorType: response.value.dataCollectorType
    };
    const dataCollectors = yield call(http.post, `/api/dataCollector/listAll?projectId=${projectId}`, filters);
    yield openReportsModule(projectId);
    yield put(actions.openEdition.success(response.value, humanHealthRisksforProject.value.healthRisks, dataCollectors.value));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
};

function* editReport({ projectId, reportId, data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(http.post, `/api/report/${reportId}/edit?projectId=${projectId}`, data);
    yield put(actions.edit.success(response.value));
    yield put(actions.goToList(projectId));
    yield put(appActions.showMessage(stringKeys.reports.list.editedSuccesfully));
  } catch (error) {
    yield put(actions.edit.failure(error.message));
    yield put(appActions.showMessage(error.message));
  }
};

function* getCsvExportData({ projectId, filters, sorting }) {
  yield put(actions.exportToExcel.request());
  try {
    const date = new Date(Date.now());
    yield downloadFile({
      url: `/api/report/exportToCsv?projectId=${projectId}`,
      fileName: `reports_${formatDate(date)}.csv`,
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
    const date = new Date(Date.now());
    yield downloadFile({
      url: `/api/report/exportToExcel?projectId=${projectId}`,
      fileName: `reports_${formatDate(date)}.xlsx`,
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
    yield call(getCorrectReports, { projectId });
  } catch (error) {
    yield put(actions.markAsError.failure());
  }
};

function* openSendReport({ projectId }) {
  yield put(actions.openSendReport.request());
  try {
    const filters = {
      area: null,
      sex: null,
      supervisorId: null,
      trainingStatus: null,
      name: null
    };
    const nationalSocietyId = yield select(state => state.appData.siteMap.parameters.nationalSocietyId);
    const dataCollectors = yield call(http.post, `/api/dataCollector/listAll?projectId=${projectId}`, filters);
    const formData = yield call(http.get, `/api/report/formData?nationalSocietyId=${nationalSocietyId}`);
    yield put(actions.openSendReport.success(dataCollectors.value, formData.value));
  } catch (error) {
    yield put(actions.openSendReport.failure(error.message));
  }
}

function* sendReport({ report }) {
  yield put(actions.sendReport.request());
  try {
    yield call(http.post, `/api/report/sendReport`, report);
    yield put(actions.sendReport.success());
    yield put(appActions.showMessage(stringKeys.reports.sendReport.success));
  } catch (error) {
    yield put(actions.sendReport.failure());
    yield put(appActions.showMessage(error.message));
  }
};

function* acceptReport({ reportId }) {
  yield put(actions.acceptReport.request());
  try {
    const projectId = yield select(state => state.reports.listProjectId);
    yield call(http.post, `/api/report/${reportId}/accept`);
    yield put(actions.acceptReport.success());
    yield put(appActions.showMessage(stringKeys.reports.list.acceptReportSuccess));
    yield getCorrectReports({projectId});
  } catch (error) {
    yield put(actions.acceptReport.failure());
    yield put(appActions.showMessage(error.message));
  }
}

function* dismissReport({ reportId }) {
  yield put(actions.dismissReport.request());
  try {
    const projectId = yield select(state => state.reports.listProjectId);
    yield call(http.post, `/api/report/${reportId}/dismiss`);
    yield put(actions.dismissReport.success());
    yield put(appActions.showMessage(stringKeys.reports.list.dismissReportSuccess));
    yield getCorrectReports({projectId});
  } catch (error) {
    yield put(actions.dismissReport.failure());
    yield put(appActions.showMessage(error.message));
  }
}
