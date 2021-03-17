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
import { NationalSocietiesOverviewPage } from '../nationalSocieties/NationalSocietiesOverviewPage';
import { SmsGatewaysListPage } from '../smsGateways/SmsGatewaysListPage';
import { SmsGatewaysCreatePage } from '../smsGateways/SmsGatewaysCreatePage';
import { SmsGatewaysEditPage } from '../smsGateways/SmsGatewaysEditPage';
import { ProjectsListPage } from '../projects/ProjectsListPage';
import { ProjectsCreatePage } from '../projects/ProjectsCreatePage';
import { ProjectsEditPage } from '../projects/ProjectsEditPage';
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
import { AgreementsPage } from '../agreements/AgreementsPage';
import { DataCollectorsListPage } from '../dataCollectors/DataCollectorsListPage';
import { DataCollectorsMapOverviewPage } from '../dataCollectors/DataCollectorsMapOverviewPage';
import { DataCollectorsCreatePage } from '../dataCollectors/DataCollectorsCreatePage';
import { DataCollectorsEditPage } from '../dataCollectors/DataCollectorsEditPage';
import { NationalSocietyStructurePage } from '../nationalSocietyStructure/NationalSocietyStructurePage';
import { ReportsListPage } from '../reports/ReportsListPage';
import { ReportsEditPage } from '../reports/ReportsEditPage';
import { ProjectDashboardPage } from '../projectDashboard/ProjectsDashboardPage';
import { MuiPickersUtilsProvider } from '@material-ui/pickers';
import DayJsUtils from '@date-io/dayjs';
import { AlertsListPage } from '../alerts/AlertsListPage';
import { AlertsAssessmentPage } from '../alerts/AlertsAssessmentPage';
import { NationalSocietyReportsListPage } from '../nationalSocietyReports/NationalSocietyReportsListPage';
import { ProjectsOverviewPage } from '../projects/ProjectsOverviewPage';
import { DataCollectorsPerformancePage } from '../dataCollectors/DataCollectorsPerformancePage';
import { AlertsLogsPage } from '../alerts/AlertsLogsPage';
import { NationalSocietyDashboardPage } from '../nationalSocietyDashboard/NationalSocietyDashboardPage';
import { TranslationsListPage } from '../translations/TranslationsListPage';
import { NotFoundPage } from '../layout/NotFoundPage';
import { OrganizationsListPage } from '../organizations/OrganizationsListPage';
import { OrganizationsCreatePage } from '../organizations/OrganizationsCreatePage';
import { OrganizationsEditPage } from '../organizations/OrganizationsEditPage';
import { ProjectOrganizationsListPage } from '../projectOrganizations/ProjectOrganizationsListPage';
import { ProjectOrganizationsCreatePage } from '../projectOrganizations/ProjectOrganizationsCreatePage';
import { ProjectAlertRecipientsCreatePage } from '../projectAlertRecipients/ProjectAlertRecipientsCreatePage';
import { ProjectAlertRecipientsEditPage } from '../projectAlertRecipients/ProjectAlertRecipientsEditPage';
import { EmailTranslationsListPage } from '../translations/EmailTranslationsListPage';
import { SmsTranslationsListPage } from '../translations/SmsTranslationsListPage';
import { ProjectAlertNotificationsPage } from '../projects/ProjectAlertNotificationPage';

