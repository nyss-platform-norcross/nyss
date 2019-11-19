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
      tabMenu: [],
      sideMenu: []
    },
    message: null,
    moduleError: null,
    showStringsKeys: false,
  },
  requests: {
    isFetching: false,
    errorMessage: null,
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
    overviewData: null,
    overviewFetching: false,
    structureData: null,
    structureFetching: false,
    dashboard: {
      name: null,
      isFetching: false
    }
  },
  nationalSocietyStructure: {
    regions: [],
    isFetching: false,
    districts: [],
    villages: [],
    zones: []
  },
  smsGateways: {
    listFetching: false,
    listRemoving: {},
    listStale: true,
    listData: [],
    formFetching: false,
    formSaving: false,
    formData: null
  },
  projects: {
    listFetching: false,
    listRemoving: {},
    listStale: true,
    listData: [],
    formFetching: false,
    formHealthRisks: [],
    formTimeZones: [],
    formSaving: false,
    formData: null,
    dashboard: {
      name: null,
      projectSummary: null,
      isFetching: false
    }
  },
  globalCoordinators: {
    listFetching: false,
    listRemoving: {},
    listData: [],
    formFetching: false,
    formSaving: false,
    formData: null
  },
  healthRisks: {
    listFetching: false,
    listRemoving: {},
    listData: [],
    formFetching: false,
    formSaving: false,
    formError: null,
    formData: null
  },
  nationalSocietyUsers: {
    listFetching: false,
    listRemoving: {},
    listStale: true,
    listData: [],
    listNationalSocietyId: null,
    settingAsHead: {},
    formFetching: false,
    formSaving: false,
    formData: null,
    formProjects: [],
    formError: null
  },
  dataCollectors: {
    listFetching: false,
    listRemoving: {},
    listStale: true,
    listData: [],
    formRegions: [],
    formSupervisors: [],
    formFetching: false,
    formSaving: false,
    formData: null
  },
  headManagerConsents: {
    nationalSocieties: []
  },
  reports: {
    listFetching: false,
    listRemoving: {},
    listStale: true, 
    paginatedListData: null
  }
};
