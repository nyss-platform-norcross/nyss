const siteMap = [
  {
    parentPath: null,
    path: "/nationalsocieties",
    title: "National societies",
    placeholder: "topMenu"
  },
  {
    parentPath: "/nationalsocieties",
    path: "/nationalsocieties/:nationalSocietyId",
    title: "{nationalSocietyName} ({nationalSocietyCountry})",
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/projects/:projectId",
    title: "{projectName}"
  },
  {
    parentPath: null,
    path: "/healthrisks",
    title: "Health risks",
    placeholder: "topMenu"
  },
];

export const getMenu = (path, siteMapParameters, placeholder) => siteMap
  .filter(item => item.parentPath === path && item.placeholder && item.placeholder === placeholder)
  .map(item => ({
    title: item.title,
    url: item.path
  }));

export const getBreadcrumb = (path, siteMapParameters) => {
  const mapItem = findSiteMapItem(path);

  let currentItem = mapItem;
  let hierarchy = [];

  while (true) {
    hierarchy.splice(0, 0, {
      title: currentItem.title,
      url: currentItem.path
    });

    if (!currentItem.parentPath) {
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