import { Administrator, GlobalCoordinator, Manager, TechnicalAdvisor, DataConsumer, Supervisor, Coordinator } from "./roles";

export const accessMap = {
  nationalSocieties: {
    showDashboard: [Administrator, GlobalCoordinator, Manager, Coordinator, TechnicalAdvisor, DataConsumer],
    list: [Administrator, GlobalCoordinator, TechnicalAdvisor, DataConsumer],
    add: [Administrator, GlobalCoordinator],
    edit: [Administrator, GlobalCoordinator, Manager, TechnicalAdvisor, Coordinator],
    delete: [Administrator, GlobalCoordinator],
    archive: [Administrator, GlobalCoordinator],
    get: [Administrator, GlobalCoordinator, Manager, TechnicalAdvisor, DataConsumer, Coordinator],
  },
  nationalSocietyStructure: {
    edit: [Administrator, Manager, TechnicalAdvisor, Coordinator]
  },
  smsGateways: {
    list: [Administrator, Manager, TechnicalAdvisor, Coordinator],
    add: [Administrator, Manager, TechnicalAdvisor, Coordinator],
    edit: [Administrator, Manager, TechnicalAdvisor, Coordinator],
    delete: [Administrator, Manager, TechnicalAdvisor, Coordinator]
  },
  organizations: {
    list: [Administrator, GlobalCoordinator, Coordinator],
    add: [Administrator, GlobalCoordinator, Coordinator],
    edit: [Administrator, GlobalCoordinator, Coordinator],
    delete: [Administrator, GlobalCoordinator, Coordinator]
  },
  projects: {
    get: [Administrator, Manager, TechnicalAdvisor, DataConsumer, Supervisor, Coordinator],
    showOverview: [Administrator, Manager, TechnicalAdvisor, Supervisor, Coordinator],
    showDashboard: [Administrator, Manager, TechnicalAdvisor, DataConsumer, Supervisor, Coordinator],
    list: [Administrator, Manager, TechnicalAdvisor, DataConsumer, Coordinator],
    add: [Administrator, Manager, TechnicalAdvisor, Coordinator],
    edit: [Administrator, Manager, TechnicalAdvisor, Coordinator],
    delete: [Administrator, Manager, TechnicalAdvisor, Coordinator]
  },
  nationalSocietyUsers: {
    list: [Administrator, GlobalCoordinator, Manager, TechnicalAdvisor, Coordinator],
    add: [Administrator, GlobalCoordinator, Manager, TechnicalAdvisor, Coordinator],
    edit: [Administrator, GlobalCoordinator, Manager, TechnicalAdvisor, Coordinator],
    delete: [Administrator, GlobalCoordinator, Manager, TechnicalAdvisor, Coordinator],
    headManagerAccess: [GlobalCoordinator, Administrator, Coordinator]
  },
  globalCoordinators: {
    list: [Administrator, GlobalCoordinator],
    add: [Administrator],
    edit: [Administrator],
    delete: [Administrator]
  },
  healthRisks: {
    list: [Administrator, GlobalCoordinator],
    add: [Administrator, GlobalCoordinator],
    edit: [Administrator, GlobalCoordinator],
    delete: [Administrator, GlobalCoordinator]
  },
  dataCollectors: {
    list: [Administrator, Manager, TechnicalAdvisor, Supervisor],
    performanceList: [Administrator, Manager, TechnicalAdvisor, Supervisor],
    add: [Administrator, Manager, TechnicalAdvisor, Supervisor],
    edit: [Administrator, Manager, TechnicalAdvisor, Supervisor],
    delete: [Administrator, Manager, TechnicalAdvisor, Supervisor],
    export: [Administrator, Manager, TechnicalAdvisor]
  },
  reports: {
    list: [Administrator, Manager, TechnicalAdvisor, Supervisor],
    edit: [Administrator, Manager, TechnicalAdvisor],
    markAsError: [Administrator, Manager, TechnicalAdvisor, Supervisor],
    sendReport: [Administrator, Manager, TechnicalAdvisor, Supervisor]
  },
  nationalSocietyReports: {
    list: [Administrator, Manager, TechnicalAdvisor],
  },
  alerts: {
    list: [Administrator, Manager, TechnicalAdvisor, Supervisor, Coordinator],
    assess: [Administrator, Manager, TechnicalAdvisor, Supervisor],
  },
  translations: {
    list: [Administrator, GlobalCoordinator]
  }
};
