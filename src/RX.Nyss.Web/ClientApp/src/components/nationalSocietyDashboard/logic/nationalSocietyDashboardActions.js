import { push } from "connected-react-router";
import {
  OPEN_NATIONAL_SOCIETY_DASHBOARD,
  GET_NATIONAL_SOCIETY_DASHBOARD_DATA,
  GET_NATIONAL_SOCIETY_DASHBOARD_REPORT_HEALTH_RISKS,
  GENERATE_NATIONAL_SOCIETY_PDF,
} from "./nationalSocietyDashboardConstants";

export const goToDashboard = (nationalSocietyId) =>
  push(`/nationalsocieties/${nationalSocietyId}/dashboard`);

export const openDashboard = {
  invoke: (nationalSocietyId, filters) => ({
    type: OPEN_NATIONAL_SOCIETY_DASHBOARD.INVOKE,
    nationalSocietyId,
    filters,
  }),
  request: () => ({ type: OPEN_NATIONAL_SOCIETY_DASHBOARD.REQUEST }),
  success: (nationalSocietyId, filtersData) => ({
    type: OPEN_NATIONAL_SOCIETY_DASHBOARD.SUCCESS,
    nationalSocietyId,
    filtersData,
  }),
  failure: (message) => ({
    type: OPEN_NATIONAL_SOCIETY_DASHBOARD.FAILURE,
    message,
  }),
};

export const getDashboardData = {
  invoke: (nationalSocietyId, filters) => ({
    type: GET_NATIONAL_SOCIETY_DASHBOARD_DATA.INVOKE,
    nationalSocietyId,
    filters,
  }),
  request: () => ({ type: GET_NATIONAL_SOCIETY_DASHBOARD_DATA.REQUEST }),
  success: (
    filters,
    summary,
    reportsGroupedByLocation,
    reportsGroupedByVillageAndDate,
    reportsGroupedByHealthRiskAndDate,
    reportsGroupedByFeaturesAndDate,
    reportsGroupedByFeatures,
    keptReportsInEscalatedAlertsHistogramData,
  ) => ({
    type: GET_NATIONAL_SOCIETY_DASHBOARD_DATA.SUCCESS,
    filters,
    summary,
    reportsGroupedByLocation,
    reportsGroupedByVillageAndDate,
    reportsGroupedByHealthRiskAndDate,
    reportsGroupedByFeaturesAndDate,
    reportsGroupedByFeatures,
    keptReportsInEscalatedAlertsHistogramData,
  }),
  failure: (message) => ({
    type: GET_NATIONAL_SOCIETY_DASHBOARD_DATA.FAILURE,
    message,
  }),
};

export const getReportHealthRisks = {
  invoke: (nationalSocietyId, latitude, longitude) => ({
    type: GET_NATIONAL_SOCIETY_DASHBOARD_REPORT_HEALTH_RISKS.INVOKE,
    nationalSocietyId,
    latitude,
    longitude,
  }),
  request: () => ({
    type: GET_NATIONAL_SOCIETY_DASHBOARD_REPORT_HEALTH_RISKS.REQUEST,
  }),
  success: (data) => ({
    type: GET_NATIONAL_SOCIETY_DASHBOARD_REPORT_HEALTH_RISKS.SUCCESS,
    data,
  }),
  failure: (message) => ({
    type: GET_NATIONAL_SOCIETY_DASHBOARD_REPORT_HEALTH_RISKS.FAILURE,
    message,
  }),
};

export const generateNationalSocietyPdf = {
  invoke: (containerElement) => ({
    type: GENERATE_NATIONAL_SOCIETY_PDF.INVOKE,
    containerElement,
  }),
  request: () => ({ type: GENERATE_NATIONAL_SOCIETY_PDF.REQUEST }),
  success: () => ({ type: GENERATE_NATIONAL_SOCIETY_PDF.SUCCESS }),
  failure: (message) => ({
    type: GENERATE_NATIONAL_SOCIETY_PDF.FAILURE,
    message,
  }),
};
