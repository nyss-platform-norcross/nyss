import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./nationalSocietyReportsConstants";
import * as actions from "./nationalSocietyReportsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { DataCollectorType, ReportErrorFilterType } from '../../common/filters/logic/reportFilterConstsants'
import { DateColumnName } from './nationalSocietyReportsConstants'
import { getUtcOffset } from "../../../utils/date";
import { apiUrl } from "../../../utils/variables";

export const nationalSocietyReportsSagas = () => [
  takeEvery(consts.OPEN_NATIONAL_SOCIETY_CORRECT_REPORTS_LIST.INVOKE, openNationalSocietyCorrectReportsList),
  takeEvery(consts.OPEN_NATIONAL_SOCIETY_INCORRECT_REPORTS_LIST.INVOKE, openNationalSocietyIncorrectReportsList),
  takeEvery(consts.GET_NATIONAL_SOCIETY_CORRECT_REPORTS.INVOKE, getNationalSocietyCorrectReports),
  takeEvery(consts.GET_NATIONAL_SOCIETY_INCORRECT_REPORTS.INVOKE, getNationalSocietyIncorrectReports)
];

function* openNationalSocietyCorrectReportsList({ nationalSocietyId }) {
  const listStale = yield select(state => state.nationalSocietyReports.correctReportsListStale);

  yield put(actions.openCorrectList.request());
  try {
    yield openNationalSocietyReportsModule(nationalSocietyId);

    const filtersData = yield call(http.get, `${apiUrl}/api/nationalSocietyReport/filters?nationalSocietyId=${nationalSocietyId}`);
    const filters = (yield select(state => state.nationalSocietyReports.correctReportsFilters)) ||
    {
      dataCollectorType: DataCollectorType.human,
      healthRisks: [],
      locations: null,
      formatCorrect: true,
      reportStatus: {
        kept: true,
        dismissed: true,
        notCrossChecked: true,
      },
      utcOffset: getUtcOffset()
    };
    const sorting = (yield select(state => state.nationalSocietyReports.correctReportsSorting)) ||
    {
      orderBy: DateColumnName,
      sortAscending: false
    };

    if (listStale) {
      yield call(getNationalSocietyCorrectReports, { nationalSocietyId, filters, sorting });
    }

    yield put(actions.openCorrectList.success(nationalSocietyId, filtersData.value));
  } catch (error) {
    yield put(actions.openCorrectList.failure(error.message));
  }
};

function* openNationalSocietyIncorrectReportsList({ nationalSocietyId }) {
  const listStale = yield select(state => state.nationalSocietyReports.incorrectReportsListStale);

  yield put(actions.openIncorrectList.request());
  try {
    yield openNationalSocietyReportsModule(nationalSocietyId);

    const filters = (yield select(state => state.nationalSocietyReports.incorrectReportsFilters)) ||
    {
      dataCollectorType: DataCollectorType.human,
      errorType: ReportErrorFilterType.all,
      area: null,
      formatCorrect: false,
      utcOffset: getUtcOffset()
    };
    const sorting = (yield select(state => state.nationalSocietyReports.incorrectReportsSorting)) ||
    {
      orderBy: DateColumnName,
      sortAscending: false
    };

    if (listStale) {
      yield call(getNationalSocietyIncorrectReports, { nationalSocietyId, filters, sorting });
    }

    yield put(actions.openIncorrectList.success(nationalSocietyId));
  } catch (error) {
    yield put(actions.openIncorrectList.failure(error.message));
  }
};

function* getNationalSocietyCorrectReports({ nationalSocietyId, pageNumber, filters, sorting }) {
  yield put(actions.getCorrectList.request());
  try {
    const response = yield call(http.post, `${apiUrl}/api/nationalSocietyReport/list?nationalSocietyId=${nationalSocietyId}&pageNumber=${pageNumber || 1}`, { ...filters, ...sorting });
    yield put(actions.getCorrectList.success(response.value.data, response.value.page, response.value.rowsPerPage, response.value.totalRows, filters, sorting));
  } catch (error) {
    yield put(actions.getCorrectList.failure(error.message));
  }
};

function* getNationalSocietyIncorrectReports({ nationalSocietyId, pageNumber, filters, sorting }) {
  yield put(actions.getIncorrectList.request());
  try {
    const response = yield call(http.post, `${apiUrl}/api/nationalSocietyReport/list?nationalSocietyId=${nationalSocietyId}&pageNumber=${pageNumber || 1}`, { ...filters, ...sorting });
    yield put(actions.getIncorrectList.success(response.value.data, response.value.page, response.value.rowsPerPage, response.value.totalRows, filters, sorting));
  } catch (error) {
    yield put(actions.getIncorrectList.failure(error.message));
  }
};

function* openNationalSocietyReportsModule(nationalSocietyId) {
  const nationalSociety = yield call(http.getCached, {
    path: `${apiUrl}/api/nationalSociety/${nationalSocietyId}/get`,
    dependencies: [entityTypes.nationalSociety(nationalSocietyId)]
  });

  yield put(appActions.openModule.invoke(null, {
    nationalSocietyId: nationalSociety.value.id,
    nationalSocietyName: nationalSociety.value.name,
    nationalSocietyCountry: nationalSociety.value.countryName,
    nationalSocietyIsArchived: nationalSociety.value.isArchived
  }));
}
