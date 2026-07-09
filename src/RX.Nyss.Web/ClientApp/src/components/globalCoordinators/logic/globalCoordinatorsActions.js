import { push } from "connected-react-router";
import {
  GET_GLOBAL_COORDINATORS,
  CREATE_GLOBAL_COORDINATOR,
  REMOVE_GLOBAL_COORDINATOR,
} from "./globalCoordinatorsConstants";
import {
  OPEN_EDITION_GLOBAL_COORDINATOR,
  EDIT_GLOBAL_COORDINATOR,
} from "./globalCoordinatorsConstants";

export const goToCreation = () => push("/globalcoordinators/add");
export const goToList = () => push("/globalcoordinators");
export const goToEdition = (id) => push(`/globalcoordinators/${id}/edit`);

export const getList = {
  invoke: () => ({ type: GET_GLOBAL_COORDINATORS.INVOKE }),
  request: () => ({ type: GET_GLOBAL_COORDINATORS.REQUEST }),
  success: (list) => ({ type: GET_GLOBAL_COORDINATORS.SUCCESS, list }),
  failure: (message) => ({ type: GET_GLOBAL_COORDINATORS.FAILURE, message }),
};

export const create = {
  invoke: (data) => ({ type: CREATE_GLOBAL_COORDINATOR.INVOKE, data }),
  request: () => ({ type: CREATE_GLOBAL_COORDINATOR.REQUEST }),
  success: () => ({ type: CREATE_GLOBAL_COORDINATOR.SUCCESS }),
  failure: (message) => ({
    type: CREATE_GLOBAL_COORDINATOR.FAILURE,
    message,
    suppressPopup: true,
  }),
};

export const edit = {
  invoke: (data) => ({ type: EDIT_GLOBAL_COORDINATOR.INVOKE, data }),
  request: () => ({ type: EDIT_GLOBAL_COORDINATOR.REQUEST }),
  success: () => ({ type: EDIT_GLOBAL_COORDINATOR.SUCCESS }),
  failure: (message) => ({
    type: EDIT_GLOBAL_COORDINATOR.FAILURE,
    message,
    suppressPopup: true,
  }),
};

export const openEdition = {
  invoke: ({ path, params }) => ({
    type: OPEN_EDITION_GLOBAL_COORDINATOR.INVOKE,
    path,
    params,
  }),
  request: () => ({ type: OPEN_EDITION_GLOBAL_COORDINATOR.REQUEST }),
  success: (data) => ({ type: OPEN_EDITION_GLOBAL_COORDINATOR.SUCCESS, data }),
  failure: (message) => ({
    type: OPEN_EDITION_GLOBAL_COORDINATOR.FAILURE,
    message,
  }),
};

export const remove = {
  invoke: (id) => ({ type: REMOVE_GLOBAL_COORDINATOR.INVOKE, id }),
  request: (id) => ({ type: REMOVE_GLOBAL_COORDINATOR.REQUEST, id }),
  success: (id) => ({ type: REMOVE_GLOBAL_COORDINATOR.SUCCESS, id }),
  failure: (id, message) => ({
    type: REMOVE_GLOBAL_COORDINATOR.FAILURE,
    id,
    message,
  }),
};
