import React from "react";
import { StringsEditor } from "./components/common/stringsEditor/StringsEditor";

let stringList = {};

let showKeys = false;

export const areStringKeysDisplayed = () => showKeys;

export const stringKeys = {
  error: {
    responseNotSuccessful: "error.responseNotSuccessful",
    notAuthenticated: "error.notAuthenticated",
    unauthorized: "error.unauthorized",
    redirected: "error.redirected",
    tooManyRequests: "error.tooManyRequests",
    errorPage: {
      message: "error.generalErrorMessage",
      goHome: "error.goHome",
      notFound: "error.notFound"
    }
  },
  roles: {
    "TechnicalAdvisor": "roles.technicalAdvisor",
    "Administrator": "roles.administrator",
    "GlobalCoordinator": "roles.globalCoordinator",
    "DataConsumer": "roles.dataConsumer",
    "Manager": "roles.manager",
    "Supervisor": "roles.supervisor",
    "Coordinator": "roles.coordinator"
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
      initialOrganizationName: "nationalSociety.form.initialOrganizationName",
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
      coordinator: "nationalSociety.list.coordinator",
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
        reportCountTitle: "nationalsociety.dashboard.numbers.reportCountTitle",
        keptReportCount: "nationalsociety.dashboard.numbers.keptReportCount",
        dismissedReportCount: "nationalsociety.dashboard.numbers.dismissedReportCount",
        notCrossCheckedReportCount: "nationalsociety.dashboard.numbers.notCrossCheckedReportCount",
        totalReportCount: "nationalsociety.dashboard.numbers.totalReportCount",
        alertsSummaryTitle: "nationalsociety.dashboard.numbers.alertsSummaryTitle",
        dismissedAlerts: "nationalsociety.dashboard.numbers.dismissedAlerts",
        escalatedAlerts: "nationalsociety.dashboard.numbers.escalatedAlerts",
        closedAlerts: "nationalsociety.dashboard.numbers.closedAlerts",
        numberOfVillages: "nationalsociety.dashboard.numbers.numberOfVillages",
        numberOfDistricts: "nationalsociety.dashboard.numbers.numberOfDistricts"
      },
      filters: {
        title: "nationalsociety.dashboard.filters.title",
        startDate: "nationalsociety.dashboard.filters.startDate",
        endDate: "nationalsociety.dashboard.filters.endDate",
        timeGrouping: "nationalsociety.dashboard.filters.timeGrouping",
        timeGroupingDay: "nationalsociety.dashboard.filters.timeGrouping.day",
        timeGroupingWeek: "nationalsociety.dashboard.filters.timeGrouping.week",
        healthRisk: "nationalsociety.dashboard.filters.healthRisk",
        organization: "nationalsociety.dashboard.filters.organization",
        healthRiskAll: "nationalsociety.dashboard.filters.healthRiskAll",
        organizationsAll: "nationalsociety.dashboard.filters.organizationsAll",
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
      geographicalCoverage: "nationalsociety.dashboard.geographicalCoverage",
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
      translationsSection: "healthRisk.form.translationsSection",
      alertsSection: "healthRisk.form.alertsSection",
      noAlertRule: "healthRisk.form.noAlertRule",
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
    apiKeyCopied: "smsGateway.apiKeyCopied",
    form: {
      creationTitle: "smsGateway.form.creationTitle",
      editionTitle: "smsGateway.form.editionTitle",
      name: "smsGateway.form.name",
      apiKey: "smsGateway.form.apiKey",
      gatewayType: "smsGateway.form.gatewayType",
      emailAddress: "smsGateway.form.emailAddress",
      create: "smsGateway.form.create",
      update: "smsGateway.form.update",
      useIotHub: "smsGateway.form.useIotHub",
      iotHubDeviceName: "smsGateway.form.iotHubDeviceName",
      ping: "smsGateway.form.ping",
      pingIsRequired: "smsGateway.form.pingIsRequired",
      useDualModem: "smsGateway.form.useDualModem",
      modemOneName: "smsGateway.form.modemOneName",
      modemTwoName: "smsGateway.form.modemTwoName"
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
      removalConfirmation: "smsGateway.list.removalConfirmation",
      useIotHub: "smsGateway.list.useIotHub"
    },
  },
  organization: {
    title: "organization.title",
    addNew: "organization.addNew",
    form: {
      creationTitle: "organization.form.creationTitle",
      editionTitle: "organization.form.editionTitle",
      name: "organization.form.name",
      create: "organization.form.create",
      update: "organization.form.update"
    },
    create: {
      success: "organization.create.success"
    },
    edit: {
      success: "organization.edit.success"
    },
    delete: {
      success: "organization.delete.success"
    },
    list: {
      name: "organization.list.name",
      projects: "organization.list.projects",
      headManager: "organization.list.headManager",
      removalConfirmation: "organization.list.removalConfirmation",
      isDefaultOrganization: "organization.list.isDefaultOrganization"
    },
  },
  projectOrganization: {
    title: "projectOrganization.title",
    addNew: "projectOrganization.addNew",
    form: {
      creationTitle: "projectOrganization.form.creationTitle",
      organization: "projectOrganization.form.organization",
      create: "projectOrganization.form.create",
      update: "projectOrganization.form.update"
    },
    create: {
      success: "projectOrganization.create.success"
    },
    delete: {
      success: "projectOrganization.delete.success"
    },
    list: {
      name: "projectOrganization.list.name",
      removalConfirmation: "projectOrganization.list.removalConfirmation"
    },
  },
  projectAlertRecipient: {
    title: "projectAlertRecipient.title",
    addNew: "projectAlertRecipient.addNew",
    form: {
      creationTitle: "projectAlertRecipient.form.creationTitle",
      editionTitle: "projectAlertRecipient.form.editionTitle",
      role: "projectAlertRecipient.form.role",
      organization: "projectAlertRecipient.form.organization",
      projectOrganization: "projectAlertRecipient.form.projectOrganization",
      email: "projectAlertRecipient.form.email",
      phoneNumber: "projectAlertRecipient.form.phoneNumber",
      create: "projectAlertRecipient.form.create",
      update: "projectAlertRecipient.form.update",
      supervisors: "projectAlertRecipient.form.supervisors",
      healthRisks: "projectAlertRecipient.form.healthRisks",
      anyHealthRisk: "projectAlertRecipient.form.anyHealthRisk",
      triggerDetails: "projectAlertRecipient.form.triggerDetails",
      receiverDetails: "projectAlertRecipient.form.receiverDetails",
      anySupervisor: "projectAlertRecipient.form.anySupervisor",
      modem: "projectAlertRecipient.form.modem"
    },
    create: {
      success: "projectAlertRecipient.create.success"
    },
    edit: {
      success: "projectAlertRecipient.edit.success"
    },
    delete: {
      success: "projectAlertRecipient.delete.success"
    },
    list: {
      role: "projectAlertRecipient.list.role",
      organization: "projectAlertRecipient.list.organization",
      email: "projectAlertRecipient.list.email",
      phoneNumber: "projectAlertRecipient.list.phoneNumber",
      removalConfirmation: "projectAlertRecipient.list.removalConfirmation",
      supervisors: "projectAlertRecipient.list.supervisors",
      healthRisks: "projectAlertRecipient.list.healthRisks"
    },
  },
  projectAlertNotHandledRecipient: {
    title: "projectAlertNotHandledRecipient.title",
    description: "projectAlertNotHandledRecipient.description",
    create: "projectAlertNotHandledRecipient.create",
    update: "projectAlertNotHandledRecipient.update"
  },
  project: {
    title: "project.title",
    settingsRootTitle: "project.settingsRootTitle",
    addNew: "project.addNew",
    edit: "project.edit",
    form: {
      creationTitle: "project.form.creationTitle",
      editionTitle: "project.form.editionTitle",
      name: "project.form.name",
      allowMultipleOrganizations: "project.form.allowMultipleOrganizations",
      organization: "project.form.organization",
      timeZone: "project.form.timeZone",
      healthRisks: "project.form.healthRisks",
      healthRisksSection: "project.form.healthRisksSection",
      overviewHealthRisksSection: "project.form.overviewHealthRisksSection",
      caseDefinition: "project.form.caseDefinition",
      feedbackMessage: "project.form.feedbackMessage",
      alertsSection: "project.form.alertsSection",
      alertRuleCountThreshold: "project.form.alertRuleCountThreshold",
      alertRuleDaysThreshold: "project.form.alertRuleDaysThreshold",
      alertRuleKilometersThreshold: "project.form.alertRuleKilometersThreshold",
      alertNotificationsSection: "project.form.alertNotificationsSection",
      overviewAlertNotificationsSection: "project.form.overviewAlertNotificationsSection",
      alertNotificationsDescription: "project.form.alertNotificationsDescription",
      alertNotificationsSupervisorsExplanation: "project.form.alertNotificationsSupervisorExplanation",
      alertNotificationsRole: "project.form.alertNotificationsRole",
      alertNotificationsOrganization: "project.form.alertNotificationsOrganization",
      alertNotificationsEmail: "project.form.alertNotificationsEmail",
      alertNotificationsPhoneNumber: "project.form.alertNotificationsPhoneNumber",
      addRecipient: "project.form.addRecipient",
      addEmail: "project.form.addEmail",
      addSms: "project.form.addSms",
      create: "project.form.create",
      update: "project.form.update",
      alertNotHandledNotificationRecipient: "project.form.alertNotHandledNotificationRecipient"
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
        reportCountTitle: "project.dashboard.numbers.reportCountTitle",
        keptReportCount: "project.dashboard.numbers.keptReportCount",
        dismissedReportCount: "project.dashboard.numbers.dismissedReportCount",
        notCrossCheckedReportCount: "project.dashboard.numbers.notCrossCheckedReportCount",
        totalReportCount: "project.dashboard.numbers.totalReportCount",
        openAlerts: "project.dashboard.numbers.openAlerts",
        dismissedAlerts: "project.dashboard.numbers.dismissedAlerts",
        escalatedAlerts: "project.dashboard.numbers.escalatedAlerts",
        closedAlerts: "project.dashboard.numbers.closedAlerts",
        numberOfVillages: "project.dashboard.numbers.villages",
        numberOfDistricts: "project.dashboard.numbers.districts"
      },
      filters: {
        startDate: "project.dashboard.filters.startDate",
        endDate: "project.dashboard.filters.endDate",
        timeGrouping: "project.dashboard.filters.timeGrouping",
        timeGroupingDay: "project.dashboard.filters.timeGrouping.day",
        timeGroupingWeek: "project.dashboard.filters.timeGrouping.week",
        organization: "project.dashboard.filters.organization",
        healthRisk: "project.dashboard.filters.healthRisk",
        organizationsAll: "project.dashboard.filters.organizationsAll",
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
        unspecifiedAge: "project.dashboard.reportsPerFeature.unspecifiedAge",
        unspecifiedSex: "project.dashboard.reportsPerFeature.unspecifiedSex"
      },
      reportsPerFeatureAndDate: {
        femalesBelow5: "project.dashboard.reportsPerFeatureAndDate.femalesBelow5",
        femalesAbove5: "project.dashboard.reportsPerFeatureAndDate.femalesAbove5",
        malesBelow5: "project.dashboard.reportsPerFeatureAndDate.malesBelow5",
        malesAbove5: "project.dashboard.reportsPerFeatureAndDate.malesAbove5",
        unspecifiedSexAndAge: "project.dashboard.reportsPerFeatureAndDate.unspecifiedSexAndAge",
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
      geographicalCoverageSummary: "project.dashboard.geographicalCoverageSummary",
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
      organization: "nationalSocietyUser.form.organization",
      phoneNumber: "nationalSocietyUser.form.phoneNumber",
      additionalPhoneNumber: "nationalSocietyUser.form.additionalPhoneNumber",
      customOrganization: "nationalSocietyUser.form.customOrganization",
      create: "nationalSocietyUser.form.create",
      addExisting: "nationalSocietyUser.form.addExisting",
      projectIsClosed: "nationalSocietyUser.form.projectIsClosed",
      update: "nationalSocietyUser.form.update",
      alertRecipients: "nationalSocietyUser.form.alertRecipients",
      alertRecipientsAllNotAllowed: "nationalSocietyUser.form.alertRecipientsAllNotAllowed",
      alertRecipientsAll: "nationalSocietyUser.form.alertRecipientsAll",
      confirmCoordinatorCreation: "nationalSocietyUser.form.confirmCoordinatorCreation",
      confirmCoordinatorCreationText: "nationalSocietyUser.form.confirmCoordinatorCreationText",
      headSupervisor: "nationalSocietyUser.form.headSupervisor",
      headSupervisorNotAssigned: "nationalSocietyUser.form.headSupervisorNotAssigned",
      modem: "nationalSocietyUser.form.modem"
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
      headManager: "nationalSocietyUser.list.headManager",
      organization: "nationalSocietyUser.list.organization",
      notVerified: "nationalSocietyUser.list.notVerified"
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
      retrieveLocation: "dataCollectors.form.retrieveLocation",
      replaceSupervisor: "dataCollectors.form.replaceSupervisor",
      newSupervisor: "dataCollectors.form.newSupervisor",
      replaceSupervisorWarning: "dataCollectors.form.replaceSupervisorWarning",
      selectLocation: "dataCollectors.form.selectLocation",
      deployed: "dataCollectors.form.deployed",
      addLocation: "dataCollectors.form.addLocation",
      personalia: "dataCollectors.form.personalia",
      location: "dataCollectors.form.location",
      removeLocation: "dataCollectors.form.removeLocation",
      locationsHeader: "dataCollectors.form.locationsHeader",
      showYourLocation: "dataCollectors.form.showYourLocation",
      retrievingLocation: "dataCollectors.form.retrievingLocation"
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
      setToInTraining: "dataCollectors.list.setToInTraining",
      takeOutOfTraining: "dataCollectors.list.takeOutOfTraining",
      supervisor: "dataCollectors.list.supervisor",
      replaceSupervisor: "dataCollectors.list.replaceSupervisor",
      supervisorReplacedSuccessfully: "dataCollectors.list.supervisorReplacedSuccessfully",
      setToDeployed: "dataCollectors.list.setToDeployed",
      setToNotDeployed: "dataCollectors.list.setToNotDeployed"
    },
    performanceList: {
      title: "dataCollectors.performanceList.title",
      completenessTitle: "dataCollectors.performanceList.completenessTitle",
      completenessDescription: "dataCollectors.performanceList.completenessDescription",
      completenessValueDescription: "dataCollectors.performanceList.completenessValueDescription",
      name: "dataCollectors.performanceList.name",
      phoneNumber: "dataCollectors.performanceList.phoneNumber",
      villageName: "dataCollectors.performanceList.villageName",
      daysSinceLastReport: "dataCollectors.performanceList.daysSinceLastReport",
      epiWeek: "dataCollectors.performanceList.epiWeek",
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
      trainingStatus: "dataCollectors.filters.trainingStatus",
      deployedMode: "dataCollectors.filters.deployedMode",
      resetAll: "dataCollectors.filters.resetAll",
      name: "dataCollectors.filters.name"
    },
    performanceListFilters: {
      name: "dataCollectors.performanceListFilters.name"
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
      },
      trainingStatus: {
        "All": "dataCollectors.trainingStatus.all",
        "InTraining": "dataCollectors.trainingStatus.inTraining",
        "Trained": "dataCollectors.trainingStatus.trained"
      },
      deployedMode: {
        "All": "dataCollectors.deployedMode.all",
        "Deployed": "dataCollectors.deployedMode.deployed",
        "NotDeployed": "dataCollectors.deployedMode.notDeployed"
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
      editedSuccesfully: "reports.list.editedSuccessfully",
      sendReport: "reports.list.sendReport",
      goToAlert: "reports.list.goToAlert",
      acceptReport: "reports.list.acceptReport",
      dismissReport: "reports.list.dismissReport",
      acceptReportSuccess: "reports.list.acceptReportSuccess",
      dismissReportSuccess: "reports.list.dismissReportSuccess",
      errorType: "reports.list.errorType"
    },
    form: {
      title: "reports.form.editionTitle",
      senderSectionTitle: "reports.form.senderSectionTitle",
      contentSectionTitle: "reports.form.contentSectionTitle",
      statusSectionTitle: "reports.form.statusSectionTitle",
      reportStatus: "reports.form.reportStatus",
      reportSex: "reports.form.reportSex",
      reportAge: "reports.form.reportAge",
      dataCollector: "reports.form.dataCollector",
      reportPartOfAlertLabel: "reports.form.reportPartOfAlertLabel",
      dataCollectorLocations: "reports.form.dataCollectorLocations",
      selectDcFirst: "reports.form.selectDcFirst",
      selectDcAndLocationFirst: "reports.form.selectDcAndLocationFirst",
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
    correctReportsTitle: "correctReports.title",
    incorrectReportsTitle: "incorrectReports.title",
    sendReport: {
      dataCollector: "reports.sendReport.dataCollector",
      message: "reports.sendReport.message",
      dateOfReport: "reports.sendReport.dateOfReport",
      timeOfReport: "reports.sendReport.timeOfReport",
      sendReport: "reports.sendReport.sendReport",
      goToReportList: "reports.sendReport.goToReportList",
      success: "reports.sendReport.success",
      modem: "reports.sendReport.modem"
    },
    errorTypes: {
      "GlobalHealthRiskCodeNotFound": "report.errorType.globalHealthRiskCodeNotFound",
      "HealthRiskNotFound": "report.errorType.healthRiskNotFound",
      "FormatError": "report.errorType.formatError",
      "GatewayError": "report.errorType.gatewayError",
      "TooLong": "report.errorType.tooLong",
      "DataCollectorUsedCollectionPointFormat": "report.errorType.dataCollectorUsedCollectionPointFormat",
      "CollectionPointUsedDataCollectorFormat": "report.errorType.collectionPointUsedDataCollectorFormat",
      "CollectionPointNonHumanHealthRisk": "report.errorType.collectionPointNonHumanHealthRisk",
      "SingleReportNonHumanHealthRisk": "report.errorType.singleReportNonHumanHealthRisk",
      "AggregateReportNonHumanHealthRisk": "report.errorType.aggregateReportNonHumanHealthRisk",
      "EventReportHumanHealthRisk": "report.errorType.eventReportHumanHealthRisk",
      "GenderAndAgeNonHumanHealthRisk": "report.errorType.genderAndAgeNonHumanHealthRisk",
      "Gateway": "report.errorType.gateway",
      "Other": "report.errorType.other"
    },
    status: {
      "New": "report.status.notCrossChecked",
      "Rejected": "report.status.rejected",
      "Accepted": "report.status.accepted",
      "Pending": "report.status.notCrossChecked",
      "Closed": "report.status.notCrossChecked"
    },
    sexAgeConstants: {
      "male": "report.sexAgeConstants.male",
      "female": "report.sexAgeConstants.female",
      "unspecified": "report.sexAgeConstants.unspecified",
      "belowFive": "report.sexAgeConstants.belowFive",
      "aboveFour": "report.sexAgeConstants.aboveFour"
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
      markedAsError: "nationalSocietyReports.list.markedAsError",
      errorType: "nationalSocietyReports.list.errorType"
    },
    title: "nationalSocietyReports.title"
  },
  alerts: {
    list: {
      id: "alerts.list.id",
      title: "alerts.list.title",
      dateTime: "alerts.list.dateTime",
      healthRisk: "alerts.list.healthRisk",
      reportCount: "alerts.list.reportCount",
      village: "alerts.list.village",
      status: "alerts.list.status",
      success: "alerts.list.success",
      error: "alerts.list.error",
      export: "alerts.list.export"
    },
    filters: {
      healthRisks: "alerts.filters.healthRisks",
      healthRisksAll: "alerts.filters.healthRisksAll",
      status: "alerts.filters.status"
    },
    details: {
      title: "alerts.details.title"
    },
    assess: {
      title: "alerts.assess.title",
      caseDefinition: "alerts.assess.caseDefinition",
      introduction: "alerts.assess.introduction",
      reports: "alerts.assess.reports",
      closeReason: "alerts.assess.closeReason",
      statusDescription: {
        closed: "alerts.assess.statusDescription.closed",
        escalated: "alerts.assess.statusDescription.escalated",
        dismissed: "alerts.assess.statusDescription.dismissed",
        rejected: "alerts.assess.statusDescription.rejected"
      },
      escalatedOutcomes: {
        "Dismissed": "alerts.assess.escalatedOutcomes.dismiss",
        "ActionTaken": "alerts.assess.escalatedOutcomes.actionTaken",
        "Other": "alerts.assess.escalatedOutcomes.other"
      },
      alert: {
        close: "alerts.assess.alert.close",
        closeComments: "alerts.assess.alert.closeComments",
        closeConfirmation: "alerts.assess.alert.closeConfirmation",
        closeDescription: "alerts.assess.alert.closeDescription",
        escalate: "alerts.assess.alert.escalate",
        escalateConfirmation: "alerts.assess.alert.escalateConfirmation",
        escalateConfirmationInformDataCollectors: "alerts.assess.alert.escalateConfirmationInformDataCollectors",
        escalateNotificationEmails: "alerts.assess.alert.escalateNotificationEmails",
        escalateNotificationSmses: "alerts.assess.alert.escalateNotificationSmses",
        escalateWithoutNotification: "alerts.assess.alert.escalateWithoutNotification",
        escalateWithoutNotificationDialogTitle: "alerts.assess.alert.escalateWithoutNotificationDialogTitle",
        escalateWithoutNotificationConfirmation: "alerts.assess.alert.escalateWithoutNotificationConfirmation",
        dismiss: "alerts.assess.alert.dismiss",
        escalationPossible: "alerts.assess.alert.escalationPossible",
        dismissalPossible: "alerts.assess.alert.dismissalPossible",
        escalatedSuccessfully: "alerts.assess.alert.escalatedSuccessfully",
        dismissedSuccessfully: "alerts.assess.alert.dismissedSuccessfully",
        closedSuccessfully: "alerts.assess.alert.closedSuccessfully"
      },
      report: {
        accept: "alerts.assess.report.accept",
        acceptedSuccessfully: "alerts.assess.report.acceptedSuccessfully",
        dismiss: "alerts.assess.report.dismiss",
        dismissedSuccessfully: "alerts.assess.report.dismissedSuccessfully",
        reset: "alerts.assess.report.reset",
        resetSuccessfully: "alerts.assess.report.resetSuccessfully",
        sender: "alerts.assess.report.sender",
        phoneNumber: "alerts.assess.report.phoneNumber",
        village: "alerts.assess.report.village",
        district: "alerts.assess.report.district",
        region: "alerts.assess.report.region",
        sex: "alerts.assess.report.sex",
        age: "alerts.assess.report.age",
        id: "alerts.assess.report.id",
        linkedToSupervisor: "alerts.assess.report.linkedToSupervisor"
      }
    },
    eventLog: {
      title: "alerts.eventLog.title",
      addNew: "alerts.eventLog.addNew",
      edit: "alerts.eventLog.edit",      
      list: {
        date: "alerts.eventLog.list.date",
        userName: "alerts.eventLog.list.userName",
        type: "alerts.eventLog.list.type",
        subtype: "alerts.eventLog.list.subtype",
        comment: "alerts.eventLog.list.comment",
        removalConfirmation: "alerts.eventLog.list.removalConfirmation"
      },
      form: {
        dateOfEvent: "alerts.eventLog.form.dateOfEvent",
        timeOfEvent: "alerts.eventLog.form.timeOfEvent",
        comment: "alerts.eventLog.form.comment",
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
      escalatedOutcomes: {
        "Dismissed": "alerts.escalatedOutcomes.dismissed",
        "ActionTaken": "alerts.escalatedOutcomes.actionTaken",
        "Other": "alerts.escalatedOutcomes.other"
      },
      reportStatus: {
        "Rejected": "alerts.reportStatus.rejected",
        "Accepted": "alerts.reportStatus.accepted",
        "Pending": "alerts.reportStatus.pending",
        "Closed": "alerts.reportStatus.closed"
      },
      eventTypes: {
        "Investigation": "alerts.eventTypes.investigation",
        "Outcome": "alerts.eventTypes.outcome",
        "Summary": "alerts.eventTypes.summary",
        "PublicHealthActionTaken": "alerts.eventTypes.publicHealthActionTaken",        
      },
      eventSubtypes: {
        "Investigated": "alerts.eventTypes.subtypes.investigated",
        "NotInvestigated": "alerts.eventTypes.subtypes.notInvestigated",
        "Unknown": "alerts.eventTypes.subtypes.unknown",
        "PresumedCasePositiveLab": "alerts.eventTypes.subtypes.presumedCasePositiveLab",
        "PresumedCasePositiveClinical": "alerts.eventTypes.subtypes.presumedCasePositiveClinical",
        "PresumedCasePositiveUnknown": "alerts.eventTypes.subtypes.presumedCasePositiveUnknown",
        "PresumedCaseNegativeLab": "alerts.eventTypes.subtypes.presumedCaseNegativeLab",
        "PresumedCaseNegativeClinical": "alerts.eventTypes.subtypes.presumedCaseNegativeClinical",
        "PresumedCaseNegativeUnknown": "alerts.eventTypes.subtypes.presumedCaseNegativeUnknown",
        "Recovered": "alerts.eventTypes.subtypes.recovered",
        "Deceased": "alerts.eventTypes.subtypes.deceased",
        "ImmunizationCampaign": "alerts.eventTypes.subtypes.immunizationCampaign",
        "HealthMessagesAwarenessRaising": "alerts.eventTypes.subtypes.healthMessagesAwareNessRaising",
        "Referral": "alerts.eventTypes.subtypes.referral",
        "Isolation": "alerts.eventTypes.subtypes.isolation",
        "ProvidedORS": "alerts.eventTypes.subtypes.providedORS",
        "AnimalsDisposed": "alerts.eventTypes.subtypes.AnimalsDisposed",
        "SafeDignifiedBurials": "alerts.eventTypes.subtypes.SafeDignifiedBurials",
        "CommunityMeeting": "alerts.eventTypes.subtypes.communityMeeting",
        "AssistedInvestigation": "alerts.eventTypes.subtypes.assistedInvestigation",
        "CleanupFogging": "alerts.eventTypes.subtypes.cleanupAndOrFogging",
        "Other": "alerts.eventTypes.subtypes.other",
      },
      sex: {
        "Female": "alerts.sex.female",
        "Male": "alerts.sex.male",
        "Unspecified": "alerts.sexAndAge.unspecified"
      },
      age: {
        "BelowFive": "alerts.age.belowFive",
        "AtLeastFive": "alerts.age.atLeastFive",
        "Unspecified": "alerts.sexAndAge.unspecified"
      },
      logType: {
        "TriggeredAlert": "alerts.logType.TriggeredAlert",
        "EscalatedAlert": "alerts.logType.EscalatedAlert",
        "DismissedAlert": "alerts.logType.DismissedAlert",
        "ClosedAlert": "alerts.logType.ClosedAlert",
        "AcceptedReport": "alerts.logType.AcceptedReport",
        "RejectedReport": "alerts.logType.RejectedReport",
        "ResetReport": "alerts.logType.ResetReport"
      }
    },
    title: "alerts.title"
  },
  filters: {
    area: {
      title: "filters.area.title",
      all: "filters.area.all",
      unknown: "filters.area.unknown"
    },
    report: {
      healthRisk: "filters.report.healthRisk",
      healthRiskAll: "filters.report.healthRiskAll",
      selectReportListType: "filters.report.selectReportListType",
      mainReportsListType: "filters.report.mainReportsListType",
      dcpReportListType: "filters.report.dcpReportListType",
      unknownSenderReportListType: "filters.report.unknownSenderReportListType",
      status: "filters.report.status",
      reportType: "filters.report.reportType",
      nonTrainingReports: "filters.report.nonTrainingReports",
      correctedReports: "filters.report.correctedReports",
      kept: "filters.report.keptReports",
      dismissed: "filters.report.dismissedReports",
      notCrossChecked: "filters.report.notCrossCheckedReports",
      selectErrorType: "filters.report.selectErrorType",
      errorTypes: {
        "All": "filters.report.errorTypeAll",
        "HealthRiskNotFound": "filters.report.errorTypeHealthRiskNotFound",
        "WrongFormat": "filters.report.errorTypeWrongFormat",
        "GatewayError": "filters.report.errorTypeGatewayError",
        "Other": "filters.report.errorTypeOther"
      }
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
      emailSent: "user.resetPassword.emailSent",
      goToLoginPage: "user.resetPassword.goToLoginPage"
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
    inRange: "validation.valueMustBeInRange",
    invalidTimeFormat: "validation.invalidTimeFormat",
    sexOrAgeUnspecified: "validation.sexOrAgeUnspecified",
    duplicateLocation: "validation.dataCollector.duplicateLocation"
  },
  nationalSocietyConsents: {
    title: "headManagerConsents.title",
    consentText: "headManagerConsents.consentText",
    nationalSociety: "headManagerConsents.nationalSociety",
    nationalSocietyWithUpdatedAgreement: "headManagerConsents.nationalSocietyWithUpdatedAgreement",
    nationalSocietiesWithUpdatedAgreement: "headManagerConsents.nationalSocietiesWithUpdatedAgreement",
    nationalSocieties: "headManagerConsents.nationalSocieties",
    agreeToContinue: "headManagerConsents.agreeToContinue",
    submit: "headManagerConsents.submit",
    postpone: "headManagerConsents.postpone",
    iConsent: "headManagerConsents.iConsent",
    setAsHeadManager: "headManagerConsents.setAsHeadManager",
    pendingHeadManager: "headManagerConsents.pendingHeadManager",
    setSuccessfully: "headManagerConsents.setSuccessfully",
    selectLanguage: "headManagerConsents.selectLanguage",
    downloadDirectly: "headManagerConsents.downloadDirectly"
  },
  common: {
    boolean: {
      "true": "common.true",
      "false": "common.false"
    }
  },
  translations: {
    title: "translations.title",
    emailTitle: "translations.emailTitle",
    smsTitle: "translations.smsTitle",
    smsCharacters: "translations.smsCharacters",
    smsParts: "translations.smsParts",
    list: {
      key: "translations.list.key"
    },
    needsImprovement: "translations.needsImprovement"
  }
};

export const isStringKey = (key) =>
  (key && key.substr(0, stringPrefix.length) === stringPrefix);

export const extractString = (keyValue, noEditor) =>
  strings(extractKey(keyValue), noEditor);

export const strings = (key, noEditor) => {
  const value = stringList[key];

  if (showKeys) {
    return noEditor
      ? key
      : <StringsEditor stringKey={key} type="strings" needsImprovement={!!value ? value.needsImprovement : true} />;
  }

  return value === undefined ? key : value.value;
}

export const stringsFormat = (key, data, noEditor) => {
  const value = stringList[key];

  if (showKeys) {
    return noEditor
      ? key
      : <StringsEditor stringKey={key} type="strings" needsImprovement={!!value ? value.needsImprovement : true} />;
  }

  if (value === undefined) {
    return key;
  }

  return Object.keys(data || {}).reduce((result, stringKey) => result.replace(`{${stringKey}}`, data[stringKey]), value.value || "");
}

const stringPrefix = "string:";

export const stringKey = (key) =>
  `${stringPrefix}${key}`;

export const extractKey = (key) =>
  isStringKey(key)
    ? key.substring(stringPrefix.length)
    : key

export function updateStrings(strings) {
  Object.assign(stringList, strings);
}

export function toggleStringsMode() {
  showKeys = !showKeys;
}

export default stringList;
