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
    showStringsKeys: false
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
    formError: null
  },
  dataCollectors: {
    listFetching: false,
    listRemoving: {},
    listStale: true,
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
    performanceListFetching: false,
    performanceListFilters: {
      area: null,
      name: '',
      supervisorId: null,
      pageNumber: 1,
      lastWeek: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      },
      twoWeeksAgo: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      },
      threeWeeksAgo: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      },
      fourWeeksAgo: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      },
      fiveWeeksAgo: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      },
      sixWeeksAgo: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      },
      sevenWeeksAgo: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      },
      eightWeeksAgo: {
        reportingCorrectly: true,
        reportingWithErrors: true,
        notReporting: true
      }
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
    listStale: true,
    listProjectId: null,
    paginatedListData: null,
    markingAsError: false,
    filtersData: {
      healthRisks: []
    },
    filters: null,
    sorting: null,
    formHealthRisks: [],
    sendReport: {
      dataCollectors: []
    }
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
    listData: null,
    formData: null,
    isFetchingRecipients: false,
    isPendingAlertState: false,
    notificationPhoneNumbers: [],
    notificationEmails: []
  },
  translations: {
    listFetching: false,
    listLanguages: [],
    listTranslations: [],
    emailLanguages: [],
    emailTranslations: [],
    smsLanguages: [],
    smsTranslations: []
  }
};
