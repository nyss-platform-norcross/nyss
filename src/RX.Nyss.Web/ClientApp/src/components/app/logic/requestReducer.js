import { initialState } from "../../../initialState";

const getActionType = (type) => {
  const parts = type.split("_");
  return parts[parts.length - 1];
};

const getActionName = (type) => {
  const parts = type.split("_");
  return parts.slice(0, parts.length - 1).join("_");
};

export function requestReducer(state = initialState.requests, action) {
  const actionType = getActionType(action.type);
  const actionName = getActionName(action.type);

  switch (actionType) {
    case "REQUEST":
      return {
        ...state,
        isFetching: true,
        pending: {
          ...state.pending,
          ...(action.id ? ({ [actionName + "_" + action.id]: true }) : {}),
          [actionName]: true
        }
      };

    case "SUCCESS":
      return {
        ...state,
        isFetching: false,
        pending: {
          ...state.pending,
          ...(action.id ? ({ [actionName + "_" + action.id]: false }) : {}),
          [actionName]: false
        }
      };
    case "FAILURE":
      return {
        ...state,
        isFetching: false,
        pending: {
          ...state.pending,
          ...(action.id ? ({ [actionName + "_" + action.id]: false }) : {}),
          [actionName]: false
        }
      };

    default:
      return state;
  }
};