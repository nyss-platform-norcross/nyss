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
      filters: null,
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
    listNationalSocietyId: null,
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
    },
  },
  projectDashboard: {
    name: null,
    projectSummary: null,
    isFetching: true,
    filtersData: {
      healthRisks: []
    },
    filters: null,
    reportsGroupedByLocationDetails: null,
    reportsGroupedByLocationDetailsFetching: false
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
    formData: null,
    mapOverviewDataCollectorLocations: [],
    mapOverviewCenterLocation: null,
    mapOverviewFilters: null,
    mapOverviewDetails: [],
    mapOverviewDetailsFetching: false,
    settingTrainingState: {}
  },
  headManagerConsents: {
    nationalSocieties: []
  },
  reports: {
    listFetching: false,
    listRemoving: {},
    listStale: true,
    listProjectId: null,
    paginatedListData: null,
    reportListFilter: {
      reportListType: "main"
    }
  },
  alerts: {
    listFetching: false,
    listRemoving: {},
    listStale: true,
    listProjectId: null,
    listData: null
  }
};
