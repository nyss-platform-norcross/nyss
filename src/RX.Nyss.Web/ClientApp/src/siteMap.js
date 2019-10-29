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
    path: "/nationalsocieties/:nationalSocietyId/edit",
    title: "Edit",
    access: [Administrator, GlobalCoordinator, DataConsumer]
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/projects",
    title: "Projects",
    placeholder: placeholders.leftMenu,
    access: [Administrator, GlobalCoordinator, DataManager, DataConsumer]
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/overview",
    title: "Settings",
    placeholder: placeholders.leftMenu,
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

export const getMenu = (path, siteMapParameters, placeholder, currentPath) => {
  if (!path) {
    return [];
  }

  const breadcrumb = currentPath ? getBreadcrumb(currentPath, siteMapParameters) : [];

  return siteMap
    .filter(item => item.parentPath === path && item.placeholder && item.placeholder === placeholder)
    .map(item => ({
      title: item.title,
      url: getUrl(item.path, siteMapParameters),
      isActive: breadcrumb.some(b => b.path === item.path)
    }))
};

export const getBreadcrumb = (path, siteMapParameters) => {
  const authUser = getAccessTokenData();

  if (!authUser) {
    return [];
  }

  if (path === "/") {
    return [];
  }

  const role = authUser.role;
  const mapItem = findSiteMapItem(path);

  let currentItem = mapItem;
  let hierarchy = [];

  while (true) {
    if (!currentItem.access || !currentItem.access.length || (currentItem.access.some(item => item === role))) {
      hierarchy.splice(0, 0, {
        path: currentItem.path,
        title: getTitle(currentItem.title, siteMapParameters),
        url: getUrl(currentItem.path, siteMapParameters),
        isActive: currentItem.path === path
      });
    }

    if (currentItem.parentPath === "/") {
      break;
    }

    currentItem = findSiteMapItem(currentItem.parentPath);
  }

  return hierarchy;
}

const getTitle = (template, params) => {
  let result = template;
  for (let key in params) {
    result = result.replace(`{${key}}`, params[key])
  }
  return result;
}

const getUrl = (template, params) => {
  let result = template;
  for (let key in params) {
    result = result.replace(`:${key}`, params[key])
  }
  return result;
}

const findSiteMapItem = (path) => {
  const item = siteMap.find(item => item.path === path);
  if (!item) {
    throw new Error(`SiteMap configuration is inconsistent. Cannot find item with path: ${path}`)
  }
  return item;
}