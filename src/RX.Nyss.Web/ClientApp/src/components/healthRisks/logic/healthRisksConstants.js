import { action } from "../../../utils/actions";

export const GET_HEALTH_RISKS = action("GET_HEALTH_RISKS");
export const ADD_HEALTH_RISK = action("ADD_HEALTH_RISK");
export const OPEN_CREATION_HEALTH_RISK = action("OPEN_CREATION_HEALTH_RISK");
export const CREATE_HEALTH_RISK = action("CREATE_HEALTH_RISK");
export const EDIT_HEALTH_RISK = action("EDIT_HEALTH_RISK");
export const OPEN_EDITION_HEALTH_RISK = action("OPEN_EDITION_HEALTH_RISK");
export const REMOVE_HEALTH_RISK = action("REMOVE_HEALTH_RISK");

export const healthRiskTypes = [
  "Human",
  "NonHuman",
  "UnusualEvent",
  "Activity",
];
