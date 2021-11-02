import { generateEpiWeekFilters } from "./utils/date";

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
    listArchiving: {},
    listReopening: {},
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
  nationalSocietyDashboard: {
    name: null,
    summary: null,
    isFetching: true,
    isGeneratingPdf: false,
    filtersData: {
      healthRisks: [],
      organizations: []
    },
    filters: null,
    reportsGroupedByLocationDetails: null,
    reportsGroupedByLocationDetailsFetching: false
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
    formData: null,
    pinging: {},
    availableIoTDevices: []
  },
  organizations: {
    listNationalSocietyId: null,
    listFetching: false,
    listRemoving: {},
    listStale: true,
    listData: [],
    formFetching: false,
    formSaving: false,
    formData: null
  },
  projectOrganizations: {
    listProjectId: null,
    listFetching: false,
    listRemoving: {},
    listStale: true,
    listData: [],
    formFetching: false,
    formSaving: false,
    formData: null
  },
  projectAlertRecipients: {
    listProjectId: null,
    listFetching: false,
    listRemoving: {},
    listStale: true,
    listData: [],
    formFetching: false,
    formSaving: false,
    formData: null,
    recipient: null
  },
  projectAlertNotHandledRecipients: {
    projectId: null,
    listFetching: false,
    listStale: true,
    listData: [],
    users: [],
    formDataFetching: false,
    saving: false
  },
  projects: {
    listFetching: false,
    isClosing: {},
    isClosed: {},
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
      organizations: [],
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
    formError: null,
    addExistingFormData: null,
  },
  dataCollectors: {
    listFetching: false,
    listRemoving: {},
    listStale: true,
    filtersStale: true,
    listData: {
      data: [],
      page: null,
      rowsPerPage: null,
      totalRows: null
    },
    listSelectedAll: false,
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
    updatingDataCollector: {},
    replacingSupervisor: false,
    performanceListData: {
      data: [],
      page: null,
      rowsPerPage: null,
      totalRows: null
    },
    completeness: null,
    performanceListFetching: false,
    performanceListFilters: {
      area: null,
      name: '',
      supervisorId: null,
      trainingStatus: 'Trained',
      pageNumber: 1,
      epiWeekFilters: generateEpiWeekFilters()
    },
    filtersData: {
      supervisors: [],
      nationalSocietyId: null
    },
    filters: {
      supervisorId: null,
      area: null,
      sex: null,
      trainingStatus: 'All',
      deployedMode: 'Deployed',
      name: null,
      pageNumber: 1
    }
  },
  agreements: {
    pendingSocieties: [],
    staleSocieties: [],
    agreementDocuments: []
  },
  reports: {
    listFetching: false,
    listRemoving: {},
    correctReportsListStale: true,
    incorrectReportsListStale: true,
    listProjectId: null,
    markingAsError: false,
    filtersData: {
      healthRisks: []
    },
    filters: null,
    sorting: null,
    editReport: {
      formHealthRisks: [],
      formDataCollectors: []
    },
    sendReport: {
      dataCollectors: []
    },
    correctReportsPaginatedListData: null,
    correctReportsFilters: null,
    correctReportsSorting: null,
    incorrectReportsPaginatedListData: null,
    incorrectReportsFilters: null,
    incorrectReportsSorting: null
  },
  nationalSocietyReports: {
    listFetching: false,
    listRemoving: {},
    correctReportsListStale: true,
    incorrectReportsListStale: true,
    listNationalSocietyId: null,
    paginatedListData: null,
    filtersData: {
      healthRisks: []
    },
    correctReportsPaginatedListData: null,
    correctReportsFilters: null,
    correctReportsSorting: null,
    incorrectReportsPaginatedListData: null,
    incorrectReportsFilters: null,
    incorrectReportsSorting: null
  },
  alerts: {
    listFetching: false,
    listRemoving: {},
    listProjectId: null,
    listData: null,
    formData: null,
    isFetchingRecipients: false,
    isPendingAlertState: false,
    notificationPhoneNumbers: [],
    notificationEmails: [],
    filters: null,
    filtersData: null
  },
  alertEvents: {
    eventTypes: [],
    eventSubtypes: [],
    logFetching: false,
    logRemoving: {},
    listStale: true,
    logItems: null,
    formFetching: false,
    formSaving: false,
    formError: null
  },
  translations: {
    listFetching: false,
    listLanguages: [],
    listTranslations: [],
    emailLanguages: [],
    emailTranslations: [],
    smsLanguages: [],
    smsTranslations: []
  },
  tracking: {
    appInsights: null,
  }
};
