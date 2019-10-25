import * as actions from "./nationalSocietiesConstants";
import { initialState } from "../../../initialState";

export function nationalSocietiesReducer(state = initialState.nationalSocieties, action) {
  switch (action.type) {
    case actions.GET_NATIONAL_SOCIETIES.REQUEST:
      return {
        ...state,
        list: {
          isFetching: true,
          data: []
        }
      };

    case actions.GET_NATIONAL_SOCIETIES.SUCCESS:
      return {
        ...state,
        list: {
          isFetching: false,
          data: action.list
        }
      };

    case actions.GET_NATIONAL_SOCIETIES.FAILURE:
      return {
        ...state,
        list: {
          isFetching: false,
          data: []
        }
      };

    case actions.CREATE_NATIONAL_SOCIETY.SUCCESS:
      return {
        ...state,
        list: {
          isFetching: false,
          data: []
        }
      };

    default:
      return state;
  }
};
