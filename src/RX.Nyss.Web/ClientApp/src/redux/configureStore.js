import "regenerator-runtime/runtime";
import { createStore, compose, applyMiddleware } from "redux";
import { createRootReducer } from "./reducers";
import thunk from "redux-thunk";
import createSagaMiddleware from "redux-saga";
import { getRootSaga } from "./sagas";
import { routerMiddleware } from "connected-react-router";

export const configureStore = (history, initialState) => {
  const composeEnhancers =
    process.env.NODE_ENV === "production"
      ? compose
      : window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose;

  const sagaMiddleware = createSagaMiddleware();
  const store = createStore(
    createRootReducer(history),
    initialState,
    composeEnhancers(
      applyMiddleware(routerMiddleware(history), thunk, sagaMiddleware),
    ),
  );

  sagaMiddleware.run(getRootSaga());

  if (module.hot) {
    module.hot.accept("./reducers", () => {
      const reducers = require("./reducers");
      store.replaceReducer(reducers.createRootReducer(history));
    });
  }

  return store;
};
