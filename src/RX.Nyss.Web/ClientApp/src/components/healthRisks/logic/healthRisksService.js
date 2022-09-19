export const getSaveFormModel = (values, contentLanguages) =>
  ({
    healthRiskCode: parseInt(values.healthRiskCode),
    healthRiskType: values.healthRiskType,
    alertRuleCountThreshold: parseInt(values.alertRuleCountThreshold),
    alertRuleDaysThreshold: parseInt(values.alertRuleDaysThreshold),
    alertRuleKilometersThreshold: parseInt(values.alertRuleKilometersThreshold),
    suspectedDiseases: suspectedDiseases.map(suspectedDisease => ({
      id: values[`suspectedDisease.${suspectedDisease.suspectedDiseaseId}.projectSuspectedDiseaseId`],
      suspectedDiseaseId: suspectedDisease.suspectedDiseaseId
      //feedbackMessage: values[`healthRisk.${healthRisk.healthRiskId}.feedbackMessage`],
      //caseDefinition: values[`healthRisk.${healthRisk.healthRiskId}.caseDefinition`],
      //alertRuleCountThreshold: parseInt(values[`healthRisk.${healthRisk.healthRiskId}.alertRuleCountThreshold`]),
      //alertRuleDaysThreshold: parseInt(values[`healthRisk.${healthRisk.healthRiskId}.alertRuleDaysThreshold`]),
      //alertRuleKilometersThreshold: parseInt(values[`healthRisk.${healthRisk.healthRiskId}.alertRuleKilometersThreshold`])
    })),
    languageContent: contentLanguages.map(lang => ({
      languageId: lang.id,
      name: values[`contentLanguage_${lang.id}_name`],
      caseDefinition: values[`contentLanguage_${lang.id}_caseDefinition`],
      feedbackMessage: values[`contentLanguage_${lang.id}_feedbackMessage`]
    }))
  });