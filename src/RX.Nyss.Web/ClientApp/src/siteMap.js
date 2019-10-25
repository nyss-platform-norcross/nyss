import { Administrator, GlobalCoordinator, DataManager, DataConsumer } from "./authentication/roles";
import { getAccessTokenData } from "./authentication/auth";
import { accessMap } from "./authentication/accessMap";

export const placeholders = {
  topMenu: "topMenu",
  leftMenu: "leftMenu"
};

const siteMap = [
  {
    parentPath: "/",
    path: "/nationalsocieties",
    title: "National societies",
    placeholder: placeholders.topMenu,
    access: accessMap.nationalSocieties.list
  },
  {
    parentPath: "/nationalsocieties",
    path: "/nationalsocieties/add",
    title: "Add National Society",
    access: accessMap.nationalSocieties.add
  },
  {
    parentPath: "/nationalsocieties",
    path: "/nationalsocieties/:nationalSocietyId",
    title: "{nationalSocietyName} ({nationalSocietyCountry})",
    access: [Administrator, GlobalCoordinator, DataConsumer]
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/projects/:projectId",
    title: "{projectName}",
    access: [Administrator, GlobalCoordinator, DataManager, DataConsumer]
  },
  {
    parentPath: "/",
    path: "/healthrisks",
    title: "Health risks",
    placeholder: placeholders.topMenu,
    access: accessMap.healthRisks.list
  },
];

export const getMenu = (path, siteMapParameters, placeholder) => {
  if (!path) {
    return [];
  }

  return siteMap
    .filter(item => item.parentPath === path && item.placeholder && item.placeholder === placeholder)
    .map(item => ({
      title: item.title,
      url: item.path
    }))
};

export const getBreadcrumb = (path, siteMapParameters) => {
  const authUser = getAccessTokenData();

  if (!authUser) {
    return [];
  }

  const role = authUser.role;
  const mapItem = findSiteMapItem(path);

  let currentItem = mapItem;
  let hierarchy = [];

  while (true) {
    if (!currentItem.access || !currentItem.access.length || (currentItem.access.some(item => item === role))) {
      hierarchy.splice(0, 0, {
        title: currentItem.title,
        url: currentItem.path
      });
    }

    if (currentItem.parentPath === "/") {
      break;
    }

    currentItem = findSiteMapItem(currentItem.parentPath);
  }

  return hierarchy;
}

const findSiteMapItem = (path) => {
  const item = siteMap.find(item => item.path === path);
  if (!item) {
    throw new Error(`SiteMap configuration is inconsistent. Cannot find item with path: ${path}`)
  }
  return item;
}