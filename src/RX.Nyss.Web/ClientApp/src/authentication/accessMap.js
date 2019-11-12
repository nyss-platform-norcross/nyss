import { Administrator, GlobalCoordinator, Manager, TechnicalAdvisor } from "./roles";

export const accessMap = {
  nationalSocieties: {
    list: [Administrator, GlobalCoordinator],
    add: [Administrator, GlobalCoordinator],
    edit: [Administrator, GlobalCoordinator],
    delete: [Administrator, GlobalCoordinator]
  },
  smsGateways: {
    list: [Administrator, GlobalCoordinator, Manager, TechnicalAdvisor],
    add: [Administrator, GlobalCoordinator, Manager, TechnicalAdvisor],
    edit: [Administrator, GlobalCoordinator, Manager, TechnicalAdvisor],
    delete: [Administrator, GlobalCoordinator, Manager, TechnicalAdvisor]
  },
  nationalSocietyUsers: {
    list: [Administrator, Manager, TechnicalAdvisor],
    add: [Administrator, Manager, TechnicalAdvisor],
    edit: [Administrator, Manager, TechnicalAdvisor],
    delete: [Administrator, Manager, TechnicalAdvisor]
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
