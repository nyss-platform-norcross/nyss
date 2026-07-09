import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./dataCollectorsConstants";
import * as actions from "./dataCollectorsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { stringKeys } from "../../../strings";
import dayjs from "dayjs";
import { downloadFile } from "../../../utils/downloadFile";
import { convertToUtc, formatDate } from "../../../utils/date";

const defaultLocationFilter = () => ({
  regionIds: [],
  districtIds: [],
  villageIds: [],
  zoneIds: [],
  includeUnknownLocation: true,
});

function normalizeDataCollectorsListFilters(filters) {
  const f = filters || {};
  return {
    ...f,
    locations:
      f.locations && typeof f.locations === "object"
        ? f.locations
        : defaultLocationFilter(),
    name: f.name != null ? f.name : "",
  };
}

function mergePerformanceListFilters(previous, incoming) {
  const prev = previous || {};
  const inc = incoming || {};
  const { type: _filterActionType, ...patch } = inc;
  return { ...prev, ...patch };
}

function normalizePerformanceFilters(filters) {
  const f = filters || {};
  return {
    ...f,
    locations:
      f.locations && typeof f.locations === "object"
        ? f.locations
        : defaultLocationFilter(),
    name: f.name != null ? f.name : "",
    epiWeekFilters: Array.isArray(f.epiWeekFilters) ? f.epiWeekFilters : [],
    healthRiskIds: Array.isArray(f.healthRiskIds) ? f.healthRiskIds : [],
  };
}

export const dataCollectorsSagas = () => [
  takeEvery(consts.OPEN_DATA_COLLECTORS_LIST.INVOKE, openDataCollectorsList),
  takeEvery(
    consts.OPEN_DATA_COLLECTOR_CREATION.INVOKE,
    openDataCollectorCreation,
  ),
  takeEvery(
    consts.OPEN_DATA_COLLECTOR_EDITION.INVOKE,
    openDataCollectorEdition,
  ),
  takeEvery(
    consts.OPEN_DATA_COLLECTORS_MAP_OVERVIEW.INVOKE,
    openDataCollectorMapOverview,
  ),
  takeEvery(
    consts.GET_DATA_COLLECTORS_MAP_OVERVIEW.INVOKE,
    getDataCollectorMapOverview,
  ),
  takeEvery(consts.CREATE_DATA_COLLECTOR.INVOKE, createDataCollector),
  takeEvery(consts.EDIT_DATA_COLLECTOR.INVOKE, editDataCollector),
  takeEvery(consts.REMOVE_DATA_COLLECTOR.INVOKE, removeDataCollector),
  takeEvery(consts.GET_DATA_COLLECTORS_MAP_DETAILS.INVOKE, getMapDetails),
  takeEvery(consts.SET_DATA_COLLECTORS_TRAINING_STATE.INVOKE, setTrainingState),
  takeEvery(
    consts.OPEN_DATA_COLLECTORS_PERFORMANCE_LIST.INVOKE,
    openDataCollectorsPerformanceList,
  ),
  takeEvery(
    consts.GET_DATA_COLLECTORS_PERFORMANCE.INVOKE,
    getDataCollectorsPerformance,
  ),
  takeEvery(consts.EXPORT_DATA_COLLECTORS_TO_EXCEL.INVOKE, getExcelExportData),
  takeEvery(consts.EXPORT_DATA_COLLECTORS_TO_CSV.INVOKE, getCsvExportData),
  takeEvery(consts.GET_DATA_COLLECTORS.INVOKE, getDataCollectors),
  takeEvery(consts.REPLACE_SUPERVISOR.INVOKE, replaceSupervisor),
  takeEvery(consts.SET_DATA_COLLECTORS_DEPLOYED_STATE.INVOKE, setDeployedState),
  takeEvery(
    consts.EXPORT_DATA_COLLECTOR_PERFORMANCE.INVOKE,
    exportDataCollectorPerformance,
  ),
];

