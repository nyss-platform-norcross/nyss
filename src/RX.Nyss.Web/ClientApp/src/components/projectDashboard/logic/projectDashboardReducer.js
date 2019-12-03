import * as actions from "./projectDashboardConstants";
import { initialState } from "../../../initialState";
import { LOCATION_CHANGE } from "connected-react-router";

export function projectDashboardReducer(state = initialState.projectDashboard, action) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null }

    case actions.OPEN_PROJECT_DASHBOARD.REQUEST:
      return { ...state, isFetching: true };

    case actions.OPEN_PROJECT_DASHBOARD.SUCCESS:
      return { ...state, name: action.name, filtersData: action.filtersData, isFetching: false };

    case actions.OPEN_PROJECT_DASHBOARD.FAILURE:
      return { ...state, isFetching: false };

    case actions.GET_PROJECT_DASHBOARD_DATA.REQUEST:
      return { ...state, isFetching: true };

    case actions.GET_PROJECT_DASHBOARD_DATA.SUCCESS:
      return {
        ...state,
        name: action.name,
        filters: action.filters,
        projectSummary: action.summary,
        reportsGroupedByDate: action.reportsGroupedByDate,
        reportsGroupedByFeaturesAndDate: action.reportsGroupedByFeaturesAndDate,
        reportsGroupedByFeatures: action.reportsGroupedByFeatures,
        reportsGroupedByLocation: action.reportsGroupedByLocation,
        isFetching: false
      };

    case actions.GET_PROJECT_DASHBOARD_DATA.FAILURE:
      return { ...state, isFetching: false };

    case actions.GET_PROJECT_DASHBOARD_REPORT_HEALTH_RISKS.REQUEST:
      return { ...state, reportsGroupedByLocationDetails: null, reportsGroupedByLocationDetailsFetching: true };

    case actions.GET_PROJECT_DASHBOARD_REPORT_HEALTH_RISKS.SUCCESS:
      return { ...state, reportsGroupedByLocationDetails: action.data, reportsGroupedByLocationDetailsFetching: false };

    case actions.GET_PROJECT_DASHBOARD_REPORT_HEALTH_RISKS.FAILURE:
      return { ...state, reportsGroupedByLocationDetailsFetching: false };

    default:
      return state;
  }
};
