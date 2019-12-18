import React from 'react';
import { StringsEditor } from './components/common/stringsEditor/StringsEditor';

let stringList = {};

let showKeys = false;

export const areStringKeysDisplayed = () => showKeys;

export const stringKeys = {
  error: {
    responseNotSuccessful: "error.responseNotSuccessful",
    notAuthenticated: "error.notAuthenticated",
    unauthorized: "error.unauthorized"
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
    form: {
      creationTitle: "nationalSociety.form.creationTitle",
      editionTitle: "nationalSociety.form.editionTitle",
      name: "nationalSociety.form.name",
      country: "nationalSociety.form.country",
      contentLanguage: "nationalSociety.form.contentLanguage",
      create: "nationalSociety.form.create",
      update: "nationalSociety.form.update"
    },
    create: {
      success: "nationalSociety.create.success"
    },
    list: {
      name: "nationalSociety.list.name",
      country: "nationalSociety.list.country",
      startDate: "nationalSociety.list.startDate",
      headManager: "nationalSociety.list.headManager",
      technicalAdvisor: "nationalSociety.list.technicalAdvisor",
      removalConfirmation: "nationalSociety.list.removalConfirmation",
    },
    settings: {
      title: "nationalSociety.settings.title"
    },
    dashboard: {
      title: "nationalSociety.dashboard.title"
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
    form: {
      creationTitle: "project.form.creationTitle",
      editionTitle: "project.form.editionTitle",
      name: "project.form.name",
      timeZone: "project.form.timeZone",
      healthRisks: "project.form.healthRisks",
      healthRisksSetion: "project.form.healthRisksSetion",
      caseDefinition: "project.form.caseDefinition",
      feedbackMessage: "project.form.feedbackMessage",
      alertsSetion: "project.form.alertsSetion",
      alertRuleCountThreshold: "project.form.alertRuleCountThreshold",
      alertRuleDaysThreshold: "project.form.alertRuleDaysThreshold",
      alertRuleKilometersThreshold: "project.form.alertRuleKilometersThreshold",
      emailNotificationsSection: "project.form.emailNotificationsSection",
      emailNotificationDescription: "project.form.emailNotificationDescription",
      smsNotificationsSetion: "project.form.smsNotificationsSetion",
      smsNotificationDescription: "project.form.smsNotificationDescription",
      addEmail: "project.form.addEmail",
      addSms: "project.form.addSms",
      create: "project.form.create",
      update: "project.form.update"
    },
    create: {
      success: "project.create.success"
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
      remove: "project.list.remove",
      removalConfirmation: "project.list.removalConfirmation"
    },
    dashboard: {
      title: "project.dashboard.title",
      numbers: {
        totalReportCount: "project.dashboard.numbers.totalReportCount",
        totalReportCountTitle: "project.dashboard.numbers.totalReportCountTitle"
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
        mainReportsType: "project.dashboard.filters.mainReportsType",
        trainingReportsType: "project.dashboard.filters.trainingReportsType",
      },
      allReportsChart: {
        title: "project.dashboard.allReportsChart.title",
        numberOfReports: "project.dashboard.allReportsChart.numberOfReports",
        periods: "project.dashboard.allReportsChart.periods"
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
      activeDataCollectorCount: "project.dashboard.activeDataCollectorCount",
      inactiveDataCollectorCount: "project.dashboard.inactiveDataCollectorCount",
      startDate: "project.dashboard.startDate",
      dataCollectors: "project.dashboard.dataCollectors",
      healthRisks: "project.dashboard.healthRisks",
      supervisors: "project.dashboard.supervisors",
      supervisorEmailAddress: "project.dashboard.supervisorEmailAddress",
      supervisorPhoneNumber: "project.dashboard.supervisorPhoneNumber",
      supervisorAdditionalPhoneNumber: "project.dashboard.supervisorAdditionalPhoneNumber",
      map: {
        reportCount: "project.dashboard.map.reportCount",
        title: "project.dashboard.map.title",
        reports: "project.dashboard.map.reports",
        report: "project.dashboard.map.report",
      }
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
      update: "nationalSocietyUser.form.update"
    },
    create: {
      success: "nationalSocietyUser.create.success"
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
    dataCollectorType: "dataCollectors.form.dataCollectorType",
    dataCollectorTypeHuman: "dataCollectors.form.dataCollectorType.human",
    dataCollectorTypeCollectionPoint: "dataCollectors.form.dataCollectorType.collectionPoint",
    form: {
      creationTitle: "dataCollectors.form.creationTitle",
      editionTitle: "dataCollectors.form.editionTitle",
      dataCollectorType: "dataCollectors.form.dataCollectorType",
      dataCollectorTypeHuman: "dataCollectors.form.dataCollectorType.human",
      dataCollectorTypeCollectionPoint: "dataCollectors.form.dataCollectorType.collectionPoint",
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
      zone: "dataCollectors.form.zone"
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
      takeOutOfTraining: "dataCollectors.list.takeOutOfTraining"
    },
    constants: {
      sex: {
        male: "dataCollectors.sex.male",
        female: "dataCollectors.sex.female",
        other: "dataCollectors.sex.other",
      }
    },
    mapOverview: {
      title: "dataCollectors.mapOverview.title",
      legend: {
        "ReportingCorrectly": "dataCollectors.mapOverview.legend.reportingCorrectly",
        "NotReporting": "dataCollectors.mapOverview.legend.notReporting",
        "ReportingWithErrors": "dataCollectors.mapOverview.legend.reportingWithErrors"
      }
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
      dataCollectorPhoneNumber: "reports.list.dataCollectorPhoneNumber",
      malesBelowFive: "reports.list.malesBelowFive",
      malesAtLeastFive: "reports.list.malesAtLeastFive",
      femalesBelowFive: "reports.list.femalesBelowFive",
      femalesAtLeastFive: "reports.list.femalesAtLeastFive",
      success: "reports.list.success",
      error: "reports.list.error",
      selectReportListType: "reports.list.selectReportListType",
      mainReportsListType: "reports.list.mainReportsListType",
      trainingReportsListType: "reports.list.trainingReportsListType"

    },
    title: "reports.title"
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
      }
    },
    title: "alerts.title"
  },
  filters: {
    area: {
      title: "filters.area.title",
      all: "filters.area.all"
    }
  },
  table: {
    noData: "table.noData"
  },
  form: {
    cancel: "form.cancel",
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
    invalidModuloTen: "validation.invalidModuloTen"
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
