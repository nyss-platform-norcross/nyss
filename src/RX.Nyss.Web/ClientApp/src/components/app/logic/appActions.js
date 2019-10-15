import * as actions from "./appConstans";

export const initializeApplication = () => (dispatch) =>
    dispatch({ type: actions.INIT_APPLICATION.INVOKE });
