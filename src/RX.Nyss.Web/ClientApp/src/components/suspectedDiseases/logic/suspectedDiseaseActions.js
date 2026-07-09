import { push } from "connected-react-router";
import {
  GET_SUSPECTED_DISEASE,
  CREATE_SUSPECTED_DISEASE,
  REMOVE_SUSPECTED_DISEASE,
} from "./suspectedDiseaseConstants";
import {
  OPEN_EDITION_SUSPECTED_DISEASE,
  EDIT_SUSPECTED_DISEASE,
} from "./suspectedDiseaseConstants";

export const goToCreation = () => push("/suspecteddiseases/add");
export const goToList = () => push("/suspecteddiseases");
export const goToEdition = (id) => push(`/suspecteddiseases/${id}/edit`);

export const getList = {
  invoke: () => ({ type: GET_SUSPECTED_DISEASE.INVOKE }),
  request: () => ({ type: GET_SUSPECTED_DISEASE.REQUEST }),
  success: (list) => ({ type: GET_SUSPECTED_DISEASE.SUCCESS, list }),
  failure: (message) => ({ type: GET_SUSPECTED_DISEASE.FAILURE, message }),
};

export const create = {
  invoke: (data) => ({ type: CREATE_SUSPECTED_DISEASE.INVOKE, data }),
  request: () => ({ type: CREATE_SUSPECTED_DISEASE.REQUEST }),
  success: () => ({ type: CREATE_SUSPECTED_DISEASE.SUCCESS }),
  failure: (error) => ({
    type: CREATE_SUSPECTED_DISEASE.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const edit = {
  invoke: (id, data) => ({ type: EDIT_SUSPECTED_DISEASE.INVOKE, id, data }),
  request: () => ({ type: EDIT_SUSPECTED_DISEASE.REQUEST }),
  success: () => ({ type: EDIT_SUSPECTED_DISEASE.SUCCESS }),
  failure: (error) => ({
    type: EDIT_SUSPECTED_DISEASE.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const openEdition = {
  invoke: ({ path, params }) => ({
    type: OPEN_EDITION_SUSPECTED_DISEASE.INVOKE,
    path,
    params,
  }),
  request: () => ({ type: OPEN_EDITION_SUSPECTED_DISEASE.REQUEST }),
  success: (data) => ({ type: OPEN_EDITION_SUSPECTED_DISEASE.SUCCESS, data }),
  failure: (message) => ({
    type: OPEN_EDITION_SUSPECTED_DISEASE.FAILURE,
    message,
  }),
};

export const remove = {
  invoke: (id) => ({ type: REMOVE_SUSPECTED_DISEASE.INVOKE, id }),
  request: (id) => ({ type: REMOVE_SUSPECTED_DISEASE.REQUEST, id }),
  success: (id) => ({ type: REMOVE_SUSPECTED_DISEASE.SUCCESS, id }),
  failure: (id, message) => ({
    type: REMOVE_SUSPECTED_DISEASE.FAILURE,
    id,
    message,
  }),
};
