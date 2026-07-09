import * as actions from "./nationalSocietyDashboardConstants";
import { initialState } from "../../../initialState";

export function nationalSocietyDashboardReducer(
  state = initialState.nationalSocietyDashboard,
  action,
) {
  switch (action.type) {
    case actions.OPEN_NATIONAL_SOCIETY_DASHBOARD.INVOKE:
      return {
        ...state,
        filters:
          action.nationalSocietyId === state.nationalSocietyId
            ? state.filters
            : null,
      };

    case actions.OPEN_NATIONAL_SOCIETY_DASHBOARD.REQUEST:
      return { ...state, isFetching: true };

    case actions.OPEN_NATIONAL_SOCIETY_DASHBOARD.SUCCESS:
      return {
        ...state,
        nationalSocietyId: action.nationalSocietyId,
        filtersData: action.filtersData,
        isFetching: false,
      };

    case actions.OPEN_NATIONAL_SOCIETY_DASHBOARD.FAILURE:
      return { ...state, isFetching: false };

    case actions.GENERATE_NATIONAL_SOCIETY_PDF.REQUEST:
      return { ...state, isGeneratingPdf: true };

    case actions.GENERATE_NATIONAL_SOCIETY_PDF.SUCCESS:
    case actions.GENERATE_NATIONAL_SOCIETY_PDF.FAILURE:
      return { ...state, isGeneratingPdf: false };

    case actions.GET_NATIONAL_SOCIETY_DASHBOARD_DATA.REQUEST:
      return { ...state, isFetching: true };

    // Updates redux state with dashboard data recieved from action
    case actions.GET_NATIONAL_SOCIETY_DASHBOARD_DATA.SUCCESS:
      return {
        ...state,
        name: action.name,
        filters: action.filters,
        summary: action.summary,
        reportsGroupedByLocation: action.reportsGroupedByLocation,
        reportsGroupedByVillageAndDate: action.reportsGroupedByVillageAndDate,
        reportsGroupedByHealthRiskAndDate:
          action.reportsGroupedByHealthRiskAndDate,
        reportsGroupedByFeaturesAndDate: action.reportsGroupedByFeaturesAndDate,
        reportsGroupedByFeatures: action.reportsGroupedByFeatures,
        keptReportsInEscalatedAlertsHistogramData:
          action.keptReportsInEscalatedAlertsHistogramData,
        isFetching: false,
      };

    case actions.GET_NATIONAL_SOCIETY_DASHBOARD_DATA.FAILURE:
      return { ...state, isFetching: false };

    case actions.GET_NATIONAL_SOCIETY_DASHBOARD_REPORT_HEALTH_RISKS.REQUEST:
      return {
        ...state,
        reportsGroupedByLocationDetails: null,
        reportsGroupedByLocationDetailsFetching: true,
      };

    case actions.GET_NATIONAL_SOCIETY_DASHBOARD_REPORT_HEALTH_RISKS.SUCCESS:
      return {
        ...state,
        reportsGroupedByLocationDetails: action.data,
        reportsGroupedByLocationDetailsFetching: false,
      };

    case actions.GET_NATIONAL_SOCIETY_DASHBOARD_REPORT_HEALTH_RISKS.FAILURE:
      return { ...state, reportsGroupedByLocationDetailsFetching: false };

    default:
      return state;
  }
}
