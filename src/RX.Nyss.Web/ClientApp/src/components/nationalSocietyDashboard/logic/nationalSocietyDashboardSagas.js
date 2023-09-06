import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./nationalSocietyDashboardConstants";
import * as actions from "./nationalSocietyDashboardActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { stringsFormat, stringKeys } from "../../../strings";
import { generatePdfDocument } from "../../../utils/pdf";
import { trackTrace } from "../../../utils/tracking";
import dayjs from "dayjs";
import utc from 'dayjs/plugin/utc';
dayjs.extend(utc);

export const nationalSocietyDashboardSagas = () => [
  takeEvery(consts.OPEN_NATIONAL_SOCIETY_DASHBOARD.INVOKE, openNationalSocietyDashboard),
  takeEvery(consts.GET_NATIONAL_SOCIETY_DASHBOARD_DATA.INVOKE, getNationalSocietyDashboardData),
  takeEvery(consts.GET_NATIONAL_SOCIETY_DASHBOARD_REPORT_HEALTH_RISKS.INVOKE, getNationalSocietyDashboardReportHealthRisks),
  takeEvery(consts.GENERATE_NATIONAL_SOCIETY_PDF.INVOKE, generateNationalSocietyPdf),
];

function* openNationalSocietyDashboard({ nationalSocietyId }) {
  yield put(actions.openDashboard.request());
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
      healthRisks: [],
      organizationId: null,
      locations: null,
      startDate: endDate.add(-7, "day"),
      endDate: endDate,
      groupingType: "Day",
      reportStatus: {
        kept: true,
        dismissed: false,
        notCrossChecked: true,
      },
      dataCollectorType: "all",
      utcOffset: utcOffset
    };

    yield call(getNationalSocietyDashboardData, { nationalSocietyId, filters })

    yield put(actions.openDashboard.success(nationalSocietyId, filtersData.value));
  } catch (error) {
    yield put(actions.openDashboard.failure(error.message));
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

function* generateNationalSocietyPdf({ containerElement }) {
  const reportFileName = "Report";

  const nationalSocietyId = yield select(state => state.appData.route.params.nationalSocietyId);
  const message = "Generate National Society dashboard pdf";
  const properties = {
    nationalSocietyId,
  };

  yield call(trackTrace, { message, properties });
  yield put(actions.generateNationalSocietyPdf.request());
  try {
    const siteMapParams = yield select(state => state.appData.siteMap.parameters);

    const printTitleParams = {
      nationalSocietyName: siteMapParams.nationalSocietyName
    }

    const title = stringsFormat(stringKeys.dashboard.printTitle, printTitleParams, true);

    yield call(generatePdfDocument, title, containerElement, reportFileName);
    yield put(actions.generateNationalSocietyPdf.success());
  } catch (error) {
    yield put(actions.generateNationalSocietyPdf.failure(error.message));
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
