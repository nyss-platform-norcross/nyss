import { call, put, takeEvery } from "redux-saga/effects";
import * as consts from "./nationalSocietiesConstants";
import * as actions from "./nationalSocietiesActions";
import * as http from "../../../utils/http";

export const nationalSocietiesSagas = () => [
  takeEvery(consts.GET_NATIONAL_SOCIETIES.INVOKE, getNationalSocieties)
];

function* getNationalSocieties() {
  yield put(actions.getList.request());
  try {
    // yield call(http.get, "/api/nationalSocieties/get");
    const response = {
      isSuccess: true,
      value: [
        { id: 1, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
        { id: 2, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
        { id: 3, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
        { id: 4, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
        { id: 5, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
        { id: 6, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
        { id: 7, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
        { id: 8, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
        { id: 9, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
        { id: 10, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
        { id: 11, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
        { id: 12, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
        { id: 13, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
        { id: 14, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
        { id: 15, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
        { id: 16, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" }
      ],
    }

    yield put(actions.getList.success(response.value));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
};
