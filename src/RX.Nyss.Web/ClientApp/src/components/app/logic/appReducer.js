import { initialState } from "../../../initialState";
import * as actions from "./appConstans";

export function appReducer(state = initialState.appData, action) {
    switch (action.type) {
        case actions.INIT_APPLICATION.SUCCESS:
            return {
                ...state,
                appReady: true
            };

        case actions.GET_USER.SUCCESS:
            return {
                ...state,
                user: action.user
                    ? {
                        name: action.user.name,
                        email: action.user.email,
                        roles: action.user.roles
                    }
                    : null
                }

        default:
            return state;
    }
};