function* openDataCollectorsList({ projectId }) {
  yield put(actions.openList.request());
  try {
    yield openDataCollectorsModule(projectId);

    const filtersData = (yield call(
      http.get,
      `/api/dataCollector/filters?projectId=${projectId}`,
    )).value;

    const filters = {
      supervisorId: null,
      locations: {
        regionIds: [],
        districtIds: [],
        villageIds: [],
        zoneIds: [],
        includeUnknownLocation: true
      },
      sex: null,
      dataCollectorType: null,
      trainingStatus: "All",
      deployedMode: "Deployed",
      name: null,
      pageNumber: 1,
    };

    yield call(getDataCollectors, { projectId, filters });

    yield put(actions.openList.success(projectId, filtersData));
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
}

function* openDataCollectorCreation({ projectId }) {
  yield put(actions.openCreation.request());
  try {
    yield openDataCollectorsModule(projectId);

    const response = yield call(
      http.get,
      `/api/dataCollector/formData?projectId=${projectId}`,
    );

    yield put(
      actions.openCreation.success(
        response.value.regions,
        response.value.supervisors,
        response.value.defaultLocation,
        response.value.defaultSupervisorId,
        response.value.countryCode,
      ),
    );
  } catch (error) {
    yield put(actions.openCreation.failure(error.message));
  }
}

function* openDataCollectorMapOverview({ projectId }) {
  yield put(actions.openMapOverview.request());
  try {
    yield openDataCollectorsModule(projectId);

    let endDate = convertToUtc(dayjs(new Date()));
    endDate = endDate.set("hour", 0);
    endDate = endDate.set("minute", 0);
    endDate = endDate.set("second", 0);
    const filters = {
      startDate: endDate.add(-7, "day"),
      endDate: endDate,
      healthRiskIds: [],
    };

    yield call(getDataCollectorMapOverview, { projectId, filters });
    yield put(actions.openMapOverview.success());
  } catch (error) {
    yield put(actions.openMapOverview.failure(error.message));
  }
}

function* getDataCollectorMapOverview({ projectId, filters }) {
  const fromIso = encodeURIComponent(filters.startDate.toISOString());
  const toIso = encodeURIComponent(filters.endDate.toISOString());
  const healthRiskIds = Array.isArray(filters.healthRiskIds)
    ? filters.healthRiskIds
    : [];
  const healthRiskIdsQuery = healthRiskIds
    .map((id) => `healthRiskIds=${encodeURIComponent(id)}`)
    .join("&");
  const healthRiskQuerySuffix = healthRiskIdsQuery
    ? `&${healthRiskIdsQuery}`
    : "";
  
  yield put(actions.getMapOverview.request());
  try {
    const response = yield call(
      http.get,
      `/api/dataCollector/mapOverview?projectId=${projectId}&from=${fromIso}&to=${toIso}${healthRiskQuerySuffix}`,
    );
    const allHealthRiskIds = (response.value.healthRisks || []).map((hr) => hr.id);
    const normalizedHealthRiskIds =
      healthRiskIds.length > 0 ? healthRiskIds : allHealthRiskIds;
    const normalizedFilters = {
      ...filters,
      healthRiskIds: normalizedHealthRiskIds,
    };
    yield put(
      actions.getMapOverview.success(
        normalizedFilters,
        response.value.dataCollectorLocations,
        response.value.centerLocation,
        response.value.healthRisks || [],
      ),
    );
  } catch (error) {
    yield put(actions.getMapOverview.failure(error.message));
  }
}

function* openDataCollectorEdition({ dataCollectorId }) {
  yield put(actions.openEdition.request());
  try {
    const response = yield call(
      http.get,
      `/api/dataCollector/${dataCollectorId}/get`,
    );
    yield openDataCollectorsModule(response.value.projectId);
    yield put(actions.openEdition.success(response.value));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
}

function* createDataCollector({ data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(http.post, `/api/dataCollector/create`, data);
    yield put(actions.create.success(response.value));
    yield put(actions.goToList(data.projectId));
    yield put(appActions.showMessage(response.value));
  } catch (error) {
    yield put(actions.create.failure(error));
  }
}

function* editDataCollector({ data }) {
  yield put(actions.edit.request());
  try {
    // Build payload ensuring displayName is only included for Human data collectors
    // Explicitly exclude it for CollectionPoint to avoid validation errors
    const payload = {
      id: data.id,
      dataCollectorType: data.dataCollectorType,
      name: data.name,
      phoneNumber: data.phoneNumber,
      additionalPhoneNumber: data.additionalPhoneNumber,
      supervisorId: data.supervisorId,
      deployed: data.deployed,
      locations: data.locations,
      linkedToHeadSupervisor: data.linkedToHeadSupervisor,
    };
    
    // Only include Human-specific fields when dataCollectorType is Human
    // For CollectionPoint, these fields must NOT be in the payload
    if (data.dataCollectorType === consts.dataCollectorType.human) {
      payload.displayName = data.displayName;
      payload.sex = data.sex;
      payload.birthGroupDecade = data.birthGroupDecade;
    }
    // Explicitly ensure displayName is not present for CollectionPoint
    else if (data.dataCollectorType === consts.dataCollectorType.collectionPoint) {
      // Make sure displayName is not included
      if (payload.hasOwnProperty('displayName')) {
        delete payload.displayName;
      }
    }
    
    console.log('Edit payload dataCollectorType:', payload.dataCollectorType);
    console.log('Edit payload has displayName:', payload.hasOwnProperty('displayName'));
    console.log('Edit payload:', JSON.stringify(payload, null, 2));
    
    const response = yield call(
      http.post,
      `/api/dataCollector/${data.id}/edit`,
      payload,
    );
    yield put(actions.edit.success(response.value));
    yield put(actions.goToList(data.projectId));
    yield put(appActions.showMessage(response.value));
  } catch (error) {
    yield put(actions.edit.failure(error));
  }
}

function* removeDataCollector({ dataCollectorId }) {
  yield put(actions.remove.request(dataCollectorId));
  try {
    const response = yield call(
      http.post,
      `/api/dataCollector/${dataCollectorId}/delete`,
    );
    yield put(actions.remove.success(dataCollectorId));
    const projectId = yield select(
      (state) => state.appData.route.params.projectId,
    );
    const filters = yield select((state) => state.dataCollectors.filters);
    yield call(getDataCollectors, { projectId, filters });
    yield put(appActions.showMessage(response.value));
  } catch (error) {
    yield put(actions.remove.failure(dataCollectorId, error.message));
  }
}

function* getMapDetails({ projectId, lat, lng }) {
  yield put(actions.getMapDetails.request());
  try {
    const filters = yield select(
      (state) => state.dataCollectors.mapOverviewFilters,
    );
    const fromIso = encodeURIComponent(filters.startDate.toISOString());
    const toIso = encodeURIComponent(filters.endDate.toISOString());
    const healthRiskIds = Array.isArray(filters.healthRiskIds)
      ? filters.healthRiskIds
      : [];
    const healthRiskIdsQuery = healthRiskIds
      .map((id) => `healthRiskIds=${encodeURIComponent(id)}`)
      .join("&");
    const healthRiskQuerySuffix = healthRiskIdsQuery
      ? `&${healthRiskIdsQuery}`
      : "";
    const response = yield call(
      http.get,
      `/api/dataCollector/mapOverviewDetails?projectId=${projectId}&from=${fromIso}&to=${toIso}&lat=${lat}&lng=${lng}${healthRiskQuerySuffix}`,
    );
    yield put(actions.getMapDetails.success(response.value));
  } catch (error) {
    yield put(actions.getMapDetails.failure(error.message));
  }
}

function* getDataCollectors({ projectId, filters }) {
  yield put(actions.getList.request());
  try {
    const normalizedFilters = normalizeDataCollectorsListFilters(filters);
    const response = yield call(
      http.post,
      `/api/dataCollector/list?projectId=${projectId}`,
      normalizedFilters,
    );
    yield put(actions.getList.success(response.value, filters));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
}

function* openDataCollectorsModule(projectId) {
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
      projectIsClosed: project.value.isClosed,
    }),
  );
}

