import { push } from "connected-react-router";
import {
  OPEN_PROJECT_DASHBOARD
} from "./projectDashboardConstants";

export const goToDashboard = (nationalSocietyId, projectId) => push(`/nationalsocieties/${nationalSocietyId}/projects/${projectId}/dashboard`);

export const openDashbaord = {
  invoke: (projectId) => ({ type: OPEN_PROJECT_DASHBOARD.INVOKE, projectId }),
  request: () => ({ type: OPEN_PROJECT_DASHBOARD.REQUEST }),
  success: (name, projectSummary) => ({ type: OPEN_PROJECT_DASHBOARD.SUCCESS, name, projectSummary }),
  failure: (message) => ({ type: OPEN_PROJECT_DASHBOARD.FAILURE, message })
};