import * as actions from "./nationalSocietyStructureConstants";
import { initialState } from "../../../initialState";
import { removeFromArray, assignInArray } from "../../../utils/immutable";
import { LOCATION_CHANGE } from "connected-react-router";

export function nationalSocietyStructureReducer(
  state = initialState.nationalSocietyStructure,
  action,
) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, regions: null };

    case actions.OPEN_NATIONAL_SOCIETY_STRUCTURE.REQUEST:
      return { ...state, isFetching: true, regions: null };

    case actions.OPEN_NATIONAL_SOCIETY_STRUCTURE.SUCCESS:
      return {
        ...state,
        isFetching: false,
        regions: action.regions,
        districts: action.districts,
        villages: action.villages,
        zones: action.zones,
      };

    case actions.OPEN_NATIONAL_SOCIETY_STRUCTURE.FAILURE:
      return { ...state, isFetching: false };

    case actions.CREATE_REGION.SUCCESS:
      return { ...state, regions: [...state.regions, action.region] };

    case actions.REMOVE_REGION.SUCCESS:
      return {
        ...state,
        regions: removeFromArray(
          state.regions,
          (item) => item.id === action.id,
        ),
      };

    case actions.EDIT_REGION.SUCCESS:
      return {
        ...state,
        regions: assignInArray(
          state.regions,
          (item) => item.id === action.id,
          (item) => ({ ...item, name: action.name }),
        ),
      };

    case actions.CREATE_DISTRICT.SUCCESS:
      return { ...state, districts: [...state.districts, action.district] };

    case actions.REMOVE_DISTRICT.SUCCESS:
      return {
        ...state,
        districts: removeFromArray(
          state.districts,
          (item) => item.id === action.id,
        ),
      };

    case actions.EDIT_DISTRICT.SUCCESS:
      return {
        ...state,
        districts: assignInArray(
          state.districts,
          (item) => item.id === action.id,
          (item) => ({ ...item, name: action.name }),
        ),
      };

    case actions.CREATE_VILLAGE.SUCCESS:
      return { ...state, villages: [...state.villages, action.village] };

    case actions.REMOVE_VILLAGE.SUCCESS:
      return {
        ...state,
        villages: removeFromArray(
          state.villages,
          (item) => item.id === action.id,
        ),
      };

    case actions.EDIT_VILLAGE.SUCCESS:
      return {
        ...state,
        villages: assignInArray(
          state.villages,
          (item) => item.id === action.id,
          (item) => ({ ...item, name: action.name }),
        ),
      };

    case actions.CREATE_ZONE.SUCCESS:
      return { ...state, zones: [...state.zones, action.zone] };

    case actions.REMOVE_ZONE.SUCCESS:
      return {
        ...state,
        zones: removeFromArray(state.zones, (item) => item.id === action.id),
      };

    case actions.EDIT_ZONE.SUCCESS:
      return {
        ...state,
        zones: assignInArray(
          state.zones,
          (item) => item.id === action.id,
          (item) => ({ ...item, name: action.name }),
        ),
      };

    default:
      return state;
  }
}
