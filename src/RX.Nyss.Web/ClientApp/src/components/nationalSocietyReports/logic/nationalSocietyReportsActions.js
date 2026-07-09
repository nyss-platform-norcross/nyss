import { push } from "connected-react-router";
import {
  OPEN_NATIONAL_SOCIETY_INCORRECT_REPORTS_LIST,
  OPEN_NATIONAL_SOCIETY_CORRECT_REPORTS_LIST,
  GET_NATIONAL_SOCIETY_CORRECT_REPORTS,
  GET_NATIONAL_SOCIETY_INCORRECT_REPORTS,
} from "./nationalSocietyReportsConstants";

export const goToList = (nationalSocietyId) =>
  push(`/nationalsociety/${nationalSocietyId}/reports`);

export const openCorrectList = {
  invoke: (nationalSocietyId) => ({
    type: OPEN_NATIONAL_SOCIETY_CORRECT_REPORTS_LIST.INVOKE,
    nationalSocietyId,
  }),
  request: () => ({ type: OPEN_NATIONAL_SOCIETY_CORRECT_REPORTS_LIST.REQUEST }),
  success: (nationalSocietyId, filtersData) => ({
    type: OPEN_NATIONAL_SOCIETY_CORRECT_REPORTS_LIST.SUCCESS,
    nationalSocietyId,
    filtersData,
  }),
  failure: (message) => ({
    type: OPEN_NATIONAL_SOCIETY_CORRECT_REPORTS_LIST.FAILURE,
    message,
  }),
};

export const openIncorrectList = {
  invoke: (nationalSocietyId) => ({
    type: OPEN_NATIONAL_SOCIETY_INCORRECT_REPORTS_LIST.INVOKE,
    nationalSocietyId,
  }),
  request: () => ({
    type: OPEN_NATIONAL_SOCIETY_INCORRECT_REPORTS_LIST.REQUEST,
  }),
  success: (nationalSocietyId) => ({
    type: OPEN_NATIONAL_SOCIETY_INCORRECT_REPORTS_LIST.SUCCESS,
    nationalSocietyId,
  }),
  failure: (message) => ({
    type: OPEN_NATIONAL_SOCIETY_INCORRECT_REPORTS_LIST.FAILURE,
    message,
  }),
};

export const getCorrectList = {
  invoke: (nationalSocietyId, pageNumber, filters, sorting) => ({
    type: GET_NATIONAL_SOCIETY_CORRECT_REPORTS.INVOKE,
    nationalSocietyId,
    pageNumber,
    filters,
    sorting,
  }),
  request: () => ({ type: GET_NATIONAL_SOCIETY_CORRECT_REPORTS.REQUEST }),
  success: (data, page, rowsPerPage, totalRows, filters, sorting) => ({
    type: GET_NATIONAL_SOCIETY_CORRECT_REPORTS.SUCCESS,
    data,
    page,
    rowsPerPage,
    totalRows,
    filters,
    sorting,
  }),
  failure: (message) => ({
    type: GET_NATIONAL_SOCIETY_CORRECT_REPORTS.FAILURE,
    message,
  }),
};

export const getIncorrectList = {
  invoke: (nationalSocietyId, pageNumber, filters, sorting) => ({
    type: GET_NATIONAL_SOCIETY_INCORRECT_REPORTS.INVOKE,
    nationalSocietyId,
    pageNumber,
    filters,
    sorting,
  }),
  request: () => ({ type: GET_NATIONAL_SOCIETY_INCORRECT_REPORTS.REQUEST }),
  success: (data, page, rowsPerPage, totalRows, filters, sorting) => ({
    type: GET_NATIONAL_SOCIETY_INCORRECT_REPORTS.SUCCESS,
    data,
    page,
    rowsPerPage,
    totalRows,
    filters,
    sorting,
  }),
  failure: (message) => ({
    type: GET_NATIONAL_SOCIETY_INCORRECT_REPORTS.FAILURE,
    message,
  }),
};
