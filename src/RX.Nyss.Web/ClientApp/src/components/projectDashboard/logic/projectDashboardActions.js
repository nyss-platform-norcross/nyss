import { push } from "connected-react-router";
import {
  OPEN_PROJECT_DASHBOARD,
  GET_PROJECT_DASHBOARD_DATA,
  GET_PROJECT_DASHBOARD_REPORT_HEALTH_RISKS,
  GENERATE_PDF,
} from "./projectDashboardConstants";

export const goToDashboard = (nationalSocietyId, projectId) =>
  push(
    `/nationalsocieties/${nationalSocietyId}/projects/${projectId}/dashboard`,
  );

export const openDashboard = {
  invoke: (projectId, filters) => ({
    type: OPEN_PROJECT_DASHBOARD.INVOKE,
    projectId,
    filters,
  }),
  request: () => ({ type: OPEN_PROJECT_DASHBOARD.REQUEST }),
  success: (projectId, filtersData) => ({
    type: OPEN_PROJECT_DASHBOARD.SUCCESS,
    projectId,
    filtersData,
  }),
  failure: (message) => ({ type: OPEN_PROJECT_DASHBOARD.FAILURE, message }),
};

export const getDashboardData = {
  invoke: (projectId, filters) => ({
    type: GET_PROJECT_DASHBOARD_DATA.INVOKE,
    projectId,
    filters,
  }),
  request: () => ({ type: GET_PROJECT_DASHBOARD_DATA.REQUEST }),
  success: (
    filters,
    summary,
    reportsGroupedByHealthRiskAndDate,
    reportsGroupedByFeaturesAndDate,
    reportsGroupedByVillageAndDate,
    reportsGroupedByFeatures,
    reportsGroupedByLocation,
    dataCollectionPointReports,
    keptReportsInEscalatedAlertsHistogramData,
  ) => ({
    type: GET_PROJECT_DASHBOARD_DATA.SUCCESS,
    filters,
    summary,
    reportsGroupedByHealthRiskAndDate,
    reportsGroupedByFeaturesAndDate,
    reportsGroupedByVillageAndDate,
    reportsGroupedByFeatures,
    reportsGroupedByLocation,
    dataCollectionPointReports,
    keptReportsInEscalatedAlertsHistogramData,
  }),
  failure: (message) => ({ type: GET_PROJECT_DASHBOARD_DATA.FAILURE, message }),
};

export const getReportHealthRisks = {
  invoke: (projectId, latitude, longitude) => ({
    type: GET_PROJECT_DASHBOARD_REPORT_HEALTH_RISKS.INVOKE,
    projectId,
    latitude,
    longitude,
  }),
  request: () => ({ type: GET_PROJECT_DASHBOARD_REPORT_HEALTH_RISKS.REQUEST }),
  success: (data) => ({
    type: GET_PROJECT_DASHBOARD_REPORT_HEALTH_RISKS.SUCCESS,
    data,
  }),
  failure: (message) => ({
    type: GET_PROJECT_DASHBOARD_REPORT_HEALTH_RISKS.FAILURE,
    message,
  }),
};

export const generatePdf = {
  invoke: (containerElement) => ({
    type: GENERATE_PDF.INVOKE,
    containerElement,
  }),
  request: () => ({ type: GENERATE_PDF.REQUEST }),
  success: () => ({ type: GENERATE_PDF.SUCCESS }),
  failure: (message) => ({ type: GENERATE_PDF.FAILURE, message }),
};
