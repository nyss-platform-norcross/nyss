import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./nationalSocietyDashboardConstants";
import * as actions from "./nationalSocietyDashboardActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import dayjs from "dayjs";
import utc from 'dayjs/plugin/utc';
dayjs.extend(utc);

export const nationalSocietyDashboardSagas = () => [
  takeEvery(consts.OPEN_NATIONAL_SOCIETY_DASHBOARD.INVOKE, openNationalSocietyDashboard),
  takeEvery(consts.GET_NATIONAL_SOCIETY_DASHBOARD_DATA.INVOKE, getNationalSocietyDashboardData),
  takeEvery(consts.GET_NATIONAL_SOCIETY_DASHBOARD_REPORT_HEALTH_RISKS.INVOKE, getNationalSocietyDashboardReportHealthRisks),
];

function* openNationalSocietyDashboard({ nationalSocietyId }) {
  yield put(actions.openDashbaord.request());
  try {
    yield call(openNationalSocietyDashboardModule, nationalSocietyId);
    const filtersData = yield call(http.get, `/api/nationalSocietyDashboard/filters?nationalSocietyId=${nationalSocietyId}`);
    const localDate = dayjs();
    const utcOffset = Math.floor(localDate.utcOffset() / 60);
    let endDate = localDate.add(-utcOffset, 'hour');
    endDate = endDate.set('hour', 0);
    endDate = endDate.set('minute', 0);
    endDate = endDate.set('second', 0);
    const filters = (yield select(state => state.nationalSocietyDashboard.filters)) ||
    {
      healthRiskId: null,
      organizationId: null,
      area: null,
      startDate: endDate.add(-7, "day"),
      endDate: endDate,
      groupingType: "Day",
      reportStatus: {
        kept: true,
        dismissed: false,
        notCrossChecked: true,
        training: false
      },
      dataCollectorType: "all",
      utcOffset: utcOffset
    };

    yield call(getNationalSocietyDashboardData, { nationalSocietyId, filters })

    yield put(actions.openDashbaord.success(nationalSocietyId, filtersData.value));
  } catch (error) {
    yield put(actions.openDashbaord.failure(error.message));
  }
};

function* getNationalSocietyDashboardData({ nationalSocietyId, filters }) {
  yield put(actions.getDashboardData.request());
  try {
    const response = yield call(http.post, `/api/nationalSocietyDashboard/data?nationalSocietyId=${nationalSocietyId}`, filters);
    yield put(actions.getDashboardData.success(
      filters,
      response.value.summary,
      response.value.reportsGroupedByLocation,
      response.value.reportsGroupedByVillageAndDate
    ));
  } catch (error) {
    yield put(actions.getDashboardData.failure(error.message));
  }
};

function* getNationalSocietyDashboardReportHealthRisks({ nationalSocietyId, latitude, longitude }) {
  yield put(actions.getReportHealthRisks.request());
  try {
    const filters = yield select(state => state.nationalSocietyDashboard.filters);
    const response = yield call(http.post, `/api/nationalSocietyDashboard/reportHealthRisks?nationalSocietyId=${nationalSocietyId}&latitude=${latitude}&longitude=${longitude}`, filters);
    yield put(actions.getReportHealthRisks.success(response.value));
  } catch (error) {
    yield put(actions.getReportHealthRisks.failure(error.message));
  }
};

function* openNationalSocietyDashboardModule(nationalSocietyId) {
  const nationalSociety = yield call(http.getCached, {
    path: `/api/nationalSociety/${nationalSocietyId}/get`,
    dependencies: [entityTypes.nationalSociety(nationalSocietyId)]
  });

  yield put(appActions.openModule.invoke(null, {
    nationalSocietyId: nationalSociety.value.id,
    nationalSocietyName: nationalSociety.value.name,
    nationalSocietyCountry: nationalSociety.value.countryName,
    nationalSocietyIsArchived: nationalSociety.value.isArchived
  }));

  return nationalSociety.value;
}
