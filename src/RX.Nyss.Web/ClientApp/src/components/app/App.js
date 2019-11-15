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
import { ProjectsListPage } from '../projects/ProjectsListPage';
import { ProjectsCreatePage } from '../projects/ProjectsCreatePage';
import { ProjectsEditPage } from '../projects/ProjectsEditPage';
import { ProjectsDashboardPage } from '../projects/ProjectsDashboardPage';
import { VerifyEmailPage } from '../verifyEmailPage/VerifyEmailPage';
import { GlobalCoordinatorsListPage } from '../globalCoordinators/GlobalCoordinatorsListPage';
import { GlobalCoordinatorsCreatePage } from '../globalCoordinators/GlobalCoordinatorsCreatePage';
import { GlobalCoordinatorsEditPage } from '../globalCoordinators/GlobalCoordinatorsEditPage';
import { ResetPasswordPage } from '../resetPasswordPage/ResetPasswordPage';
import { ResetPasswordCallbackPage } from '../resetPasswordCallbackPage/ResetPasswordCallbackPage';
import { HealthRisksListPage } from '../healthRisks/HealthRisksListPage';
import { HealthRisksCreatePage } from '../healthRisks/HealthRisksCreatePage';
import { HealthRisksEditPage } from '../healthRisks/HealthRisksEditPage';
import { NationalSocietyUsersListPage } from '../nationalSocietyUsers/NationalSocietyUsersListPage';
import { NationalSocietyUsersCreatePage } from '../nationalSocietyUsers/NationalSocietyUsersCreatePage';
import { NationalSocietyUsersAddExistingPage } from '../nationalSocietyUsers/NationalSocietyUsersAddExistingPage';
import { NationalSocietyUsersEditPage } from '../nationalSocietyUsers/NationalSocietyUsersEditPage';
import { HeadManagerConsentsPage } from '../headManagerConsents/HeadManagerConsentsPage';
import { DataCollectorsListPage } from '../dataCollectors/DataCollectorsListPage';
import { DataCollectorsCreatePage } from '../dataCollectors/DataCollectorsCreatePage';
import { DataCollectorsEditPage } from '../dataCollectors/DataCollectorsEditPage';

export const App = ({ history }) => (
  <ThemeProvider theme={theme}>
    <ConnectedRouter history={history}>
      <Switch>
        <Route path='/login' component={LoginPage} />
        <Route path='/verifyEmail' component={VerifyEmailPage} />
        <Route path='/resetPassword' component={ResetPasswordPage} />
        <Route path='/resetPasswordCallback' component={ResetPasswordCallbackPage} />

        <AuthRoute exact path='/' component={Home} />
        <AuthRoute exact path='/headManagerConsents' component={HeadManagerConsentsPage} />

        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/dashboard' component={NationalSocietiesDashboardPage} roles={accessMap.nationalSocieties.get} />
        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/overview' component={NationalSocietiesOverviewPage} roles={accessMap.nationalSocieties.edit} />
        <AuthRoute exact path='/nationalsocieties' component={NationalSocietiesListPage} roles={accessMap.nationalSocieties.list} />
        <AuthRoute exact path='/nationalsocieties/add' component={NationalSocietiesCreatePage} roles={accessMap.nationalSocieties.add} />
        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/edit' component={NationalSocietiesEditPage} roles={accessMap.nationalSocieties.edit} />
        <Redirect exact from='/nationalsocieties/:nationalSocietyId' to='/nationalsocieties/:nationalSocietyId/dashboard' />

        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/smsgateways' component={SmsGatewaysListPage} roles={accessMap.smsGateways.list} />
        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/smsgateways/add' component={SmsGatewaysCreatePage} roles={accessMap.smsGateways.add} />
        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/smsgateways/:smsGatewayId/edit' component={SmsGatewaysEditPage} roles={accessMap.smsGateways.edit} />

        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/projects/:projectId/dashboard' component={ProjectsDashboardPage} roles={accessMap.projects.get} />
        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/projects' component={ProjectsListPage} roles={accessMap.projects.list} />
        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/projects/add' component={ProjectsCreatePage} roles={accessMap.projects.add} />
        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/projects/:projectId/edit' component={ProjectsEditPage} roles={accessMap.projects.edit} />
        <Redirect exact from='/nationalsocieties/:nationalSocietyId/projects/:projectId' to='/nationalsocieties/:nationalSocietyId/projects/:projectId/dashboard' />

        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/users' component={NationalSocietyUsersListPage} roles={accessMap.nationalSocietyUsers.list} />
        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/users/add' component={NationalSocietyUsersCreatePage} roles={accessMap.nationalSocietyUsers.add} />
        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/users/addExisting' component={NationalSocietyUsersAddExistingPage} roles={accessMap.nationalSocietyUsers.add} />
        <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/users/:nationalSocietyUserId/edit' component={NationalSocietyUsersEditPage} roles={accessMap.nationalSocietyUsers.edit} />

        <AuthRoute exact path='/globalcoordinators' component={GlobalCoordinatorsListPage} roles={accessMap.globalCoordinators.list} />
        <AuthRoute exact path='/globalcoordinators/add' component={GlobalCoordinatorsCreatePage} roles={accessMap.globalCoordinators.add} />
        <AuthRoute exact path='/globalcoordinators/:globalCoordinatorId/edit' component={GlobalCoordinatorsEditPage} roles={accessMap.globalCoordinators.edit} />

        <AuthRoute exact path='/healthrisks' component={HealthRisksListPage} roles={accessMap.healthRisks.list} />
        <AuthRoute exact path='/healthrisks/add' component={HealthRisksCreatePage} roles={accessMap.healthRisks.add} />
        <AuthRoute exact path='/healthrisks/:healthRiskId/edit' component={HealthRisksEditPage} roles={accessMap.healthRisks.edit} />

        <AuthRoute exact path='/projects/:projectId/datacollectors' component={DataCollectorsListPage} roles={accessMap.dataCollectors.list} />
        <AuthRoute exact path='/projects/:projectId/datacollectors/add' component={DataCollectorsCreatePage} roles={accessMap.dataCollectors.add} />
        <AuthRoute exact path='/projects/:projectId/datacollectors/:dataCollectorId/edit' component={DataCollectorsEditPage} roles={accessMap.dataCollectors.edit} />
      </Switch>
    </ConnectedRouter>
  </ThemeProvider>
);

export default App;
