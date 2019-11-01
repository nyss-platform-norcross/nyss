import './App.scss'

import React from 'react';
import { Route, Switch, Redirect } from 'react-router'
import { ThemeProvider } from '@material-ui/styles';
import { theme } from './theme';
import { Home } from '../homePage/Home';
import { LoginPage } from '../loginPage/LoginPage';
import { AuthRoute } from './AuthRoute';
import { ConnectedRouter } from 'connected-react-router'
import { accessMap } from '../../authentication/accessMap';
import { NationalSocietiesListPage } from '../nationalSocieties/NationalSocietiesListPage';
import { NationalSocietiesCreatePage } from '../nationalSocieties/NationalSocietiesCreatePage';
import { NationalSocietiesEditPage } from '../nationalSocieties/NationalSocietiesEditPage';
import { NationalSocietiesDashboardPage } from '../nationalSocieties/NationalSocietiesDashboardPage';
import { NationalSocietiesOverviewPage } from '../nationalSocieties/NationalSocietiesOverviewPage';
import { SmsGatewaysListPage } from '../smsGateways/SmsGatewaysListPage';
import { SmsGatewaysCreatePage } from '../smsGateways/SmsGatewaysCreatePage';
import { SmsGatewaysEditPage } from '../smsGateways/SmsGatewaysEditPage';
import { VerifyEmailPage } from '../verifyEmailPage/VerifyEmailPage';
import { GlobalCoordinatorsListPage } from '../globalCoordinators/GlobalCoordinatorsListPage';
import { GlobalCoordinatorsCreatePage } from '../globalCoordinators/GlobalCoordinatorsCreatePage';
import { GlobalCoordinatorsEditPage } from '../globalCoordinators/GlobalCoordinatorsEditPage';
import { ResetPasswordPage } from '../resetPasswordPage/ResetPasswordPage';
import { ResetPasswordCallbackPage } from '../resetPasswordCallbackPage/ResetPasswordCallbackPage';

export const App = ({ history }) => (
  <ThemeProvider theme={theme}>
    <ConnectedRouter history={history}>
      <Switch>
        <Route path='/login' component={LoginPage} />
        <Route path='/verifyEmail' component={VerifyEmailPage} />
        <Route path='/resetPassword' component={ResetPasswordPage} />
        <Route path='/resetPasswordCallback' component={ResetPasswordCallbackPage} />

        <AuthRoute exact path='/' component={Home} />
        <AuthRoute exact path='/nationalsocieties' component={NationalSocietiesListPage} roles={accessMap.nationalSocieties.list} />
        <AuthRoute exact path='/nationalsocieties/add' component={NationalSocietiesCreatePage} roles={accessMap.nationalSocieties.add} />
        <Redirect exact from="/nationalsocieties/:nationalSocietyId" to="/nationalsocieties/:nationalSocietyId/dashboard" />
        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/dashboard' component={NationalSocietiesDashboardPage} roles={accessMap.nationalSocieties.add} />
        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/overview' component={NationalSocietiesOverviewPage} roles={accessMap.nationalSocieties.add} />
        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/edit' component={NationalSocietiesEditPage} roles={accessMap.nationalSocieties.edit} />

        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/smsgateways' component={SmsGatewaysListPage} roles={accessMap.smsGateways.list} />
        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/smsgateways/add' component={SmsGatewaysCreatePage} roles={accessMap.smsGateways.add} />
        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/smsgateways/:smsGatewayId/edit' component={SmsGatewaysEditPage} roles={accessMap.smsGateways.edit} />

        <AuthRoute exact path='/globalcoordinators' component={GlobalCoordinatorsListPage} roles={accessMap.globalCoordinators.list} />
        <AuthRoute exact path='/globalcoordinators/add' component={GlobalCoordinatorsCreatePage} roles={accessMap.globalCoordinators.add} />
        <AuthRoute exact path='/globalcoordinators/:globalCoordinatorId/edit' component={GlobalCoordinatorsEditPage} roles={accessMap.globalCoordinators.edit} />
      </Switch>
    </ConnectedRouter>
  </ThemeProvider>
);

export default App;
