export const initialState = {
  appData: {
    appReady: false,
    user: null,
    contentLanguages: [],
    siteMap: {
      path: null,
      parameters: {},
      breadcrumb: [],
      topMenu: [],
      sideMenu: []
    }
  },
  requests: {
    isFetching: false,
    pending: {}
  },
  auth: {
    loginResponse: null
  },
  nationalSocieties: {
    listFetching: false,
    listRemoving: {},
    listData: [],
    formFetching: false,
    formSaving: false,
    formData: null,
    dashboard: {
      name: null,
      isFetching: false
    }
  }
};
