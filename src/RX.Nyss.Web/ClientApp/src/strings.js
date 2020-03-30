import React from 'react';
import { StringsEditor } from './components/common/stringsEditor/StringsEditor';

let stringList = {};

let showKeys = false;

export const areStringKeysDisplayed = () => showKeys;

export const stringKeys = {
  error: {
    responseNotSuccessful: "error.responseNotSuccessful",
    notAuthenticated: "error.notAuthenticated",
    unauthorized: "error.unauthorized",
    redirected: "error.redirected"
  },
  roles: {
    "TechnicalAdvisor": "roles.technicalAdvisor",
    "Administrator": "roles.administrator",
    "GlobalCoordinator": "roles.globalCoordinator",
    "DataConsumer": "roles.dataConsumer",
    "Manager": "roles.manager",
    "Supervisor": "roles.supervisor",
  },
  login: {
    title: "login.title",
    welcome: "login.welcome",
    signIn: "login.signIn",
    email: "login.email",
    password: "login.password",
    forgotPassword: "login.forgotPassword",
    notSucceeded: "login.notSucceeded",
    lockedOut: "login.lockedOut"
  },
  nationalSociety: {
    title: "nationalSociety.title",
    addNew: "nationalSociety.addNew",
    edit: "nationalSociety.edit",
    archive:
    {
      title: "nationalSociety.archive.title",
      content: "nationalSociety.archive.content"
    },
    reopen:
    {
      title: "nationalSociety.reopen.title",
      content: "nationalSociety.reopen.content"
    },
    form: {
      creationTitle: "nationalSociety.form.creationTitle",
      editionTitle: "nationalSociety.form.editionTitle",
      name: "nationalSociety.form.name",
      country: "nationalSociety.form.country",
      contentLanguage: "nationalSociety.form.contentLanguage",
      create: "nationalSociety.form.create",
      update: "nationalSociety.form.update"
    },
    messages: {
      create: {
        success: "nationalSociety.create.success"
      },
      edit: {
        success: "nationalSociety.edit.success"
      },
      delete: {
        success: "nationalSociety.delete.success"
      },
      archive: {
        success: "nationalSociety.archive.success"
      },
      reopen: {
        success: "nationalSociety.reopen.success"
      }
    },
    list: {
      name: "nationalSociety.list.name",
      country: "nationalSociety.list.country",
      startDate: "nationalSociety.list.startDate",
      headManager: "nationalSociety.list.headManager",
      technicalAdvisor: "nationalSociety.list.technicalAdvisor",
      removalConfirmation: "nationalSociety.list.removalConfirmation",
      archive: "nationalSociety.list.archive",
      reopen: "nationalSociety.list.reopen",
    },
    settings: {
      title: "nationalSociety.settings.title"
    },
    dashboard: {
      title: "nationalSociety.dashboard.title",
      numbers: {
        alertsSummaryTitle: "nationalsociety.dashboard.numbers.alertsSummaryTitle",
        dismissedAlerts: "nationalsociety.dashboard.numbers.dismissedAlerts",
        escalatedAlerts: "nationalsociety.dashboard.numbers.escalatedAlerts",
        closedAlerts: "nationalsociety.dashboard.numbers.closedAlerts",
        totalReportCount: "nationalsociety.dashboard.numbers.totalReportCount",
        totalErrorReportCount: "nationalsociety.dashboard.numbers.totalErrorReportCount",
        totalReportCountTitle: "nationalsociety.dashboard.numbers.totalReportCountTitle"
      },
      filters: {
        startDate: "nationalsociety.dashboard.filters.startDate",
        endDate: "nationalsociety.dashboard.filters.endDate",
        timeGrouping: "nationalsociety.dashboard.filters.timeGrouping",
        timeGroupingDay: "nationalsociety.dashboard.filters.timeGrouping.day",
        timeGroupingWeek: "nationalsociety.dashboard.filters.timeGrouping.week",
        healthRisk: "nationalsociety.dashboard.filters.healthRisk",
        healthRiskAll: "nationalsociety.dashboard.filters.healthRiskAll",
        reportsType: "nationalsociety.dashboard.filters.reportsType",
        allReportsType: "nationalsociety.dashboard.filters.allReportsType",
        dataCollectorReportsType: "nationalsociety.dashboard.filters.dataCollectorReportsType",
        dataCollectionPointReportsType: "nationalsociety.dashboard.filters.dataCollectionPointReportsType"
      },
      activeDataCollectorCount: "nationalsociety.dashboard.activeDataCollectorCount",
      referredToHospitalCount: "nationalsociety.dashboard.referredToHospitalCount",
      fromOtherVillagesCount: "nationalsociety.dashboard.fromOtherVillagesCount",
      deathCount: "nationalsociety.dashboard.deathCount",
      startDate: "nationalsociety.dashboard.startDate",
      dataCollectors: "nationalsociety.dashboard.dataCollectors",
      dataCollectionPoints: "nationalsociety.dashboard.dataCollectionPoints",
      healthRisks: "nationalsociety.dashboard.healthRisks",
      map: {
        reportCount: "nationalsociety.dashboard.map.reportCount",
        title: "nationalsociety.dashboard.map.title",
        reports: "nationalsociety.dashboard.map.reports",
        report: "nationalsociety.dashboard.map.report",
      },
      reportsPerVillageAndDate: {
        title: "nationalSociety.dashboard.reportsPerVillageAndDate.title",
        rest: "nationalSociety.dashboard.reportsPerVillageAndDate.rest",
        numberOfReports: "nationalSociety.dashboard.reportsPerVillageAndDate.numberOfReports"
      },
    },
    overview: {
      title: "nationalSociety.dashboard.overview"
    },
    structure: {
      title: "nationalSociety.structure.title",
      introduction: "nationalSociety.structure.introduction",
      removalConfirmation: "nationalSociety.structure.removalConfirmation",
      cancelEdition: "nationalSociety.structure.cancelEdition",
      saveEdition: "nationalSociety.structure.saveEdition",
      saveNew: "nationalSociety.structure.saveNew",
      addRegion: "nationalSociety.structure.addRegion",
      addDistrict: "nationalSociety.structure.addDistrict",
      addVillage: "nationalSociety.structure.addVillage",
      addZone: "nationalSociety.structure.addZone"
    },
    setHead: {
      notAMemberOfSociety: "nationalSociety.setHead.notAMemberOfSociety"
    }
  },
  reportsMap: {
    reports: "reportsMap.reports",
    report: "reportsMap.report",
  },
  healthRisk: {
    title: "healthRisk.title",
    addNew: "healthRisk.addNew",
    form: {
      creationTitle: "healthRisk.form.creationTitle",
      editionTitle: "healthRisk.form.editionTitle",
      healthRiskCode: "healthRisk.form.healthRiskCode",
      healthRiskType: "healthRisk.form.healthRiskType",
      translationsSetion: "healthRisk.form.translationsSetion",
      alertsSetion: "healthRisk.form.alertsSetion",
      alertRuleDescription: "healthRisk.form.alertRuleDescription",
      alertRuleCountThreshold: "healthRisk.form.alertRuleCountThreshold",
      alertRuleDaysThreshold: "healthRisk.form.alertRuleDaysThreshold",
      alertRuleKilometersThreshold: "healthRisk.form.alertRuleKilometersThreshold",
      contentLanguageName: "healthRisk.form.contentLanguageName",
      contentLanguageCaseDefinition: "healthRisk.form.contentLanguageCaseDefinition",
      contentLanguageFeedbackMessage: "healthRisk.form.contentLanguageFeedbackMessage",
      create: "healthRisk.form.create",
      update: "healthRisk.form.update"
    },
    list: {
      healthRiskCode: "healthRisk.list.healthRiskCode",
      name: "healthRisk.list.name",
      healthRiskType: "healthRisk.list.healthRiskType",
      removalConfirmation: "healthRisk.list.removalConfirmation"
    },
    create: {
      success: "healthRisk.create.success"
    },
    edit: {
      success: "healthRisk.edit.success"
    },
    delete: {
      success: "healthRisk.delete.success"
    },
    constants: {
      healthRiskType: {
        human: "healthRisk.type.human",
        nonhuman: "healthRisk.type.nonhuman",
        unusualevent: "healthRisk.type.unusualevent",
        activity: "healthRisk.type.activity",
      }
    }
  },
  smsGateway: {
    title: "smsGateway.title",
    addNew: "smsGateway.addNew",
    form: {
      creationTitle: "smsGateway.form.creationTitle",
      editionTitle: "smsGateway.form.editionTitle",
      name: "smsGateway.form.name",
      apiKey: "smsGateway.form.apiKey",
      gatewayType: "smsGateway.form.gatewayType",
      emailAddress: "smsGateway.form.emailAddress",
      create: "smsGateway.form.create",
      update: "smsGateway.form.update"
    },
    create: {
      success: "smsGateway.create.success"
    },
    edit: {
      success: "smsGateway.edit.success"
    },
    delete: {
      success: "smsGateway.delete.success"
    },
    list: {
      name: "smsGateway.list.name",
      apiKey: "smsGateway.list.apiKey",
      gatewayType: "smsGateway.list.gatewayType",
      removalConfirmation: "smsGateway.list.removalConfirmation"
    },
  },
  project: {
    title: "project.title",
    addNew: "project.addNew",
    edit: "project.edit",
    form: {
      creationTitle: "project.form.creationTitle",
      editionTitle: "project.form.editionTitle",
      name: "project.form.name",
      timeZone: "project.form.timeZone",
      healthRisks: "project.form.healthRisks",
      healthRisksSetion: "project.form.healthRisksSetion",
      overviewHealthRisksSection: "project.form.overviewHealthRisksSection",
      caseDefinition: "project.form.caseDefinition",
      feedbackMessage: "project.form.feedbackMessage",
      alertsSetion: "project.form.alertsSetion",
      alertRuleCountThreshold: "project.form.alertRuleCountThreshold",
      alertRuleDaysThreshold: "project.form.alertRuleDaysThreshold",
      alertRuleKilometersThreshold: "project.form.alertRuleKilometersThreshold",
      emailNotificationsSection: "project.form.emailNotificationsSection",
      overviewEmailNotificationsSection: "project.form.overviewEmailNotificationsSection",
      emailNotificationDescription: "project.form.emailNotificationDescription",
      smsNotificationsSetion: "project.form.smsNotificationsSetion",
      overviewSmsNotificationsSection: "project.form.overviewSmsNotificationsSection",
      smsNotificationDescription: "project.form.smsNotificationDescription",
      addEmail: "project.form.addEmail",
      addSms: "project.form.addSms",
      create: "project.form.create",
      update: "project.form.update"
    },
    messages: {
      create: {
        success: "project.create.success"
      },
      edit: {
        success: "project.edit.success"
      },
      close: {
        success: "project.close.success"
      },
    },
    list: {
      name: "project.list.name",
      totalReportCount: "project.list.totalReportCount",
      totalDataCollectorCount: "project.list.totalDataCollectorCount",
      startDate: "project.list.startDate",
      escalatedAlertCount: "project.list.escalatedAlertCount",
      supervisorCount: "project.list.supervisorCount",
      endDate: "project.list.endDate",
      ongoing: "project.list.ongoing",
      open: "project.list.open",
      edit: "project.list.edit",
      close: "project.list.close",
      removalConfirmation: "project.list.removalConfirmation",
      removalConfirmationText: "project.list.removalConfirmationText",
      removalConfirmationTextTwo: "project.list.removalConfirmationTextTwo"
    },
    dashboard: {
      title: "project.dashboard.title",
      printTitle: "project.dashboard.printTitle",
      numbers: {
        totalReportCount: "project.dashboard.numbers.totalReportCount",
        totalErrorReportCount: "project.dashboard.numbers.totalErrorReportCount",
        totalReportCountTitle: "project.dashboard.numbers.totalReportCountTitle",
        dismissedAlerts: "project.dashboard.numbers.dismissedAlerts",
        escalatedAlerts: "project.dashboard.numbers.escalatedAlerts",
        closedAlerts: "project.dashboard.numbers.closedAlerts"
      },
      filters: {
        startDate: "project.dashboard.filters.startDate",
        endDate: "project.dashboard.filters.endDate",
        timeGrouping: "project.dashboard.filters.timeGrouping",
        timeGroupingDay: "project.dashboard.filters.timeGrouping.day",
        timeGroupingWeek: "project.dashboard.filters.timeGrouping.week",
        healthRisk: "project.dashboard.filters.healthRisk",
        healthRiskAll: "project.dashboard.filters.healthRiskAll",
        reportsType: "project.dashboard.filters.reportsType",
        allReportsType: "project.dashboard.filters.allReportsType",
        dataCollectorReportsType: "project.dashboard.filters.dataCollectorReportsType",
        dataCollectionPointReportsType: "project.dashboard.filters.dataCollectionPointReportsType",
        trainingReportsListType: "project.dashboard.filters.trainingReportsListType",
        notInTraining: "project.dashboard.filters.notInTraining",
        inTraining: "project.dashboard.filters.inTraining"
      },
      reportsPerHealthRisk: {
        title: "project.dashboard.reportsPerHealthRisk.title",
        numberOfReports: "project.dashboard.reportsPerHealthRisk.numberOfReports",
        periods: "project.dashboard.reportsPerHealthRisk.periods",
        rest: "project.dashboard.reportsPerHealthRisk.rest",
      },
      reportsPerFeature: {
        title: "project.dashboard.reportsPerFeature.title",
        female: "project.dashboard.reportsPerFeature.female",
        male: "project.dashboard.reportsPerFeature.male",
        total: "project.dashboard.reportsPerFeature.total",
        below5: "project.dashboard.reportsPerFeature.below5",
        above5: "project.dashboard.reportsPerFeature.above5",
      },
      reportsPerFeatureAndDate: {
        femalesBelow5: "project.dashboard.reportsPerFeatureAndDate.femalesBelow5",
        femalesAbove5: "project.dashboard.reportsPerFeatureAndDate.femalesAbove5",
        malesBelow5: "project.dashboard.reportsPerFeatureAndDate.malesBelow5",
        malesAbove5: "project.dashboard.reportsPerFeatureAndDate.malesAbove5",
        numberOfReports: "project.dashboard.reportsPerFeatureAndDate.numberOfReports",
        title: "project.dashboard.reportsPerFeatureAndDate.title"
      },
      reportsPerVillageAndDate: {
        title: "project.dashboard.reportsPerVillageAndDate.title",
        rest: "project.dashboard.reportsPerVillageAndDate.rest",
        numberOfReports: "project.dashboard.reportsPerVillageAndDate.numberOfReports"
      },
      dataCollectionPointReportsByDate: {
        referredToHospitalCount: "project.dashboard.dataCollectionPointReportsByDate.referredToHospitalCount",
        deathCount: "project.dashboard.dataCollectionPointReportsByDate.deathCount",
        fromOtherVillagesCount: "project.dashboard.dataCollectionPointReportsByDate.fromOtherVillagesCount",
        numberOfReports: "project.dashboard.dataCollectionPointReportsByDate.numberOfReports",
        title: "project.dashboard.dataCollectionPointReportsByDate.title"
      },
      activeDataCollectorCount: "project.dashboard.activeDataCollectorCount",
      referredToHospitalCount: "project.dashboard.referredToHospitalCount",
      fromOtherVillagesCount: "project.dashboard.fromOtherVillagesCount",
      deathCount: "project.dashboard.deathCount",
      startDate: "project.dashboard.startDate",
      dataCollectors: "project.dashboard.dataCollectors",
      dataCollectionPoints: "project.dashboard.dataCollectionPoints",
      healthRisks: "project.dashboard.healthRisks",
      supervisors: "project.dashboard.supervisors",
      supervisorEmailAddress: "project.dashboard.supervisorEmailAddress",
      supervisorPhoneNumber: "project.dashboard.supervisorPhoneNumber",
      supervisorAdditionalPhoneNumber: "project.dashboard.supervisorAdditionalPhoneNumber",
      alertsSummary: "project.dashboard.alertsSummary",
      map: {
        reportCount: "project.dashboard.map.reportCount",
        title: "project.dashboard.map.title",
        reports: "project.dashboard.map.reports",
        report: "project.dashboard.map.report",
      },
      generatePdf: "project.dashboard.generatePdf"
    },
    settings: "project.settings.title"
  },
  globalCoordinator: {
    title: "globalCoordinator.title",
    addNew: "globalCoordinator.addNew",
    form: {
      creationTitle: "globalCoordinator.form.creationTitle",
      editionTitle: "globalCoordinator.form.editionTitle",
      name: "globalCoordinator.form.name",
      email: "globalCoordinator.form.email",
      phoneNumber: "globalCoordinator.form.phoneNumber",
      additionalPhoneNumber: "globalCoordinator.form.additionalPhoneNumber",
      organization: "globalCoordinator.form.organization",
      create: "globalCoordinator.form.create",
      update: "globalCoordinator.form.update"
    },
    list: {
      name: "globalCoordinator.list.name",
      email: "globalCoordinator.list.email",
      phoneNumber: "globalCoordinator.list.phoneNumber",
      organization: "globalCoordinator.list.organization",
      removalConfirmation: "globalCoordinator.list.removalConfirmation"
    },
    create: {
      success: "globalCoordinator.create.success"
    },
    edit: {
      success: "globalCoordinator.edit.success"
    },
    delete: {
      success: "globalCoordinator.delete.success"
    }
  },
  nationalSocietyUser: {
    title: "nationalSocietyUser.title",
    addNew: "nationalSocietyUser.addNew",
    addSuccess: "nationalSocietyUser.addSuccess",
    addExisting: "nationalSocietyUser.addExisting",
    form: {
      creationTitle: "nationalSocietyUser.form.creationTitle",
      addExistingTitle: "nationalSocietyUser.form.addExistingTitle",
      addExistingDescription: "nationalSocietyUser.form.addExistingDescription",
      editionTitle: "nationalSocietyUser.form.editionTitle",
      name: "nationalSocietyUser.form.name",
      role: "nationalSocietyUser.form.role",
      decadeOfBirth: "nationalSocietyUser.form.decadeOfBirth",
      sex: "nationalSocietyUser.form.sex",
      email: "nationalSocietyUser.form.email",
      project: "nationalSocietyUser.form.project",
      phoneNumber: "nationalSocietyUser.form.phoneNumber",
      additionalPhoneNumber: "nationalSocietyUser.form.additionalPhoneNumber",
      organization: "nationalSocietyUser.form.organization",
      create: "nationalSocietyUser.form.create",
      addExisting: "nationalSocietyUser.form.addExisting",
      projectIsClosed: "nationalSocietyUser.form.projectIsClosed",
      update: "nationalSocietyUser.form.update"
    },
    create: {
      success: "nationalSocietyUser.create.success"
    },
    edit: {
      success: "nationalSocietyUser.edit.success"
    },
    remove: {
      success: "nationalSocietyUser.remove.success"
    },
    list: {
      name: "nationalSocietyUser.list.name",
      email: "nationalSocietyUser.list.email",
      phoneNumber: "nationalSocietyUser.list.phoneNumber",
      role: "nationalSocietyUser.list.role",
      project: "nationalSocietyUser.list.project",
      removalConfirmation: "nationalSocietyUser.list.removalConfirmation",
      headManager: "nationalSocietyUser.list.headManager"
    },
    messages: {
      creationSuccessful: "nationalSocietyUser.messages.creationSuccessful",
      roleNotValid: "nationalSocietyUser.messages.roleNotValid"
    }
  },
  dataCollector: {
    title: "dataCollectors.title",
    addNew: "dataCollectors.addNew",
    exportExcel: "dataCollectors.exportExcel",
    exportCsv: "dataCollectors.exportCsv",
    form: {
      creationTitle: "dataCollectors.form.creationTitle",
      editionTitle: "dataCollectors.form.editionTitle",
      dataCollectorType: "dataCollectors.form.dataCollectorType",
      name: "dataCollectors.form.name",
      displayName: "dataCollectors.form.displayName",
      sex: "dataCollectors.form.sex",
      phoneNumber: "dataCollectors.form.phoneNumber",
      additionalPhoneNumber: "dataCollectors.form.additionalPhoneNumber",
      latitude: "dataCollectors.form.latitude",
      longitude: "dataCollectors.form.longitude",
      create: "dataCollectors.form.create",
      update: "dataCollectors.form.update",
      birthYearGroup: "dataCollectors.form.birthYearGroup",
      supervisor: "dataCollectors.form.supervisor",
      village: "dataCollectors.form.village",
      district: "dataCollectors.form.district",
      region: "dataCollectors.form.region",
      zone: "dataCollectors.form.zone",
      retrieveLocation: "dataCollectors.form.retrieveLocation"
    },
    list: {
      dataCollectorType: "dataCollectors.list.dataCollectorType",
      name: "dataCollectors.list.name",
      displayName: "dataCollectors.list.displayName",
      phoneNumber: "dataCollectors.list.phoneNumber",
      sex: "dataCollectors.list.sex",
      location: "dataCollectors.list.location",
      status: "dataCollectors.list.status",
      removalConfirmation: "dataCollectors.list.removalConfirmation",
      title: "dataCollectors.list.title",
      trainingStatus: "dataCollectors.list.trainingStatus",
      isInTrainingMode: "dataCollectors.list.isInTrainingMode",
      isNotInTrainingMode: "dataCollectors.list.isNotInTrainingMode",
      setToInTraining: "dataCollectors.list.setToInTraining",
      takeOutOfTraining: "dataCollectors.list.takeOutOfTraining",
      supervisor: "dataCollectors.list.supervisor"
    },
    performanceList: {
      title: "dataCollectors.performanceList.title",
      name: "dataCollectors.performanceList.name",
      daysSinceLastReport: "dataCollectors.performanceList.daysSinceLastReport",
      statusLastWeek: "dataCollectors.performanceList.statusLastWeek",
      statusTwoWeeksAgo: "dataCollectors.performanceList.statusTwoWeeksAgo",
      statusThreeWeeksAgo: "dataCollectors.performanceList.statusThreeWeeksAgo",
      statusFourWeeksAgo: "dataCollectors.performanceList.statusFourWeeksAgo",
      statusFiveWeeksAgo: "dataCollectors.performanceList.statusFiveWeeksAgo",
      statusSixWeeksAgo: "dataCollectors.performanceList.statusSixWeeksAgo",
      statusSevenWeeksAgo: "dataCollectors.performanceList.statusSevenWeeksAgo",
      statusEightWeeksAgo: "dataCollectors.performanceList.statusEightWeeksAgo",
      legend: {
        "ReportingCorrectly": "dataCollectors.performanceList.legend.reportingCorrectly",
        "NotReporting": "dataCollectors.performanceList.legend.notReporting",
        "ReportingWithErrors": "dataCollectors.performanceList.legend.reportingWithErrors"
      }
    },
    filters: {
      supervisors: "dataCollectors.filters.supervisors",
      sex: "dataCollectors.filters.sex",
      supervisorsAll: "dataCollectors.filters.supervisorsAll",
      sexAll: "dataCollectors.filters.sexAll",
      trainingStatus: "dataCollectors.filters.trainingStatus"
    },
    constants: {
      sex: {
        male: "dataCollectors.sex.male",
        female: "dataCollectors.sex.female",
        other: "dataCollectors.sex.other",
      },
      dataCollectorType: {
        "Human": "dataCollectors.dataCollectorType.human",
        "CollectionPoint": "dataCollectors.dataCollectorType.collectionPoint"
      }
    },
    mapOverview: {
      title: "dataCollectors.mapOverview.title",
      legend: {
        "ReportingCorrectly": "dataCollectors.mapOverview.legend.reportingCorrectly",
        "NotReporting": "dataCollectors.mapOverview.legend.notReporting",
        "ReportingWithErrors": "dataCollectors.mapOverview.legend.reportingWithErrors"
      }
    },
    messages: {
      creationSuccessful: "dataCollectors.create.success",
      editionSuccessful: "dataCollectors.edit.success",
      deletionSuccessful: "dataCollectors.delete.success"
    }
  },
  reports: {
    list: {
      title: "reports.list.title",
      date: "reports.list.date",
      time: "reports.list.time",
      healthRisk: "reports.list.healthRisk",
      status: "reports.list.status",
      region: "reports.list.region",
      district: "reports.list.district",
      village: "reports.list.village",
      location: "reports.list.location",
      zone: "reports.list.zone",
      dataCollectorDisplayName: "reports.list.dataCollectorDisplayName",
      message: "reports.list.message",
      malesBelowFive: "reports.list.malesBelowFive",
      malesAtLeastFive: "reports.list.malesAtLeastFive",
      femalesBelowFive: "reports.list.femalesBelowFive",
      femalesAtLeastFive: "reports.list.femalesAtLeastFive",
      referredCount: "reports.list.referredCount",
      deathCount: "reports.list.deathCount",
      fromOtherVillagesCount: "reports.list.fromOtherVillagesCount",
      success: "reports.list.success",
      error: "reports.list.error",
      selectReportListType: "reports.list.selectReportListType",
      mainReportsListType: "reports.list.mainReportsListType",
      trainingReportsListType: "reports.list.trainingReportsListType",
      training: "reports.list.training",
      nonTraining: "reports.list.nonTraining",
      dcpReportListType: "reports.list.dcpReportListType",
      exportToExcel: "reports.list.exportToExcel",
      exportToCsv: "reports.list.exportToCsv",
      successfulyMarkedAsError: "reports.list.successfulyMarkedAsError",
      markAsError: "reports.list.markAsError",
      markAsErrorConfirmation: "reports.list.markAsErrorConfirmation",
      markAsErrorConfirmationText: "reports.list.markAsErrorConfirmationText",
      markedAsError: "reports.list.markedAsError",
      editReport: "reports.list.editReport",
      editedSuccesfully: "reports.list.editedSuccessfully"
    },
    form: {
      title: "reports.form.editionTitle",
      date: "reports.form.date",
      healthRisk: "reports.form.healthRisk",
      malesBelowFive: "reports.form.malesBelowFive",
      malesAtLeastFive: "reports.form.malesAtLeastFive",
      femalesBelowFive: "reports.form.femalesBelowFive",
      femalesAtLeastFive: "reports.form.femalesAtLeastFive",
      referredCount: "reports.form.referredCount",
      deathCount: "reports.form.deathCount",
      fromOtherVillagesCount: "reports.form.fromOtherVillagesCount",
      update: "reports.form.update"
    },
    title: "reports.title",
    sendReport: {
      phoneNumber: "reports.sendReport.phoneNumber",
      message: "reports.sendReport.message",
      apiKey: "reports.sendReport.apiKey",
      sendReport: "reports.sendReport.sendReport",
      goToReportList: "reports.sendReport.goToReportList",
      success: "reports.sendReport.success"
    }
  },
  nationalSocietyReports: {
    list: {
      title: "nationalSocietyReports.list.title",
      date: "nationalSocietyReports.list.date",
      healthRisk: "nationalSocietyReports.list.healthRisk",
      status: "nationalSocietyReports.list.status",
      project: "nationalSocietyReports.list.project",
      region: "nationalSocietyReports.list.region",
      district: "nationalSocietyReports.list.district",
      village: "nationalSocietyReports.list.village",
      location: "nationalSocietyReports.list.location",
      zone: "nationalSocietyReports.list.zone",
      dataCollectorDisplayName: "nationalSocietyReports.list.dataCollectorDisplayName",
      message: "nationalSocietyReports.list.message",
      malesBelowFive: "nationalSocietyReports.list.malesBelowFive",
      malesAtLeastFive: "nationalSocietyReports.list.malesAtLeastFive",
      femalesBelowFive: "nationalSocietyReports.list.femalesBelowFive",
      femalesAtLeastFive: "nationalSocietyReports.list.femalesAtLeastFive",
      referredCount: "nationalSocietyReports.list.referredCount",
      deathCount: "nationalSocietyReports.list.deathCount",
      fromOtherVillagesCount: "nationalSocietyReports.list.fromOtherVillagesCount",
      success: "nationalSocietyReports.list.success",
      error: "nationalSocietyReports.list.error",
      markedAsError: "nationalSocietyReports.list.markedAsError"
    },
    title: "nationalSocietyReports.title"
  },
  alerts: {
    list: {
      title: "alerts.list.title",
      dateTime: "alerts.list.dateTime",
      healthRisk: "alerts.list.healthRisk",
      reportCount: "alerts.list.reportCount",
      village: "alerts.list.village",
      status: "alerts.list.status",
      success: "alerts.list.success",
      error: "alerts.list.error"
    },
    details: {
      title: "alerts.details.title"
    },
    assess: {
      title: "alerts.assess.title",
      caseDefinition: "alerts.assess.caseDefinition",
      introduction: "alerts.assess.introduction",
      reports: "alerts.assess.reports",
      comments: "alerts.assess.comments",
      statusDescription: {
        closed: "alerts.assess.statusDescription.closed",
        escalated: "alerts.assess.statusDescription.escalated",
        dismissed: "alerts.assess.statusDescription.dismissed",
        rejected: "alerts.assess.statusDescription.rejected"
      },
      alert: {
        close: "alerts.assess.alert.close",
        escalate: "alerts.assess.alert.escalate",
        escalateConfirmation: "alerts.assess.alert.escalateConfirmation",
        escalateConfirmationInformDataCollectors: "alerts.assess.alert.escalateConfirmationInformDataCollectors",
        escalateNotificationEmails: "alerts.assess.alert.escalateNotificationEmails",
        escalateNotificationSmses: "alerts.assess.alert.escalateNotificationSmses",
        dismiss: "alerts.assess.alert.dismiss",
        escalationPossible: "alerts.assess.alert.escalationPossible",
        dismissalPossible: "alerts.assess.alert.dismissalPossible",
        escalatedSuccessfully: "alerts.assess.alert.escalatedSuccessfully",
        dismissedSuccessfully: "alerts.assess.alert.dismissedSuccessfully",
        closedSuccessfully: "alerts.assess.alert.closedSuccessfully",
      },
      report: {
        accept: "alerts.assess.report.accept",
        acceptedSuccessfully: "alerts.assess.report.acceptedSuccessfully",
        dismiss: "alerts.assess.report.dismiss",
        dismissedSuccessfully: "alerts.assess.report.dismissedSuccessfully",
        sender: "alerts.assess.report.sender",
        phoneNumber: "alerts.assess.report.phoneNumber",
        village: "alerts.assess.report.village",
        sex: "alerts.assess.report.sex",
        age: "alerts.assess.report.age",
        id: "alerts.assess.report.id",
      }
    },
    logs: {
      title: "alerts.logs.title",
      list: {
        date: "alerts.logs.list.date",
        logType: "alerts.logs.list.logType",
        userName: "alerts.logs.list.userName"
      }
    },
    constants: {
      alertStatus: {
        "Pending": "alerts.alertStatus.pending",
        "Rejected": "alerts.alertStatus.rejected",
        "Dismissed": "alerts.alertStatus.dismissed",
        "Escalated": "alerts.alertStatus.escalated",
        "Closed": "alerts.alertStatus.closed"
      },
      reportStatus: {
        "Rejected": "alerts.reportStatus.rejected",
        "Accepted": "alerts.reportStatus.accepted",
        "Pending": "alerts.reportStatus.pending"
      },
      sex: {
        "Female": "alerts.sex.female",
        "Male": "alerts.sex.female",
      },
      age: {
        "BelowFive": "alerts.age.belowFive",
        "AtLeastFive": "alerts.age.atLeastFive",
      },
      logType: {
        "TriggeredAlert": "alerts.logType.TriggeredAlert",
        "EscalatedAlert": "alerts.logType.EscalatedAlert",
        "DismissedAlert": "alerts.logType.DismissedAlert",
        "ClosedAlert": "alerts.logType.ClosedAlert",
        "AcceptedReport": "alerts.logType.AcceptedReport",
        "RejectedReport": "alerts.logType.RejectedReport"
      }
    },
    title: "alerts.title"
  },
  filters: {
    area: {
      title: "filters.area.title",
      all: "filters.area.all"
    },
    report: {
      healthRisk: "filters.report.healthRisk",
      healthRiskAll: "filters.report.healthRiskAll",
      selectReportListType: "filters.report.selectReportListType",
      mainReportsListType: "filters.report.mainReportsListType",
      dcpReportListType: "filters.report.dcpReportListType",
      unknownSenderReportListType: "filters.report.unknownSenderReportListType",
      status: "filters.report.status",
      success: "filters.report.success",
      error: "filters.report.error",
      training: "filters.report.training",
      trainingReports: "filters.report.trainingReports",
      nonTrainingReports: "filters.report.nonTrainingReports"
    }
  },
  table: {
    noData: "table.noData"
  },
  form: {
    cancel: "form.cancel",
    confirm: "form.confirm",
    inlineSave: "form.inlineSave"
  },
  user: {
    logout: "user.logout",
    verifyEmail: {
      setPassword: "user.verifyEmail.setPassword",
      welcome: "user.verifyEmail.welcome",
      signIn: "user.verifyEmail.signIn",
      password: "user.verifyEmail.password",
      failed: "user.verifyEmail.failed"
    },
    registration: {
      passwordTooWeak: "user.registration.passwordTooWeak"
    },
    resetPassword: {
      success: "user.resetPassword.success",
      failed: "user.resetPassword.failed",
      enterEmail: "user.resetPassword.enterEmail",
      emailAddress: "user.resetPassword.emailAddress",
      submit: "user.resetPassword.submit",
      enterNewPassword: "user.resetPassword.enterNewPassword",
      notFound: "user.resetPassword.notFound",
      emailSent: "user.resetPassword.emailSent"
    }
  },
  validation: {
    invalidPhoneNumber: "validation.invalidPhoneNumber",
    fieldRequired: "validation.fieldRequired",
    tooShortString: "validation.tooShortString",
    tooLongString: "validation.tooLongString",
    invalidEmail: "validation.invalidEmail",
    invalidInteger: "validation.invalidInteger",
    phoneNumberInvalid: "validation.phoneNumberInvalid",
    invalidModuloTen: "validation.invalidModuloTen",
    valueCannotBeNegative: "validation.valueCannotBeNegative",
    inRange: "validation.valueMustBeInRange"
  },
  headManagerConsents: {
    title: "headManagerConsents.title",
    consentText: "headManagerConsents.consentText",
    nationalSociety: "headManagerConsents.nationalSociety",
    nationalSocieties: "headManagerConsents.nationalSocieties",
    agreeToContinue: "headManagerConsents.agreeToContinue",
    submit: "headManagerConsents.submit",
    iConsent: "headManagerConsents.iConsent",
    setAsHeadManager: "headManagerConsents.setAsHeadManager",
    pendingHeadManager: "headManagerConsents.pendingHeadManager",
    setSuccessfully: "headManagerConsents.setSuccessfully",
    selectLanguage: "headManagerConsents.selectLanguage",
    downloadDirectly: "headManagerConsents.downloadDirectly"
  },
  translations: {
    title: "translations.title",
    list: {
      key: "translations.list.key"
    }
  }
};

export const extractString = (keyValue, noEditor) =>
  strings(extractKey(keyValue), noEditor);

export const strings = (key, noEditor) => {
  const value = stringList[key];

  if (showKeys) {
    return noEditor
      ? key
      : <StringsEditor stringKey={key} />;
  }

  return value === undefined ? key : value;
}

export const stringsFormat = (key, data, noEditor) => {
  const value = stringList[key];

  if (showKeys) {
    return noEditor
      ? key
      : <StringsEditor stringKey={key} />;
  }

  if (value === undefined) {
    return key;
  }

  return Object.keys(data || {}).reduce((result, stringKey) => result.replace(`{${stringKey}}`, data[stringKey]), value || "");
}

const stringPrefix = "string:";

export const stringKey = (key) =>
  `${stringPrefix}${key}`;

export const extractKey = (key) =>
  (key && key.substr(0, stringPrefix.length) === stringPrefix)
    ? key.substring(stringPrefix.length)
    : key

export function updateStrings(strings) {
  Object.assign(stringList, strings);
}

export function toggleStringsMode() {
  showKeys = !showKeys;
}

export default stringList;
