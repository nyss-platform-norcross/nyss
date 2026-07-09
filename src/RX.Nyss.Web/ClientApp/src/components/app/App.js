import "./App.scss";

import React from "react";
import { Route, Switch, Redirect } from "react-router";
import { ThemeProvider } from "@material-ui/styles";
import { theme } from "./theme";
import { Home } from "../homePage/Home";
import { LoginPage } from "../loginPage/LoginPage";
import { AuthRoute } from "./AuthRoute";
import { ConnectedRouter } from "connected-react-router";
import { accessMap } from "../../authentication/accessMap";
import { NationalSocietiesListPage } from "../nationalSocieties/NationalSocietiesListPage";
import { NationalSocietiesCreatePage } from "../nationalSocieties/NationalSocietiesCreatePage";
import { NationalSocietiesEditPage } from "../nationalSocieties/NationalSocietiesEditPage";
import { NationalSocietiesOverviewPage } from "../nationalSocieties/NationalSocietiesOverviewPage";
import { SmsGatewaysListPage } from "../smsGateways/SmsGatewaysListPage";
import { SmsGatewaysCreatePage } from "../smsGateways/SmsGatewaysCreatePage";
import { SmsGatewaysEditPage } from "../smsGateways/SmsGatewaysEditPage";
import { ProjectsListPage } from "../projects/ProjectsListPage";
import { ProjectsCreatePage } from "../projects/ProjectsCreatePage";
import { ProjectsEditPage } from "../projects/ProjectsEditPage";
import { VerifyEmailPage } from "../verifyEmailPage/VerifyEmailPage";
import { GlobalCoordinatorsListPage } from "../globalCoordinators/GlobalCoordinatorsListPage";
import { GlobalCoordinatorsCreatePage } from "../globalCoordinators/GlobalCoordinatorsCreatePage";
import { GlobalCoordinatorsEditPage } from "../globalCoordinators/GlobalCoordinatorsEditPage";
import { ResetPasswordPage } from "../resetPasswordPage/ResetPasswordPage";
import { ResetPasswordCallbackPage } from "../resetPasswordCallbackPage/ResetPasswordCallbackPage";
import { HealthRisksListPage } from "../healthRisks/HealthRisksListPage";
import { HealthRisksCreatePage } from "../healthRisks/HealthRisksCreatePage";
import { HealthRisksEditPage } from "../healthRisks/HealthRisksEditPage";
import { SuspectedDiseaseListPage } from "../suspectedDiseases/SuspectedDiseaseListPage";
import { SuspectedDiseaseCreatePage } from "../suspectedDiseases/SuspectedDiseaseCreatePage";
import { SuspectedDiseaseEditPage } from "../suspectedDiseases/SuspectedDiseaseEditPage";
import { NationalSocietyUsersListPage } from "../nationalSocietyUsers/NationalSocietyUsersListPage";
import { NationalSocietyUsersCreatePage } from "../nationalSocietyUsers/NationalSocietyUsersCreatePage";
import { NationalSocietyUsersAddExistingPage } from "../nationalSocietyUsers/NationalSocietyUsersAddExistingPage";
import { NationalSocietyUsersEditPage } from "../nationalSocietyUsers/NationalSocietyUsersEditPage";
import { AgreementsPage } from "../agreements/AgreementsPage";
import { DataCollectorsListPage } from "../dataCollectors/DataCollectorsListPage";
import { DataCollectorsMapOverviewPage } from "../dataCollectors/DataCollectorsMapOverviewPage";
import { DataCollectorsCreatePage } from "../dataCollectors/DataCollectorsCreatePage";
import { DataCollectorsEditPage } from "../dataCollectors/DataCollectorsEditPage";
import { NationalSocietyStructurePage } from "../nationalSocietyStructure/NationalSocietyStructurePage";
import { CorrectReportsListPage } from "../reports/CorrectReportsListPage";
import { ReportsEditPage } from "../reports/ReportsEditPage";
import { ProjectDashboardPage } from "../projectDashboard/ProjectsDashboardPage";
import { MuiPickersUtilsProvider } from "@material-ui/pickers";
import DayJsUtils from "@date-io/dayjs";
import { AlertsListPage } from "../alerts/AlertsListPage";
import { AlertsAssessmentPage } from "../alerts/AlertsAssessmentPage";
import { ProjectsOverviewPage } from "../projects/ProjectsOverviewPage";
import { ProjectHealthRisksPage } from "../projects/ProjectHealthRisksPage";
import { ProjectHealthRisksEditPage } from "../projects/ProjectHealthRisksEditPage";
import { DataCollectorsPerformancePage } from "../dataCollectors/DataCollectorsPerformancePage";
import { NationalSocietyDashboardPage } from "../nationalSocietyDashboard/NationalSocietyDashboardPage";
import { TranslationsListPage } from "../translations/TranslationsListPage";
import { NotFoundPage } from "../layout/NotFoundPage";
import { OrganizationsListPage } from "../organizations/OrganizationsListPage";
import { OrganizationsCreatePage } from "../organizations/OrganizationsCreatePage";
import { OrganizationsEditPage } from "../organizations/OrganizationsEditPage";
import { ProjectOrganizationsListPage } from "../projectOrganizations/ProjectOrganizationsListPage";
import { ProjectOrganizationsCreatePage } from "../projectOrganizations/ProjectOrganizationsCreatePage";
import { EmailTranslationsListPage } from "../translations/EmailTranslationsListPage";
import { SmsTranslationsListPage } from "../translations/SmsTranslationsListPage";
import { ProjectAlertRecipientsCreatePage } from "../projectAlertRecipients/ProjectAlertRecipientsCreatePage";
import { ProjectAlertRecipientsEditPage } from "../projectAlertRecipients/ProjectAlertRecipientsEditPage";
import { IncorrectReportsListPage } from "../reports/IncorrectReportsListPage";
import { NationalSocietyCorrectReportsListPage } from "../nationalSocietyReports/NationalSocietyCorrectReportsListPage";
import { NationalSocietyIncorrectReportsListPage } from "../nationalSocietyReports/NationalSocietyIncorrectReportsListPage";
import { AlertEventsLogPage } from "../alertEvents/AlertEventsLogPage";
import { ProjectErrorMessagesPage } from "../projects/ProjectErrorMessagesPage";
import { useSelector } from "react-redux";
import { useEffect } from "react";
import { EidsrIntegrationPage } from "../eidsrIntegration/EidsrIntegrationPage";
import { EidsrIntegrationEditPage } from "../eidsrIntegration/EidsrIntegrationEditPage";
import { FeedbackPage } from "../feedback/FeedbackPage";
import { ProjectAlertNotHandledRecipientsPage } from "../projectAlertNotHandledRecipient/ProjectAlertNotHandledRecipientsPage";
import { ProjectAlertRecipientsListPage } from "../projectAlertRecipients/ProjectAlertRecipientsListPage";
import { ProjectSetupPage } from "../projectSetup/ProjectSetupPage";
import { Administrator } from "../../authentication/roles";
import { ApplicationInsightsProvider } from "./ApplicationInsightsProvider";

