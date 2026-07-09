import { action } from "../../../utils/actions";
import * as roles from "../../../authentication/roles";

export const OPEN_NATIONAL_SOCIETY_USERS_LIST = action(
  "OPEN_NATIONAL_SOCIETY_USERS_LIST",
);
export const GET_NATIONAL_SOCIETY_USERS = action("GET_NATIONAL_SOCIETY_USERS");
export const OPEN_NATIONAL_SOCIETY_USER_CREATION = action(
  "OPEN_NATIONAL_SOCIETY_USER_CREATION",
);
export const OPEN_NATIONAL_SOCIETY_USER_ADD_EXISTING = action(
  "OPEN_NATIONAL_SOCIETY_USER_ADD_EXISTING",
);
export const CREATE_NATIONAL_SOCIETY_USER = action(
  "CREATE_NATIONAL_SOCIETY_USER",
);
export const ADD_EXISTING_NATIONAL_SOCIETY_USER = action(
  "ADD_EXISTING_NATIONAL_SOCIETY_USER",
);
export const OPEN_NATIONAL_SOCIETY_USER_EDITION = action(
  "OPEN_NATIONAL_SOCIETY_USER_EDITION",
);
export const EDIT_NATIONAL_SOCIETY_USER = action("EDIT_NATIONAL_SOCIETY_USER");
export const REMOVE_NATIONAL_SOCIETY_USER = action(
  "REMOVE_NATIONAL_SOCIETY_USER",
);
export const SET_AS_HEAD_MANAGER = action("SET_AS_HEAD_MANAGER");

export const userRoles = [
  roles.Manager,
  roles.TechnicalAdvisor,
  roles.DataConsumer,
  roles.Supervisor,
  roles.HeadSupervisor,
];
export const globalCoordinatorUserRoles = [roles.Manager, roles.Coordinator];
export const coordinatorUserRoles = [roles.Manager, roles.Coordinator];
export const headManagerRoles = [
  roles.Manager,
  roles.TechnicalAdvisor,
  roles.DataConsumer,
  roles.Supervisor,
  roles.HeadSupervisor,
  roles.Coordinator,
];
export const sexValues = ["Male", "Female", "Other"];
