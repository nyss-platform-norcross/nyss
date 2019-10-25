import 'react-app-polyfill/ie11';
import 'react-app-polyfill/stable';
import React from 'react';
import ReactDOM from 'react-dom';
import App from './components/app/App';
import { createBrowserHistory } from 'history'
import { Provider } from "react-redux";
import { configureStore } from './store/configureStore';
import { initialState } from './initialState';
import * as appActions from './components/app/logic/appActions';

const history = createBrowserHistory();
const store = configureStore(history, initialState);

store.dispatch(appActions.initApplication.invoke());

const render = () => {
    return ReactDOM.render(
        <Provider store={store}>
            <App history={history} />
        </Provider>,
        document.getElementById('root')
    );
};

render();

if (module.hot) {
    module.hot.accept("./components/app/App", () => render());
}
