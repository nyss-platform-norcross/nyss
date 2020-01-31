import * as actions from "./translationsConstants";
import { initialState } from "../../../initialState";
import { LOCATION_CHANGE } from "connected-react-router";

export function translationsReducer(state = initialState.translations, action) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null, formError: null }

    case actions.OPEN_TRANSLATIONS_LIST.INVOKE:
      return { ...state };

    case actions.OPEN_TRANSLATIONS_LIST.SUCCESS:
      return { ...state };

    case actions.GET_TRANSLATIONS.REQUEST:
      return { ...state, listFetching: true, listData: [] };

    case actions.GET_TRANSLATIONS.SUCCESS:
      return { ...state, listFetching: false, listData: action.list };

    case actions.GET_TRANSLATIONS.FAILURE:
      return { ...state, listFetching: false, listData: [] };

    default:
      return state;
  }
};
