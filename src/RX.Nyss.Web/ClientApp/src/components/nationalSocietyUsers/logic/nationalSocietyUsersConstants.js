import { action } from "../../../utils/actions";
import * as roles from "../../../authentication/roles";

export const OPEN_NATIONAL_SOCIETY_USERS_LIST = action("OPEN_NATIONAL_SOCIETY_USERS_LIST");
export const GET_NATIONAL_SOCIETY_USERS = action("GET_NATIONAL_SOCIETY_USERS");
export const OPEN_NATIONAL_SOCIETY_USER_CREATION = action("OPEN_NATIONAL_SOCIETY_USER_CREATION");
export const CREATE_NATIONAL_SOCIETY_USER = action("CREATE_NATIONAL_SOCIETY_USER");
export const OPEN_NATIONAL_SOCIETY_USER_EDITION = action("OPEN_NATIONAL_SOCIETY_USER_EDITION");
export const EDIT_NATIONAL_SOCIETY_USER = action("EDIT_NATIONAL_SOCIETY_USER");
export const REMOVE_NATIONAL_SOCIETY_USER = action("REMOVE_NATIONAL_SOCIETY_USER");

export const userRoles = [ roles.DataManager, roles.TechnicalAdvisor, roles.DataConsumer ];