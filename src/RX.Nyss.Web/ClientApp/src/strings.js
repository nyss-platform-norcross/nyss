let stringList = {};

let showKeys = false;

export const areStringKeysDisplayed = () => showKeys;

export const stringKeys = {
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
    list: {
      name: "nationalSociety.list.name",
      country: "nationalSociety.list.country",
      startDate: "nationalSociety.list.startDate",
      headManager: "nationalSociety.list.headManager",
      technicalAdvisor: "nationalSociety.list.technicalAdvisor",
      removalConfirmation: "nationalSociety.list.removalConfirmation",
    },
    dashboard: {
      title: "nationalSociety.dashboard.title"
    },
    overview: {
      title: "nationalSociety.dashboard.overview"
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
      alertRuleMetersThreshold: "healthRisk.form.alertRuleMetersThreshold",
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
      create: "smsGateway.form.create",
      update: "smsGateway.form.update"
    },
    list: {
      name: "smsGateway.list.name",
      apiKey: "smsGateway.list.apiKey",
      gatewayType: "smsGateway.list.gatewayType",
      removalConfirmation: "smsGateway.list.removalConfirmation"
    },
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
  },
  nationalSocietyUser: {
    title: "nationalSocietyUser.title",
    addNew: "nationalSocietyUser.addNew",
    form: {
      creationTitle: "nationalSocietyUser.form.creationTitle",
      editionTitle: "nationalSocietyUser.form.editionTitle",
      name: "nationalSocietyUser.form.name",
      role: "nationalSocietyUser.form.role",
      email: "nationalSocietyUser.form.email",
      phoneNumber: "nationalSocietyUser.form.phoneNumber",
      additionalPhoneNumber: "nationalSocietyUser.form.additionalPhoneNumber",
      organization: "nationalSocietyUser.form.organization",
      create: "nationalSocietyUser.form.create",
      update: "nationalSocietyUser.form.update"
    },
    list: {
      name: "nationalSocietyUser.list.name",
      email: "nationalSocietyUser.list.email",
      phoneNumber: "nationalSocietyUser.list.phoneNumber",
      role: "nationalSocietyUser.list.role",
      project: "nationalSocietyUser.list.project",
      removalConfirmation: "nationalSocietyUser.list.removalConfirmation"
    },
  },
  form: {
    cancel: "form.cancel"
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
      notFound: "user.resetPassword.notFound"
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
  }
};

export const strings = (key) => {
  const value = stringList[key];
  return value === undefined || showKeys ? key : value;
}

export function updateStrings(strings) {
  Object.assign(stringList, strings);
}

export function toggleStringsMode() {
  showKeys = !showKeys;
}

export default stringList;
