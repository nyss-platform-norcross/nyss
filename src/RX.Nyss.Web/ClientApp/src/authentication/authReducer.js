import * as actions from "./authConstants";
import { initialState } from "../initialState";

export function authReducer(state = initialState.auth, action) {
  switch (action.type) {
    case actions.LOGIN.FAILURE:
      return {
        ...state,
        loginResponse: action.message
      };

    default:
      return state;
  }
};
