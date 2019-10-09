import 'react-app-polyfill/ie11';
import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';
import App from './components/app/App';

const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
const rootElement = document.getElementById('root');

const render = () => {
  return ReactDOM.render(
    <BrowserRouter basename={baseUrl}>
      <App />
    </BrowserRouter>,
    rootElement
  );
};

render(App);


if (module.hot) {
  module.hot.accept("./components/app/App", () => render());
}
