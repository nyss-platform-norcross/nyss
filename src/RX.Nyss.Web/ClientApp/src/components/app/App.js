import React, { Component } from 'react';
import { Route } from 'react-router';
import { ThemeProvider } from '@material-ui/styles';
import { theme } from './theme';

import './App.scss'
import { Home } from '../home/Home';
import { LoginPage } from '../loginPage/LoginPage';

export default class App extends Component {
  render() {
    return (
      <ThemeProvider theme={theme}>
        <Route exact path='/' component={Home} />
        <Route exact path='/login' component={LoginPage} />
      </ThemeProvider>
    );
  }
}
