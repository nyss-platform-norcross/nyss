import './App.scss'

import React, { Component, Fragment } from 'react';
import { Route } from 'react-router'
import { ConnectedRouter } from 'connected-react-router'
import { ThemeProvider } from '@material-ui/styles';
import { theme } from './theme';
import { Home } from '../homePage/Home';
import { LoginPage } from '../loginPage/LoginPage';
import { AuthRoute } from './AuthRoute';

export default class App extends Component {
  render() {
    return (
      <ThemeProvider theme={theme}>
        <ConnectedRouter history={this.props.history}>
          <Fragment>
            <Route exact path='/login' component={LoginPage} />
            <AuthRoute exact path='/' component={Home} />
            <AuthRoute exact path='/test' component={Home} />
          </Fragment>
        </ConnectedRouter>
      </ThemeProvider>
    );
  }
}
