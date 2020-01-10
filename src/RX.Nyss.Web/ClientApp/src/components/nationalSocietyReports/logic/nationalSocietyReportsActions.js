import { push } from "connected-react-router";
import {
  OPEN_NATIONAL_SOCIETY_REPORTS_LIST, GET_NATIONAL_SOCIETY_REPORTS
} from "./nationalSocietyReportsConstants";

export const goToList = (nationalSocietyId) => push(`/nationalsociety/${nationalSocietyId}/reports`);

export const openList = {
  invoke: (nationalSocietyId) => ({ type: OPEN_NATIONAL_SOCIETY_REPORTS_LIST.INVOKE, nationalSocietyId }),
  request: () => ({ type: OPEN_NATIONAL_SOCIETY_REPORTS_LIST.REQUEST }),
  success: (nationalSocietyId, filtersData) => ({ type: OPEN_NATIONAL_SOCIETY_REPORTS_LIST.SUCCESS, nationalSocietyId, filtersData }),
  failure: (message) => ({ type: OPEN_NATIONAL_SOCIETY_REPORTS_LIST.FAILURE, message })
};

export const getList = {
  invoke: (nationalSocietyId, pageNumber, filters, sorting) => ({ type: GET_NATIONAL_SOCIETY_REPORTS.INVOKE, nationalSocietyId, pageNumber, filters, sorting }),
  request: () => ({ type: GET_NATIONAL_SOCIETY_REPORTS.REQUEST }),
  success: (data, page, rowsPerPage, totalRows, filters, sorting) => ({ type: GET_NATIONAL_SOCIETY_REPORTS.SUCCESS, data, page, rowsPerPage, totalRows, filters, sorting }),
  failure: (message) => ({ type: GET_NATIONAL_SOCIETY_REPORTS.FAILURE, message })
};
