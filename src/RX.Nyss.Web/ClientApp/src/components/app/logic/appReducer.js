import { initialState } from "../../../initialState";
import * as actions from "./appConstans";

export function appReducer(state = initialState.appData, action) {
    switch (action.type) {
        case actions.INIT_APPLICATION.SUCCESS:
            return {
                ...state,
                appReady: true
            };

        default:
            return state;
    }
};
