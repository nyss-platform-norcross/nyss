import { placeholders } from "../../siteMapPlaceholders";
import { accessMap } from "../../authentication/accessMap";
import { Administrator, GlobalCoordinator } from "../../authentication/roles";

export const globalCoordinatorsSiteMap = [
  {
    parentPath: "/",
    path: "/globalcoordinators",
    title: "Global Coordinators",
    placeholder: placeholders.topMenu,
    access: accessMap.globalCoordinators.list
  },
  {
    parentPath: "/globalcoordinators",
    path: "/globalcoordinators/add",
    title: "Add Global Coordinator",
    access: accessMap.globalCoordinators.add
  },
  {
    parentPath: "/globalcoordinators",
    path: "/globalcoordinators/:globalCoordinatorId",
    title: "{globalCoordinatorName}",
    access: [Administrator, GlobalCoordinator]
  },
  {
    parentPath: "/globalcoordinators",
    path: "/globalcoordinators/:globalCoordinatorId/edit",
    title: "Edit Global Coordinator",
    access: [Administrator, GlobalCoordinator]
  }
];
