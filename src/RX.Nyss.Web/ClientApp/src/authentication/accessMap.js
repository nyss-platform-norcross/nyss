import { Administrator, GlobalCoordinator } from "./roles";

export const accessMap = {
    nationalSocieties: {
        list: [Administrator, GlobalCoordinator],
        create: [Administrator, GlobalCoordinator],
        edit: [Administrator, GlobalCoordinator],
        delete: [Administrator, GlobalCoordinator]
    },
    healthRisks: {
        list: [Administrator, GlobalCoordinator],
        create: [Administrator, GlobalCoordinator],
        edit: [Administrator, GlobalCoordinator],
        delete: [Administrator, GlobalCoordinator]
    }
};
