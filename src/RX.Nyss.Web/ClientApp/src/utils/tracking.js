import { put, takeEvery, select } from "redux-saga/effects";
import { ApplicationInsights } from "@microsoft/applicationinsights-web";
import { action } from "./actions";
import { initialState } from "../initialState";

const INIT_TRACKING = action("INIT_TRACKING");
const TRACK_PAGEVIEW = action("TRACK_PAGEVIEW");
const TRACK_TRACE = action("TRACK_TRACE");

export const actions = {
  pageView: (alias) => ({ type: TRACK_PAGEVIEW.INVOKE, alias }),
  trackTrace: (message, properties) => ({
    type: TRACK_TRACE.INVOKE,
    message,
    properties,
  }),
};

export const trackingSagas = () => [
  takeEvery(INIT_TRACKING.INVOKE, initTracking),
  takeEvery(TRACK_PAGEVIEW.INVOKE, trackPage),
  takeEvery(TRACK_TRACE.INVOKE, trackTrace),
];

export function trackingReducer(state = initialState.tracking, action) {
  switch (action.type) {
    case INIT_TRACKING.SUCCESS:
      return {
        ...state,
        appInsights: action.appInsights,
      };
    default:
      return state;
  }
}

export function* initTracking() {
  const appData = yield select((state) => state.appData);

  const appInsights = new ApplicationInsights({
    config: {
      connectionString: appData.applicationInsightsConnectionString,
    },
  });
  appInsights.loadAppInsights();

  yield put({ type: INIT_TRACKING.SUCCESS, appInsights });
}

function* trackPage({ alias }) {
  const user = yield select((state) => state.appData.user);
  const appInsights = yield select((state) => state.tracking.appInsights);

  appInsights.trackPageView({
    properties: {
      alias,
      userRole: user.roles[0],
    },
  });
}

export function* trackTrace({ message, properties }) {
  const user = yield select((state) => state.appData.user);
  const appInsights = yield select((state) => state.tracking.appInsights);

  appInsights.trackTrace({
    message,
    properties: {
      ...properties,
      userRole: user.roles[0],
    },
  });
}