export const App = ({ history }) => {
  const direction = useSelector((state) => state.appData.direction);

  useEffect(() => (document.body.dir = direction), [direction]);

  const AIConnectionString = useSelector(
    (state) => state.appData.applicationInsightsConnectionString,
  );

  return (
    <ApplicationInsightsProvider connectionString={AIConnectionString}>
      <ThemeProvider theme={theme(direction)}>
        <MuiPickersUtilsProvider utils={DayJsUtils}>
          <ConnectedRouter history={history}>
            <Switch>
              <Route path="/login" component={LoginPage} />
              <Route path="/verifyEmail" component={VerifyEmailPage} />
              <Route path="/resetPassword" component={ResetPasswordPage} />
              <Route
                path="/resetPasswordCallback"
                component={ResetPasswordCallbackPage}
              />

              <AuthRoute exact path="/" component={Home} />
              <AuthRoute
                exact
                path="/agreements"
                component={AgreementsPage}
                ignoreRedirection
              />

              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/dashboard"
                component={NationalSocietyDashboardPage}
                roles={accessMap.nationalSocieties.showDashboard}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/overview"
                component={NationalSocietiesOverviewPage}
                roles={accessMap.nationalSocieties.edit}
              />
              <AuthRoute
                exact
                path="/nationalsocieties"
                component={NationalSocietiesListPage}
                roles={accessMap.nationalSocieties.list}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/add"
                component={NationalSocietiesCreatePage}
                roles={accessMap.nationalSocieties.add}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/edit"
                component={NationalSocietiesEditPage}
                roles={accessMap.nationalSocieties.edit}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/structure"
                component={NationalSocietyStructurePage}
                roles={accessMap.nationalSocieties.edit}
              />
              <Redirect
                exact
                from="/nationalsocieties/:nationalSocietyId"
                to="/nationalsocieties/:nationalSocietyId/dashboard"
              />
              <Redirect
                exact
                from="/nationalsocieties/:nationalSocietyId/settings"
                to="/nationalsocieties/:nationalSocietyId/overview"
              />

              <Redirect
                exact
                from="/nationalsocieties/:nationalSocietyId/reports"
                to="/nationalsocieties/:nationalSocietyId/reports/correct"
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/reports/correct"
                component={NationalSocietyCorrectReportsListPage}
                roles={accessMap.nationalSocietyReports.list}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/reports/incorrect"
                component={NationalSocietyIncorrectReportsListPage}
                roles={accessMap.nationalSocietyReports.list}
              />

              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/smsgateways"
                component={SmsGatewaysListPage}
                roles={accessMap.smsGateways.list}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/smsgateways/add"
                component={SmsGatewaysCreatePage}
                roles={accessMap.smsGateways.add}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/smsgateways/:smsGatewayId/edit"
                component={SmsGatewaysEditPage}
                roles={accessMap.smsGateways.edit}
              />

              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/eidsrintegration"
                component={EidsrIntegrationPage}
                roles={accessMap.eidsrIntegration.get}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/eidsrintegration/edit"
                component={EidsrIntegrationEditPage}
                roles={accessMap.eidsrIntegration.edit}
              />

              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/organizations"
                component={OrganizationsListPage}
                roles={accessMap.organizations.list}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/organizations/add"
                component={OrganizationsCreatePage}
                roles={accessMap.organizations.add}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/organizations/:organizationId/edit"
                component={OrganizationsEditPage}
                roles={accessMap.organizations.edit}
              />

              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/projects/:projectId/dashboard"
                component={ProjectDashboardPage}
                roles={accessMap.projects.showDashboard}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/projects"
                component={ProjectsListPage}
                roles={accessMap.projects.list}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/projects/add"
                component={ProjectsCreatePage}
                roles={accessMap.projects.add}
              />
              {/* Project Setup route is limited to administrators only while under development */}
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/projects/setup"
                component={ProjectSetupPage}
                roles={[Administrator]}
              />

              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/projects/:projectId/edit"
                component={ProjectsEditPage}
                roles={accessMap.projects.edit}
              />
              <Redirect
                exact
                from="/nationalsocieties/:nationalSocietyId/projects/:projectId/settings"
                to="/nationalsocieties/:nationalSocietyId/projects/:projectId/overview"
                roles={accessMap.projects.overview}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/projects/:projectId/overview"
                component={ProjectsOverviewPage}
                roles={accessMap.projects.overview}
              />
              <Redirect
                exact
                from="/nationalsocieties/:nationalSocietyId/projects/:projectId"
                to="/nationalsocieties/:nationalSocietyId/projects/:projectId/dashboard"
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/projects/:projectId/healthrisks"
                component={ProjectHealthRisksPage}
                roles={accessMap.projects.projectHealthRisks}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/projects/:projectId/editHealthRisks"
                component={ProjectHealthRisksEditPage}
                roles={accessMap.projects.edit}
              />

              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/users"
                component={NationalSocietyUsersListPage}
                roles={accessMap.nationalSocietyUsers.list}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/users/add"
                component={NationalSocietyUsersCreatePage}
                roles={accessMap.nationalSocietyUsers.add}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/users/addExisting"
                component={NationalSocietyUsersAddExistingPage}
                roles={accessMap.nationalSocietyUsers.add}
              />
              <AuthRoute
                exact
                path="/nationalsocieties/:nationalSocietyId/users/:nationalSocietyUserId/edit"
                component={NationalSocietyUsersEditPage}
                roles={accessMap.nationalSocietyUsers.edit}
              />

              <AuthRoute
                exact
                path="/globalcoordinators"
                component={GlobalCoordinatorsListPage}
                roles={accessMap.globalCoordinators.list}
              />
              <AuthRoute
                exact
                path="/globalcoordinators/add"
                component={GlobalCoordinatorsCreatePage}
                roles={accessMap.globalCoordinators.add}
              />
              <AuthRoute
                exact
                path="/globalcoordinators/:globalCoordinatorId/edit"
                component={GlobalCoordinatorsEditPage}
                roles={accessMap.globalCoordinators.edit}
              />

              <AuthRoute
                exact
                path="/healthrisks"
                component={HealthRisksListPage}
                roles={accessMap.healthRisks.list}
              />
              <AuthRoute
                exact
                path="/healthrisks/add"
                component={HealthRisksCreatePage}
                roles={accessMap.healthRisks.add}
              />
              <AuthRoute
                exact
                path="/healthrisks/:healthRiskId/edit"
                component={HealthRisksEditPage}
                roles={accessMap.healthRisks.edit}
              />

              <AuthRoute
                exact
                path="/suspecteddiseases"
                component={SuspectedDiseaseListPage}
                roles={accessMap.suspectedDiseases.list}
              />
              <AuthRoute
                exact
                path="/suspecteddiseases/add"
                component={SuspectedDiseaseCreatePage}
                roles={accessMap.suspectedDiseases.add}
              />
              <AuthRoute
                exact
                path="/suspecteddiseases/:suspecteddiseaseId/edit"
                component={SuspectedDiseaseEditPage}
                roles={accessMap.suspectedDiseases.edit}
              />

              <AuthRoute
                exact
                path="/feedback"
                component={FeedbackPage}
                roles={accessMap.feedback.send}
              />

              <Redirect
                exact
                from="/projects/:projectId/datacollectors"
                to="/projects/:projectId/datacollectors/list"
              />
              <AuthRoute
                exact
                path="/projects/:projectId/datacollectors/mapoverview"
                component={DataCollectorsMapOverviewPage}
                roles={accessMap.dataCollectors.list}
              />
              <AuthRoute
                exact
                path="/projects/:projectId/datacollectors/list"
                component={DataCollectorsListPage}
                roles={accessMap.dataCollectors.list}
              />
              <AuthRoute
                exact
                path="/projects/:projectId/datacollectors/performance"
                component={DataCollectorsPerformancePage}
                roles={accessMap.dataCollectors.performanceList}
              />
              <AuthRoute
                exact
                path="/projects/:projectId/datacollectors/add"
                component={DataCollectorsCreatePage}
                roles={accessMap.dataCollectors.add}
              />
              <AuthRoute
                exact
                path="/projects/:projectId/datacollectors/:dataCollectorId/edit"
                component={DataCollectorsEditPage}
                roles={accessMap.dataCollectors.edit}
              />

              <AuthRoute
                exact
                path="/projects/:projectId/organizations"
                component={ProjectOrganizationsListPage}
                roles={accessMap.projectOrganizations.list}
              />
              <AuthRoute
                exact
                path="/projects/:projectId/organizations/add"
                component={ProjectOrganizationsCreatePage}
                roles={accessMap.projectOrganizations.add}
              />

              <AuthRoute
                exact
                path="/projects/:projectId/unhandledAlertNotifications"
                component={ProjectAlertNotHandledRecipientsPage}
                roles={accessMap.projectAlertNotifications.list}
              />
              <AuthRoute
                exact
                path="/projects/:projectId/escalatedAlertNotifications/addRecipient"
                component={ProjectAlertRecipientsCreatePage}
                roles={accessMap.projectAlertNotifications.addRecipient}
              />
              <AuthRoute
                exact
                path="/projects/:projectId/escalatedAlertNotifications/:alertRecipientId/editRecipient"
                component={ProjectAlertRecipientsEditPage}
                roles={accessMap.projectAlertNotifications.editRecipient}
              />
              <AuthRoute
                exact
                path="/projects/:projectId/escalatedAlertNotifications"
                component={ProjectAlertRecipientsListPage}
                roles={accessMap.projectAlertNotifications.list}
              />

              <AuthRoute
                exact
                path="/projects/:projectId/errorMessages"
                component={ProjectErrorMessagesPage}
                roles={accessMap.projectErrorMessages.list}
              />

              <Redirect
                exact
                from="/projects/:projectId/reports"
                to="/projects/:projectId/reports/correct"
              />
              <AuthRoute
                exact
                path="/projects/:projectId/reports/correct"
                component={CorrectReportsListPage}
                roles={accessMap.reports.list}
              />
              <AuthRoute
                exact
                path="/projects/:projectId/reports/incorrect"
                component={IncorrectReportsListPage}
                roles={accessMap.reports.list}
              />
              <AuthRoute
                exact
                path="/projects/:projectId/reports/:reportId/edit"
                component={ReportsEditPage}
                roles={accessMap.reports.edit}
              />

              <AuthRoute
                exact
                path="/projects/:projectId/alerts"
                component={AlertsListPage}
                roles={accessMap.alerts.list}
              />
              <AuthRoute
                exact
                path="/projects/:projectId/alerts/:alertId/assess"
                component={AlertsAssessmentPage}
                roles={accessMap.alerts.assess}
              />
              <AuthRoute
                exact
                path="/projects/:projectId/alerts/:alertId/eventLog"
                component={AlertEventsLogPage}
                roles={accessMap.alertEvents.list}
              />

              <AuthRoute
                exact
                path="/translations"
                component={TranslationsListPage}
                roles={accessMap.translations.list}
              />
              <AuthRoute
                exact
                path="/emailTranslations"
                component={EmailTranslationsListPage}
                roles={accessMap.translations.list}
              />
              <AuthRoute
                exact
                path="/smsTranslations"
                component={SmsTranslationsListPage}
                roles={accessMap.translations.list}
              />

              <Route component={NotFoundPage} />
            </Switch>
          </ConnectedRouter>
        </MuiPickersUtilsProvider>
      </ThemeProvider>
    </ApplicationInsightsProvider>
  );
};

export default App;
