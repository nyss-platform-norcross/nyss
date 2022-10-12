export const getSaveFormModel = (values, contentLanguages, suspectedDiseases) =>
  ({
    healthRiskCode: parseInt(values.healthRiskCode),
    healthRiskType: values.healthRiskType,
    alertRuleCountThreshold: parseInt(values.alertRuleCountThreshold),
    alertRuleDaysThreshold: parseInt(values.alertRuleDaysThreshold),
    alertRuleKilometersThreshold: parseInt(values.alertRuleKilometersThreshold),
    suspectedDiseases: suspectedDiseases.map(suspectedDisease => ({
    //id: values[`suspectedDisease.${suspectedDisease.suspectedDiseaseCode}.healthRiskSuspectedDiseaseId`],
    //SuspectedDiseaseName: sd.SuspectedDiseaseName
      //suspectedDiseaseCode: suspectedDisease.suspectedDiseaseCode
      suspectedDiseaseId: suspectedDisease.id
      /*langlanguageContents: contentLanguages.map(lang => ({
        languageId: lang.id,
        name: values[`contentLanguage_${lang.id}_name`]
      }))*/
  })),
    languageContent: contentLanguages.map(lang => ({
      languageId: lang.id,
      name: values[`contentLanguage_${lang.id}_name`],
      caseDefinition: values[`contentLanguage_${lang.id}_caseDefinition`],
      feedbackMessage: values[`contentLanguage_${lang.id}_feedbackMessage`]
    }))
});

