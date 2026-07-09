import * as actions from "./authConstants";
import { initialState } from "../initialState";

export function authReducer(state = initialState.auth, action) {
  switch (action.type) {
    case actions.LOGIN.REQUEST:
      return { ...state, isFetching: true };

    case actions.LOGIN.FAILURE:
      return { ...state, isFetching: false, loginResponse: action.message };

    case actions.VERIFY_EMAIL.FAILURE:
      return { ...state, verifyEmailErrorMessage: action.message };

    case actions.RESET_PASSWORD.REQUEST:
      return { ...state, resetPasswordErrorMessage: null };

    case actions.RESET_PASSWORD.FAILURE:
      return { ...state, resetPasswordErrorMessage: action.message };

    case actions.RESET_PASSWORD_CALLBACK.FAILURE:
      return { ...state, resetPasswordCallbackErrorMessage: action.message };

    default:
      return state;
  }
}
