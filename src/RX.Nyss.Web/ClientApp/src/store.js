import { configureStore } from "./redux/configureStore";
import { initialState } from "./initialState";
import history from "./history";

export default configureStore(history, initialState);
