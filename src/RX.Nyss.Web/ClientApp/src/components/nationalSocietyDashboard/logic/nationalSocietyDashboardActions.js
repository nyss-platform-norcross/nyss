import { push } from "connected-react-router";
import {
  OPEN_NATIONAL_SOCIETY_DASHBOARD, GET_NATIONAL_SOCIETY_DASHBOARD_DATA,
  GET_NATIONAL_SOCIETY_DASHBOARD_REPORT_HEALTH_RISKS, GENERATE_PDF
} from "./nationalSocietyDashboardConstants";

export const goToDashboard = (nationalSocietyId) => push(`/nationalsocieties/${nationalSocietyId}/dashboard`);

export const openDashbaord = {
  invoke: (nationalSocietyId, filters) => ({ type: OPEN_NATIONAL_SOCIETY_DASHBOARD.INVOKE, nationalSocietyId, filters }),
  request: () => ({ type: OPEN_NATIONAL_SOCIETY_DASHBOARD.REQUEST }),
  success: (nationalSocietyId, filtersData) => ({ type: OPEN_NATIONAL_SOCIETY_DASHBOARD.SUCCESS, nationalSocietyId, filtersData }),
  failure: (message) => ({ type: OPEN_NATIONAL_SOCIETY_DASHBOARD.FAILURE, message })
};

export const getDashboardData = {
  invoke: (nationalSocietyId, filters) => ({ type: GET_NATIONAL_SOCIETY_DASHBOARD_DATA.INVOKE, nationalSocietyId, filters }),
  request: () => ({ type: GET_NATIONAL_SOCIETY_DASHBOARD_DATA.REQUEST }),
  success: (filters, summary, reportsGroupedByLocation) =>
    ({ type: GET_NATIONAL_SOCIETY_DASHBOARD_DATA.SUCCESS, filters, summary, reportsGroupedByLocation }),
  failure: (message) => ({ type: GET_NATIONAL_SOCIETY_DASHBOARD_DATA.FAILURE, message })
};

export const getReportHealthRisks = {
  invoke: (nationalSocietyId, latitude, longitude) => ({ type: GET_NATIONAL_SOCIETY_DASHBOARD_REPORT_HEALTH_RISKS.INVOKE, nationalSocietyId, latitude, longitude }),
  request: () => ({ type: GET_NATIONAL_SOCIETY_DASHBOARD_REPORT_HEALTH_RISKS.REQUEST }),
  success: (data) => ({ type: GET_NATIONAL_SOCIETY_DASHBOARD_REPORT_HEALTH_RISKS.SUCCESS, data }),
  failure: (message) => ({ type: GET_NATIONAL_SOCIETY_DASHBOARD_REPORT_HEALTH_RISKS.FAILURE, message })
};
