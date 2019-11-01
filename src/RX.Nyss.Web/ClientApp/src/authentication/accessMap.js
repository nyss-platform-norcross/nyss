import { Administrator, GlobalCoordinator, DataManager, TechnicalAdvisor } from "./roles";

export const accessMap = {
  nationalSocieties: {
    list: [Administrator, GlobalCoordinator],
    add: [Administrator, GlobalCoordinator],
    edit: [Administrator, GlobalCoordinator],
    delete: [Administrator, GlobalCoordinator]
  },
  smsGateways: {
    list: [Administrator, DataManager, TechnicalAdvisor],
    add: [Administrator, DataManager, TechnicalAdvisor],
    edit: [Administrator, DataManager, TechnicalAdvisor],
    delete: [Administrator, DataManager, TechnicalAdvisor]
  },
  globalCoordinators: {
    list: [Administrator, GlobalCoordinator],
    add: [Administrator, GlobalCoordinator],
    edit: [Administrator, GlobalCoordinator],
    delete: [Administrator, GlobalCoordinator]
  },
  healthRisks: {
    list: [Administrator, GlobalCoordinator],
    add: [Administrator, GlobalCoordinator],
    edit: [Administrator, GlobalCoordinator],
    delete: [Administrator, GlobalCoordinator]
  }
};