export const App = ({ history }) => (
  <ThemeProvider theme={theme}>
    <MuiPickersUtilsProvider utils={DayJsUtils}>
      <ConnectedRouter history={history}>
        <Switch>
          <Route path='/login' component={LoginPage} />
          <Route path='/verifyEmail' component={VerifyEmailPage} />
          <Route path='/resetPassword' component={ResetPasswordPage} />
          <Route path='/resetPasswordCallback' component={ResetPasswordCallbackPage} />

          <AuthRoute exact path='/' component={Home} />
          <AuthRoute exact path='/agreements' component={AgreementsPage} ignoreRedirection />

          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/dashboard' component={NationalSocietyDashboardPage} roles={accessMap.nationalSocieties.showDashboard} />
          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/overview' component={NationalSocietiesOverviewPage} roles={accessMap.nationalSocieties.edit} />
          <AuthRoute exact path='/nationalsocieties' component={NationalSocietiesListPage} roles={accessMap.nationalSocieties.list} />
          <AuthRoute exact path='/nationalsocieties/add' component={NationalSocietiesCreatePage} roles={accessMap.nationalSocieties.add} />
          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/edit' component={NationalSocietiesEditPage} roles={accessMap.nationalSocieties.edit} />
          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/structure' component={NationalSocietyStructurePage} roles={accessMap.nationalSocieties.edit} />
          <Redirect exact from='/nationalsocieties/:nationalSocietyId' to='/nationalsocieties/:nationalSocietyId/dashboard' />
          <Redirect exact from='/nationalsocieties/:nationalSocietyId/settings' to='/nationalsocieties/:nationalSocietyId/overview' />

          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/reports' component={NationalSocietyReportsListPage} roles={accessMap.nationalSocietyReports.list} />

          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/smsgateways' component={SmsGatewaysListPage} roles={accessMap.smsGateways.list} />
          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/smsgateways/add' component={SmsGatewaysCreatePage} roles={accessMap.smsGateways.add} />
          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/smsgateways/:smsGatewayId/edit' component={SmsGatewaysEditPage} roles={accessMap.smsGateways.edit} />

          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/organizations' component={OrganizationsListPage} roles={accessMap.organizations.list} />
          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/organizations/add' component={OrganizationsCreatePage} roles={accessMap.organizations.add} />
          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/organizations/:organizationId/edit' component={OrganizationsEditPage} roles={accessMap.organizations.edit} />

          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/projects/:projectId/dashboard' component={ProjectDashboardPage} roles={accessMap.projects.showDashboard} />
          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/projects' component={ProjectsListPage} roles={accessMap.projects.list} />
          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/projects/add' component={ProjectsCreatePage} roles={accessMap.projects.add} />
          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/projects/:projectId/edit' component={ProjectsEditPage} roles={accessMap.projects.edit} />
          <Redirect exact from='/nationalsocieties/:nationalSocietyId/projects/:projectId/settings' to='/nationalsocieties/:nationalSocietyId/projects/:projectId/overview' roles={accessMap.projects.showOverview} />
          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/projects/:projectId/overview' component={ProjectsOverviewPage} roles={accessMap.projects.showOverview} />
          <Redirect exact from='/nationalsocieties/:nationalSocietyId/projects/:projectId' to='/nationalsocieties/:nationalSocietyId/projects/:projectId/dashboard' />

          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/users' component={NationalSocietyUsersListPage} roles={accessMap.nationalSocietyUsers.list} />
          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/users/add' component={NationalSocietyUsersCreatePage} roles={accessMap.nationalSocietyUsers.add} />
          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/users/addExisting' component={NationalSocietyUsersAddExistingPage} roles={accessMap.nationalSocietyUsers.add} />
          <AuthRoute exact path='/nationalsocieties/:nationalSocietyId/users/:nationalSocietyUserId/edit' component={NationalSocietyUsersEditPage} roles={accessMap.nationalSocietyUsers.edit} />

          <AuthRoute exact path='/globalcoordinators' component={GlobalCoordinatorsListPage} roles={accessMap.globalCoordinators.list} />
          <AuthRoute exact path='/globalcoordinators/add' component={GlobalCoordinatorsCreatePage} roles={accessMap.globalCoordinators.add} />
          <AuthRoute exact path='/globalcoordinators/:globalCoordinatorId/edit' component={GlobalCoordinatorsEditPage} roles={accessMap.globalCoordinators.edit} />

          <Redirect exact from='/projects/:projectId/datacollectors' to='/projects/:projectId/datacollectors/list' />
          <AuthRoute exact path='/projects/:projectId/datacollectors/mapoverview' component={DataCollectorsMapOverviewPage} roles={accessMap.dataCollectors.list} />

          <AuthRoute exact path='/healthrisks' component={HealthRisksListPage} roles={accessMap.healthRisks.list} />
          <AuthRoute exact path='/healthrisks/add' component={HealthRisksCreatePage} roles={accessMap.healthRisks.add} />
          <AuthRoute exact path='/healthrisks/:healthRiskId/edit' component={HealthRisksEditPage} roles={accessMap.healthRisks.edit} />

          <AuthRoute exact path='/projects/:projectId/datacollectors/list' component={DataCollectorsListPage} roles={accessMap.dataCollectors.list} />
          <AuthRoute exact path='/projects/:projectId/datacollectors/performance' component={DataCollectorsPerformancePage} roles={accessMap.dataCollectors.performanceList} />
          <AuthRoute exact path='/projects/:projectId/datacollectors/add' component={DataCollectorsCreatePage} roles={accessMap.dataCollectors.add} />
          <AuthRoute exact path='/projects/:projectId/datacollectors/:dataCollectorId/edit' component={DataCollectorsEditPage} roles={accessMap.dataCollectors.edit} />

          <AuthRoute exact path='/projects/:projectId/organizations' component={ProjectOrganizationsListPage} roles={accessMap.projectOrganizations.list} />
          <AuthRoute exact path='/projects/:projectId/organizations/add' component={ProjectOrganizationsCreatePage} roles={accessMap.projectOrganizations.add} />

          <AuthRoute exact path='/projects/:projectId/alertNotifications' component={ProjectAlertNotificationsPage} roles={accessMap.projectAlertNotifications.list} />
          <AuthRoute exact path='/projects/:projectId/alertRecipients/add' component={ProjectAlertRecipientsCreatePage} roles={accessMap.projectAlertNotifications.addRecipient} />
          <AuthRoute exact path='/projects/:projectId/alertRecipients/:alertRecipientId/edit' component={ProjectAlertRecipientsEditPage} roles={accessMap.projectAlertNotifications.editRecipient} />

          <AuthRoute exact path='/projects/:projectId/reports' component={ReportsListPage} roles={accessMap.reports.list} />
          <AuthRoute exact path='/projects/:projectId/reports/:reportId/edit' component={ReportsEditPage} roles={accessMap.reports.edit} />

          <AuthRoute exact path='/projects/:projectId/alerts' component={AlertsListPage} roles={accessMap.alerts.list} />
          <AuthRoute exact path='/projects/:projectId/alerts/:alertId/assess' component={AlertsAssessmentPage} roles={accessMap.alerts.assess} />
          <AuthRoute exact path='/projects/:projectId/alerts/:alertId/logs' component={AlertsLogsPage} roles={accessMap.alerts.assess} />

          <AuthRoute exact path='/translations' component={TranslationsListPage} roles={accessMap.translations.list} />
          <AuthRoute exact path='/emailTranslations' component={EmailTranslationsListPage} roles={accessMap.translations.list} />
          <AuthRoute exact path='/smsTranslations' component={SmsTranslationsListPage} roles={accessMap.translations.list} />

          <Route component={NotFoundPage} />
        </Switch>
      </ConnectedRouter>
    </MuiPickersUtilsProvider>
  </ThemeProvider>
);

export default App;
