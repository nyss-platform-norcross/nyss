import { ReportListType as ProjectReportListType } from './components/reports/logic/reportsConstants'
import { ReportListType as NationalSocietyReportListType } from './components/nationalSocietyReports/logic/nationalSocietyReportsConstants'

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
    mobile: {
      sideMenuOpen: false
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
    loginResponse: null,
    isFetching: false
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
    zones: [],
    expandedItems: []
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
    overviewData: null,
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
    isGeneratingPdf: false,
    filtersData: {
      healthRisks: []
    },
    filters: null,
    reportsGroupedByLocationDetails: null,
    reportsGroupedByLocationDetailsFetching: false,
    dataCollectionPointsReportData: false
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
    settingTrainingState: {},
    performanceListData: [],
    performanceListFetching: false
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
    markingAsError: false,
    filtersData: {
      healthRisks: []
    },
    filters: null,
    sorting: null
  },
  nationalSocietyReports: {
    listFetching: false,
    listRemoving: {},
    listStale: true,
    listNationalSocietyId: null,
    paginatedListData: null,
    filtersData: {
      healthRisks: []
    },
    filters: null,
    sorting: null
  },
  alerts: {
    listFetching: false,
    listRemoving: {},
    listProjectId: null,
    listData: null
  }
};
