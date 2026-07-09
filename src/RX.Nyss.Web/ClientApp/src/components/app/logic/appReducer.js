import { initialState } from "../../../initialState";
import * as actions from "./appConstans";
import { LOCATION_CHANGE } from "connected-react-router";

export function appReducer(state = initialState.appData, action) {
  switch (action.type) {
    case LOCATION_CHANGE:
      return {
        ...state,
        moduleError: null,
        messageKey: null,
        messageTime: null,
        mobile: {
          ...state.mobile,
          sideMenuOpen: false,
        },
      };

    case actions.SWITCH_STRINGS:
      return {
        ...state,
        showStringsKeys: action.status,
      };

    case actions.SET_APP_READY:
      return {
        ...state,
        appReady: action.status,
      };

    case actions.ROUTE_CHANGED:
      return {
        ...state,
        route: {
          url: action.url,
          path: action.path,
          params: action.params,
        },
      };

    case actions.INIT_APPLICATION.SUCCESS:
      return {
        ...state,
        appReady: true,
      };

    case actions.GET_USER.SUCCESS:
      return {
        ...state,
        user: action.user
          ? {
              id: action.user.id,
              name: action.user.name,
              email: action.user.email,
              roles: action.user.roles,
              languageCode: action.user.languageCode,
              homePage: action.user.homePage,
            }
          : null,
        direction:
          action.user && action.user.languageCode === "ar"
            ? "rtl"
            : state.direction,
      };

    case actions.GET_APP_DATA.SUCCESS:
      return {
        ...state,
        contentLanguages: action.contentLanguages,
        authCookieExpiration: action.authCookieExpiration,
        countries: action.countries,
        isDevelopment: action.isDevelopment,
        isDemo: action.isDemo,
        applicationInsightsConnectionString:
          action.applicationInsightsConnectionString,
      };

    case actions.OPEN_MODULE.INVOKE:
      return {
        ...state,
        siteMap: {
          path: action.path,
          parameters: {},
          generalMenu: [],
          sideMenu: [],
          tabMenu: [],
          projectTabMenu: [],
          title: null,
          subTitle: null,
        },
      };

    case actions.OPEN_MODULE.SUCCESS:
      return {
        ...state,
        siteMap: {
          path: action.path,
          parameters: action.parameters,
          generalMenu: action.generalMenu,
          sideMenu: action.sideMenu,
          tabMenu: action.tabMenu,
          projectTabMenu: action.projectTabMenu,
          title: action.title,
        },
      };

    case actions.TOGGLE_SIDE_MENU:
      return {
        ...state,
        mobile: {
          ...state.mobile,
          sideMenuOpen: action.value,
        },
      };

    case actions.EXPAND_SIDE_MENU:
      return {
        ...state,
        isSideMenuExpanded: action.value,
      };

    case actions.OPEN_MODULE.FAILURE:
      return {
        ...state,
        moduleError: action.message,
      };

    case actions.SHOW_MESSAGE.INVOKE:
      return {
        ...state,
        messageKey: action.messageKey,
        messageTime: action.time,
      };

    case actions.CLOSE_MESSAGE.INVOKE:
      return {
        ...state,
        messageKey: null,
        messageTime: null,
      };

    case actions.SEND_FEEDBACK.REQUEST:
      return {
        ...state,
        feedback: {
          isSending: true,
          result: "",
        },
      };

    case actions.SEND_FEEDBACK.SUCCESS:
      return {
        ...state,
        feedback: {
          isSending: false,
          result: "ok",
        },
      };

    case actions.SEND_FEEDBACK.FAILURE:
      return {
        ...state,
        feedback: {
          isSending: false,
          result: "error",
        },
      };

    default:
      return state;
  }
}
