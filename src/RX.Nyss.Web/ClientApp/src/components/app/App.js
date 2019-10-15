import React, { Component, Fragment } from 'react';
import { Route } from 'react-router' // react-router v4/v5
import { ConnectedRouter } from 'connected-react-router'

import { ThemeProvider } from '@material-ui/styles';
import { theme } from './theme';

import './App.scss'
import { Home } from '../home/Home';
import { LoginPage } from '../loginPage/LoginPage';

export default class App extends Component {
    render() {
        return (
            <ThemeProvider theme={theme}>
                <ConnectedRouter history={this.props.history}>
                    <Fragment>
                        <Route exact path='/' component={Home} />
                        <Route exact path='/login' component={LoginPage} />
                    </Fragment>
                </ConnectedRouter>
            </ThemeProvider>
        );
    }
}
