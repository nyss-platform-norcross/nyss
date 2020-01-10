import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./nationalSocietyReportsConstants";
import * as actions from "./nationalSocietyReportsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { ReportListType } from '../../common/filters/logic/reportFilterConstsants'
import { DateColumnName } from './nationalSocietyReportsConstants'

export const nationalSocietyReportsSagas = () => [
  takeEvery(consts.OPEN_NATIONAL_SOCIETY_REPORTS_LIST.INVOKE, openNationalSocietyReportsList),
  takeEvery(consts.GET_NATIONAL_SOCIETY_REPORTS.INVOKE, getNationalSocietyReports)
];

function* openNationalSocietyReportsList({ nationalSocietyId }) {
  const listStale = yield select(state => state.nationalSocietyReports.listStale);

  yield put(actions.openList.request());
  try {
    yield openNationalSocietyReportsModule(nationalSocietyId);

    const filtersData = yield call(http.get, `/api/nationalSocietyReport/filters?nationalSocietyId=${nationalSocietyId}`);
    const filters = (yield select(state => state.nationalSocietyReports.filters)) ||
    {
      reportsType: ReportListType.main,
      area: null,
      healthRiskId: null,
      status: true
    };
    const sorting = (yield select(state => state.nationalSocietyReports.sorting)) ||
    {
      orderBy: DateColumnName,
      sortAscending: false
    };

    if (listStale) {
      yield call(getNationalSocietyReports, { nationalSocietyId, filters, sorting });
    }

    yield put(actions.openList.success(nationalSocietyId, filtersData.value));
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
};

function* getNationalSocietyReports({ nationalSocietyId, pageNumber, filters, sorting }) {
  yield put(actions.getList.request());
  try {
    const response = yield call(http.post, `/api/nationalSocietyReport/list?nationalSocietyId=${nationalSocietyId}&pageNumber=${pageNumber || 1}`, { ...filters, ...sorting });
    http.ensureResponseIsSuccess(response);
    yield put(actions.getList.success(response.value.data, response.value.page, response.value.rowsPerPage, response.value.totalRows, filters, sorting));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
};

function* openNationalSocietyReportsModule(nationalSocietyId) {
  const nationalSociety = yield call(http.getCached, {
    path: `/api/nationalSociety/${nationalSocietyId}/get`,
    dependencies: [entityTypes.nationalSociety(nationalSocietyId)]
  });

  yield put(appActions.openModule.invoke(null, {
    nationalSocietyId: nationalSociety.value.id,
    nationalSocietyName: nationalSociety.value.name,
    nationalSocietyCountry: nationalSociety.value.countryName
  }));
}
