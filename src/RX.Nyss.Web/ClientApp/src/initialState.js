export const initialState = {
  appData: {
    appReady: false,
    user: null
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
