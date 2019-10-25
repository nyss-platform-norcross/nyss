export const initialState = {
  appData: {
    appReady: false,
    user: null,
    contentLanguages: [],
    siteMap: {
      path: null,
      parameters: {}
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
    list: {
      isFetching: false,
      data: []
    }
  }
};
