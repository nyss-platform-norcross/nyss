import { action } from "../../../utils/actions";

export const GET_NATIONAL_SOCIETIES = action("GET_NATIONAL_SOCIETIES");
export const ADD_NATIONAL_SOCIETY = action("ADD_NATIONAL_SOCIETY");
export const CREATE_NATIONAL_SOCIETY = action("CREATE_NATIONAL_SOCIETY");
export const EDIT_NATIONAL_SOCIETY = action("EDIT_NATIONAL_SOCIETY");
export const OPEN_EDITION_NATIONAL_SOCIETY = action("OPEN_EDITION_NATIONAL_SOCIETY");
export const OPEN_NATIONAL_SOCIETY_OVERVIEW = action("OPEN_NATIONAL_SOCIETY_OVERVIEW");
export const OPEN_NATIONAL_SOCIETY_DASHBOARD = action("OPEN_NATIONAL_SOCIETY_DASHBOARD");
export const REMOVE_NATIONAL_SOCIETY = action("REMOVE_NATIONAL_SOCIETY");

export const entityTypes = {
  nationalSociety: id => `nationalSociety:${id}`
}