import { Administrator, GlobalCoordinator, DataManager, TechnicalAdvisor } from "./roles";

export const accessMap = {
  nationalSocieties: {
    list: [Administrator, GlobalCoordinator],
    add: [Administrator, GlobalCoordinator],
    edit: [Administrator, GlobalCoordinator],
    delete: [Administrator, GlobalCoordinator]
  },
  smsGateways: {
    list: [Administrator, GlobalCoordinator, DataManager, TechnicalAdvisor],
    add: [Administrator, GlobalCoordinator, DataManager, TechnicalAdvisor],
    edit: [Administrator, GlobalCoordinator, DataManager, TechnicalAdvisor],
    delete: [Administrator, GlobalCoordinator, DataManager, TechnicalAdvisor]
  },
  nationalSocietyUsers: {
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
