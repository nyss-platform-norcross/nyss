import { siteMap } from "../siteMap";

const findClosestMenu = (hierarchy, placeholder) => {
  for (let i = hierarchy.length - 1; i >= 0; i--) {
    if (hierarchy[i].siteMapData.placeholder === placeholder) {
      return hierarchy[i].siteMapData.parentPath;
    }
  }
};

export const getMenu = (
  parameters,
  placeholder,
  currentPath,
  authUser,
  parentPath,
) => {
  const pathHierarchy = getHierarchy(currentPath, parameters, authUser);
  const closestMenuPath = findClosestMenu(pathHierarchy, placeholder);

  const filteredSiteMap = siteMap.filter(
    (item) =>
      item.parentPath === (parentPath ? parentPath : closestMenuPath) &&
      item.placeholder &&
      item.placeholder === placeholder &&
      item.access.some((role) => authUser.roles.some((r) => r === role)) &&
      !(!!item.hideWhen && item.hideWhen(parameters)),
  );

  filteredSiteMap.sort((a, b) => a.placeholderIndex - b.placeholderIndex);

  return filteredSiteMap.map((item) => ({
    title: item.title(),
    url: getUrl(item.path, parameters),
    isActive: pathHierarchy.some((b) => b.siteMapData.path === item.path),
    icon: item.icon,
    path: item.path,
  }));
};

export const getHierarchy = (path, siteMapParameters, authUser) => {
  if (!authUser || !path) {
    return [];
  }

  let currentItem = findSiteMapItem(path);
  let hierarchy = [];

  while (true) {
    const hasAccess =
      !currentItem.access ||
      !currentItem.access.length ||
      currentItem.access.some((role) => authUser.roles.some((r) => r === role));

    if (hasAccess) {
      hierarchy.splice(0, 0, {
        title: getTitle(currentItem.title(), siteMapParameters),
        url: getUrl(currentItem.path, siteMapParameters),
        isActive: currentItem.path === path,
        siteMapData: { ...currentItem },
        hidden: hierarchy.length === 0 && currentItem.middleStepOnly,
      });
    }

    if (!currentItem.parentPath) {
      break;
    }

    currentItem = findSiteMapItem(currentItem.parentPath);
  }

  return hierarchy;
};

const getTitle = (template, params) =>
  Object.keys(params).reduce(
    (result, key) =>
      typeof result === "string"
        ? result.replace(`{${key}}`, params[key])
        : result,
    template,
  );

const getUrl = (template, params) =>
  Object.keys(params).reduce(
    (result, key) =>
      typeof result === "string"
        ? result.replace(`:${key}`, params[key])
        : result,
    template,
  );

const findSiteMapItem = (path) => {
  const item = siteMap.find((item) => item.path === path);
  if (!item) {
    throw new Error(
      `SiteMap configuration is inconsistent. Cannot find item with path: ${path}`,
    );
  }
  return item;
};
