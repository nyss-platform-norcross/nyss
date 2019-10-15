import "regenerator-runtime/runtime";
import { createStore, compose, applyMiddleware, combineReducers } from "redux";
import { rootReducer } from "./reducers";
import thunk from "redux-thunk";
import createSagaMiddleware from "redux-saga";
import { getRootSaga } from "./sagas";
import { connectRouter, routerMiddleware  } from 'connected-react-router'

export const configureStore = (history, initialState) => {
    const composeEnhancers = window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose;

    const reducers = combineReducers({
        ...rootReducer,
        router: connectRouter(history)
    });

    const sagaMiddleware = createSagaMiddleware();
    const store = createStore(
        reducers, initialState,
        composeEnhancers(applyMiddleware(routerMiddleware(history), thunk, sagaMiddleware))
    );

    sagaMiddleware.run(getRootSaga());

    if (module.hot) {
        module.hot.accept("./reducers", () => {
            const reducers = require("./reducers").rootReducer;
            store.replaceReducer({
                ...reducers,
                router: connectRouter(history)
            });
        });
    }

    return store;
}
