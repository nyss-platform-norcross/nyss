import { action } from "../../../utils/actions";

export const OPEN_DATA_COLLECTORS_LIST = action("OPEN_DATA_COLLECTORS_LIST");
export const GET_DATA_COLLECTORS = action("GET_DATA_COLLECTORS");
export const OPEN_DATA_COLLECTOR_CREATION = action(
  "OPEN_DATA_COLLECTOR_CREATION",
);
export const CREATE_DATA_COLLECTOR = action("CREATE_DATA_COLLECTOR");
export const OPEN_DATA_COLLECTOR_EDITION = action(
  "OPEN_DATA_COLLECTOR_EDITION",
);
export const EDIT_DATA_COLLECTOR = action("EDIT_DATA_COLLECTOR");
export const REMOVE_DATA_COLLECTOR = action("REMOVE_DATA_COLLECTOR");
export const OPEN_DATA_COLLECTORS_MAP_OVERVIEW = action(
  "OPEN_DATA_COLLECTORS_MAP_OVERVIEW",
);
export const GET_DATA_COLLECTORS_MAP_OVERVIEW = action(
  "GET_DATA_COLLECTORS_MAP_OVERVIEW",
);
export const GET_DATA_COLLECTORS_MAP_DETAILS = action(
  "GET_DATA_COLLECTORS_MAP_DETAILS",
);
export const SET_DATA_COLLECTORS_TRAINING_STATE = action(
  "SET_DATA_COLLECTORS_TRAINING_STATE",
);
export const OPEN_DATA_COLLECTORS_PERFORMANCE_LIST = action(
  "OPEN_DATA_COLLECTORS_PERFORMANCE_LIST",
);
export const GET_DATA_COLLECTORS_PERFORMANCE = action(
  "GET_DATA_COLLECTORS_PERFORMANCE",
);
export const EXPORT_DATA_COLLECTORS_TO_EXCEL = action(
  "EXPORT_DATA_COLLECTORS_TO_EXCEL",
);
export const EXPORT_DATA_COLLECTORS_TO_CSV = action(
  "EXPORT_DATA_COLLECTORS_TO_CSV",
);
export const REPLACE_SUPERVISOR = action("REPLACE_SUPERVISOR");
export const SET_DATA_COLLECTORS_DEPLOYED_STATE = action(
  "SET_DATA_COLLECTORS_DEPLOYED_STATE",
);
export const EXPORT_DATA_COLLECTOR_PERFORMANCE = action(
  "EXPORT_DATA_COLLECTOR_PERFORMANCE",
);
export const SELECT_DATA_COLLECTOR = "SELECT_DATA_COLLECTOR";
export const SELECT_ALL_DATA_COLLECTOR = "SELECT_ALL_DATA_COLLECTOR";

export const sexValues = ["Male", "Female", "Other"];

export const dataCollectorType = {
  human: "Human",
  collectionPoint: "CollectionPoint",
};

export const performanceStatus = {
  reportingCorrectly: "ReportingCorrectly",
  notReporting: "NotReporting",
  reportingWithErrors: "ReportingWithErrors",
};

export const trainingStatusAll = "All";
export const trainingStatusInTraining = "InTraining";
export const trainingStatusTrained = "Trained";
export const trainingStatus = [
  trainingStatusAll,
  trainingStatusTrained,
  trainingStatusInTraining,
];

export const deployedModeAll = "All";
export const deployedModeDeployed = "Deployed";
export const deployedModeNotDeployed = "NotDeployed";
export const deployedMode = [
  deployedModeAll,
  deployedModeDeployed,
  deployedModeNotDeployed,
];