function* setTrainingState({ dataCollectorIds, inTraining }) {
  yield put(actions.setTrainingState.request(dataCollectorIds));
  try {
    const response = yield call(
      http.post,
      "/api/dataCollector/setTrainingState",
      { dataCollectorIds, inTraining },
    );
    yield put(actions.setTrainingState.success(dataCollectorIds, inTraining));
    yield put(appActions.showMessage(response.message.key));
    const projectId = yield select((state) => state.dataCollectors.projectId);
    const filters = yield select((state) => state.dataCollectors.filters);
    yield call(getDataCollectors, { projectId, filters });
  } catch (error) {
    yield put(
      actions.setTrainingState.failure(dataCollectorIds, error.message),
    );
  }
}

function* openDataCollectorsPerformanceList({ projectId }) {
  yield put(actions.openDataCollectorsPerformanceList.request());
  try {
    yield openDataCollectorsModule(projectId);

    const filters = yield select(
      (state) => state.dataCollectors.performanceListFilters,
    );

    let filtersData = yield select((state) => state.dataCollectors.filtersData);

    if (
      filtersData.nationalSocietyId == null ||
      !Array.isArray(filtersData.healthRisks)
    ) {
      const fetchedFiltersData = yield call(
        http.get,
        `/api/dataCollector/filters?projectId=${projectId}`,
      );
      filtersData = fetchedFiltersData.value;
    }

    const allHealthRiskIds = (filtersData.healthRisks || []).map((hr) => hr.id);
    const filtersWithDefaults = {
      ...filters,
      healthRiskIds:
        Array.isArray(filters.healthRiskIds) && filters.healthRiskIds.length > 0
          ? filters.healthRiskIds
          : allHealthRiskIds,
    };

    yield call(getDataCollectorsPerformance, {
      projectId,
      filters: filtersWithDefaults,
    });
    yield put(
      actions.openDataCollectorsPerformanceList.success(
        filtersWithDefaults,
        filtersData,
      ),
    );
  } catch (error) {
    yield put(actions.openDataCollectorsPerformanceList.failure(error.message));
  }
}

