import * as actions from "./translationsConstants";
import { initialState } from "../../../initialState";
import { LOCATION_CHANGE } from "connected-react-router";
import * as appActions from "../../app/logic/appConstans";
import { assignInArray } from "../../../utils/immutable";

export function translationsReducer(state = initialState.translations, action) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null, formError: null }

    case actions.OPEN_TRANSLATIONS_LIST.INVOKE:
      return { ...state };

    case actions.OPEN_TRANSLATIONS_LIST.SUCCESS:
      return { ...state };

    case actions.GET_TRANSLATIONS.REQUEST:
      return { ...state, listFetching: true, listLanguages: [], listTranslations: [] };

    case actions.GET_TRANSLATIONS.SUCCESS:
      return { ...state, listFetching: false, listLanguages: action.data.languages, listTranslations: action.data.translations };

    case actions.GET_TRANSLATIONS.FAILURE:
      return { ...state, listFetching: false };

      case actions.GET_EMAIL_TRANSLATIONS.REQUEST:
        return { ...state, listFetching: true, emailLanguages: [], emailTranslations: [] };
  
      case actions.GET_EMAIL_TRANSLATIONS.SUCCESS:
        return { ...state, listFetching: false, emailLanguages: action.data.languages, emailTranslations: action.data.translations };
  
      case actions.GET_EMAIL_TRANSLATIONS.FAILURE:
        return { ...state, listFetching: false };

        case actions.GET_SMS_TRANSLATIONS.REQUEST:
          return { ...state, listFetching: true, smsLanguages: [], smsTranslations: [] };
    
        case actions.GET_SMS_TRANSLATIONS.SUCCESS:
          return { ...state, listFetching: false, smsLanguages: action.data.languages, smsTranslations: action.data.translations };
    
        case actions.GET_SMS_TRANSLATIONS.FAILURE:
          return { ...state, listFetching: false };

    case appActions.STRINGS_UPDATED:
      return {
        ...state, listTranslations:
          state.listTranslations.some(t => t.key === action.key)
            ? assignInArray(
              state.listTranslations,
              item => item.key === action.key,
              item => ({
                ...item,
                translations: action.translations
              })
            )
            : [
              ...state.listTranslations,
              {
                key: action.key,
                translations: action.translations
              }
            ]
      };

    default:
      return state;
  }
};
