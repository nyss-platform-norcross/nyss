import "react-app-polyfill/ie11";
import "react-app-polyfill/stable";
import React from "react";
import ReactDOM from "react-dom";
import App from "./components/app/App";
import { Provider } from "react-redux";
import { runApplication } from "./components/app/logic/appService";
import history from "./history";
import store from "./store";

const render = () => {
  return ReactDOM.render(
    <Provider store={store}>
      <App history={history} />
    </Provider>,
    document.getElementById("root"),
  );
};

runApplication(store.dispatch).finally(render);

if (module.hot) {
  module.hot.accept("./components/app/App", () => render());
}