function* getDataCollectorsPerformance({ projectId, filters }) {
  yield put(actions.getDataCollectorsPerformanceList.request());
  try {
    const previous = yield select(
      (state) => state.dataCollectors.performanceListFilters,
    );
    const merged = mergePerformanceListFilters(previous, filters);
    const normalizedFilters = normalizePerformanceFilters(merged);
    const response = yield call(
      http.post,
      `/api/dataCollector/performance?projectId=${projectId}`,
      normalizedFilters,
    );
    yield put(
      actions.getDataCollectorsPerformanceList.success(
        response.value.performance,
        response.value.completeness,
        response.value.epiDateRange,
        merged,
      ),
    );
  } catch (error) {
    yield put(actions.getDataCollectorsPerformanceList.failure(error.message));
  }
}

function* getExcelExportData({ projectId, filters }) {
  yield put(actions.exportToExcel.request());
  try {
    const date = new Date(Date.now());
    yield downloadFile({
      url: `/api/dataCollector/exportToExcel?projectId=${projectId}`,
      fileName: `dataCollectors_${formatDate(date)}.xlsx`,
      data: normalizeDataCollectorsListFilters(filters),
    });

    yield put(actions.exportToExcel.success());
  } catch (error) {
    yield put(actions.exportToExcel.failure(error.message));
  }
}

function* getCsvExportData({ projectId, filters }) {
  yield put(actions.exportToCsv.request());
  try {
    const date = new Date(Date.now());
    yield downloadFile({
      url: `/api/dataCollector/exportToCsv?projectId=${projectId}`,
      fileName: `dataCollectors_${formatDate(date)}.csv`,
      data: normalizeDataCollectorsListFilters(filters),
    });

    yield put(actions.exportToCsv.success());
  } catch (error) {
    yield put(actions.exportToCsv.failure(error.message));
  }
}

function* replaceSupervisor({
  dataCollectorIds,
  supervisorId,
  supervisorRole,
}) {
  yield put(actions.replaceSupervisor.request(dataCollectorIds));
  try {
    const projectId = yield select((state) => state.dataCollectors.projectId);
    const filters = yield select((state) => state.dataCollectors.filters);
    yield call(http.post, "/api/dataCollector/replaceSupervisor", {
      dataCollectorIds,
      supervisorId,
      supervisorRole,
    });
    yield call(getDataCollectors, { projectId, filters });
    yield put(actions.replaceSupervisor.success(dataCollectorIds));
    yield put(
      appActions.showMessage(
        stringKeys.dataCollectors.list.supervisorReplacedSuccessfully,
      ),
    );
  } catch (error) {
    yield put(actions.replaceSupervisor.failure(dataCollectorIds, error));
  }
}

function* setDeployedState({ dataCollectorIds, deployed }) {
  yield put(actions.setDeployedState.request(dataCollectorIds));
  try {
    const projectId = yield select((state) => state.dataCollectors.projectId);
    const filters = yield select((state) => state.dataCollectors.filters);
    const response = yield call(
      http.post,
      "/api/dataCollector/setDeployedState",
      { dataCollectorIds, deployed },
    );
    yield call(getDataCollectors, { projectId, filters });
    yield put(actions.setDeployedState.success(dataCollectorIds));
    yield put(appActions.showMessage(response.message.key));
  } catch (error) {
    yield put(actions.setDeployedState.failure(dataCollectorIds, error));
  }
}

function* exportDataCollectorPerformance({ projectId, filters }) {
  yield put(actions.exportDataCollectorPerformance.request());
  try {
    const date = new Date(Date.now());
    const previous = yield select(
      (state) => state.dataCollectors.performanceListFilters,
    );
    const merged = mergePerformanceListFilters(previous, filters);
    yield downloadFile({
      url: `/api/dataCollector/exportPerformance?projectId=${projectId}`,
      fileName: `dataCollectorPerformance_${formatDate(date)}.xlsx`,
      data: normalizePerformanceFilters(merged),
    });

    yield put(actions.exportDataCollectorPerformance.success());
  } catch (error) {
    yield put(actions.exportDataCollectorPerformance.failure(error.message));
  }
}