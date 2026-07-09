import { initialState } from "../../../initialState";
import { CLOSE_MESSAGE } from "./appConstans";
import { LOCATION_CHANGE } from "connected-react-router";

const getActionType = (type) => {
  const parts = type.split("_");
  return parts[parts.length - 1];
};

const getActionName = (type) => {
  const parts = type.split("_");
  return parts.slice(0, parts.length - 1).join("_");
};

export function requestReducer(state = initialState.requests, action) {
  if (action.type === CLOSE_MESSAGE.INVOKE) {
    return {
      ...state,
      errorMessage: null,
    };
  }

  if (action.type === LOCATION_CHANGE) {
    return {
      ...state,
      errorMessage: null,
    };
  }

  const actionType = getActionType(action.type);
  const actionName = getActionName(action.type);

  switch (actionType) {
    case "REQUEST":
      return {
        ...state,
        isFetching: true,
        pending: {
          ...state.pending,
          ...(action.id ? { [actionName + "_" + action.id]: true } : {}),
          [actionName]: true,
        },
      };

    case "SUCCESS":
      return {
        ...state,
        isFetching: false,
        pending: {
          ...state.pending,
          ...(action.id ? { [actionName + "_" + action.id]: false } : {}),
          [actionName]: false,
        },
      };
    case "FAILURE":
      return {
        ...state,
        isFetching: false,
        errorMessage: action.suppressPopup
          ? null
          : action.message || (action.error && action.error.message),
        pending: {
          ...state.pending,
          ...(action.id ? { [actionName + "_" + action.id]: false } : {}),
          [actionName]: false,
        },
      };

    default:
      return state;
  }
}
