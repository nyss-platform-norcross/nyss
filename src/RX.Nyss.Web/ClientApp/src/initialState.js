export const initialState = {
  appData: {
    appReady: false,
    user: null,
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
