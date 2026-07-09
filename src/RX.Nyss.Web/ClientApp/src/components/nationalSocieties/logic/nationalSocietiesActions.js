import { push } from "connected-react-router";
import {
  GET_NATIONAL_SOCIETIES,
  CREATE_NATIONAL_SOCIETY,
  OPEN_NATIONAL_SOCIETY_OVERVIEW,
} from "./nationalSocietiesConstants";
import {
  OPEN_EDITION_NATIONAL_SOCIETY,
  EDIT_NATIONAL_SOCIETY,
  ARCHIVE_NATIONAL_SOCIETY,
  REOPEN_NATIONAL_SOCIETY,
} from "./nationalSocietiesConstants";

export const goToCreation = () => push("/nationalsocieties/add");
export const goToList = () => push("/nationalsocieties");
export const goToEdition = (id) => push(`/nationalsocieties/${id}/edit`);
export const goToOverview = (id) => push(`/nationalsocieties/${id}/overview`);

export const getList = {
  invoke: (userName, password, redirectUrl) => ({
    type: GET_NATIONAL_SOCIETIES.INVOKE,
    userName,
    password,
    redirectUrl,
  }),
  request: () => ({ type: GET_NATIONAL_SOCIETIES.REQUEST }),
  success: (list) => ({ type: GET_NATIONAL_SOCIETIES.SUCCESS, list }),
  failure: (message) => ({ type: GET_NATIONAL_SOCIETIES.FAILURE, message }),
};

export const create = {
  invoke: (data) => ({ type: CREATE_NATIONAL_SOCIETY.INVOKE, data }),
  request: () => ({ type: CREATE_NATIONAL_SOCIETY.REQUEST }),
  success: () => ({ type: CREATE_NATIONAL_SOCIETY.SUCCESS }),
  failure: (error) => ({
    type: CREATE_NATIONAL_SOCIETY.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const edit = {
  invoke: (data) => ({ type: EDIT_NATIONAL_SOCIETY.INVOKE, data }),
  request: () => ({ type: EDIT_NATIONAL_SOCIETY.REQUEST }),
  success: () => ({ type: EDIT_NATIONAL_SOCIETY.SUCCESS }),
  failure: (error) => ({
    type: EDIT_NATIONAL_SOCIETY.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const openEdition = {
  invoke: ({ path, params }) => ({
    type: OPEN_EDITION_NATIONAL_SOCIETY.INVOKE,
    path,
    params,
  }),
  request: () => ({ type: OPEN_EDITION_NATIONAL_SOCIETY.REQUEST }),
  success: (data) => ({ type: OPEN_EDITION_NATIONAL_SOCIETY.SUCCESS, data }),
  failure: (message) => ({
    type: OPEN_EDITION_NATIONAL_SOCIETY.FAILURE,
    message,
  }),
};

export const openOverview = {
  invoke: ({ path, params }) => ({
    type: OPEN_NATIONAL_SOCIETY_OVERVIEW.INVOKE,
    path,
    params,
  }),
  request: () => ({ type: OPEN_NATIONAL_SOCIETY_OVERVIEW.REQUEST }),
  success: (data) => ({ type: OPEN_NATIONAL_SOCIETY_OVERVIEW.SUCCESS, data }),
  failure: (message) => ({
    type: OPEN_NATIONAL_SOCIETY_OVERVIEW.FAILURE,
    message,
  }),
};

export const archive = {
  invoke: (id) => ({ type: ARCHIVE_NATIONAL_SOCIETY.INVOKE, id }),
  request: (id) => ({ type: ARCHIVE_NATIONAL_SOCIETY.REQUEST, id }),
  success: (id) => ({ type: ARCHIVE_NATIONAL_SOCIETY.SUCCESS, id }),
  failure: (id, message) => ({
    type: ARCHIVE_NATIONAL_SOCIETY.FAILURE,
    id,
    message,
  }),
};

export const reopen = {
  invoke: (id) => ({ type: REOPEN_NATIONAL_SOCIETY.INVOKE, id }),
  request: (id) => ({ type: REOPEN_NATIONAL_SOCIETY.REQUEST, id }),
  success: (id) => ({ type: REOPEN_NATIONAL_SOCIETY.SUCCESS, id }),
  failure: (id, message) => ({
    type: REOPEN_NATIONAL_SOCIETY.FAILURE,
    id,
    message,
  }),
};
