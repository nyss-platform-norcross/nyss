import { push } from "connected-react-router";
import {
  OPEN_PROJECT_DASHBOARD, GET_PROJECT_DASHBOARD_DATA, UPDATE_PROJECT_DASHBOARD_FILTERS
} from "./projectDashboardConstants";

export const goToDashboard = (nationalSocietyId, projectId) => push(`/nationalsocieties/${nationalSocietyId}/projects/${projectId}/dashboard`);

export const openDashbaord = {
  invoke: (projectId, filters) => ({ type: OPEN_PROJECT_DASHBOARD.INVOKE, projectId, filters }),
  request: () => ({ type: OPEN_PROJECT_DASHBOARD.REQUEST }),
  success: (name, filtersData) => ({ type: OPEN_PROJECT_DASHBOARD.SUCCESS, name, filtersData }),
  failure: (message) => ({ type: OPEN_PROJECT_DASHBOARD.FAILURE, message })
};

export const getDashboardData = {
  invoke: (projectId, filters) => ({ type: GET_PROJECT_DASHBOARD_DATA.INVOKE, projectId, filters }),
  request: () => ({ type: GET_PROJECT_DASHBOARD_DATA.REQUEST }),
  success: (filters, summary, reportsGroupedByDate, reportsGroupedByFeaturesAndDate, reportsGroupedByFeatures, reportsGroupedByLocation) =>
    ({ type: GET_PROJECT_DASHBOARD_DATA.SUCCESS, filters, summary, reportsGroupedByDate, reportsGroupedByFeaturesAndDate, reportsGroupedByFeatures, reportsGroupedByLocation }),
  failure: (message) => ({ type: GET_PROJECT_DASHBOARD_DATA.FAILURE, message })
};
