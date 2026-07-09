export const getSaveFormModel = (
  values,
  contentLanguages,
  selectedSuspectedDiseases,
) => ({
  healthRiskCode: parseInt(values.healthRiskCode),
  healthRiskType: values.healthRiskType,
  alertRuleCountThreshold: parseInt(values.alertRuleCountThreshold),
  alertRuleDaysThreshold: parseInt(values.alertRuleDaysThreshold),
  alertRuleKilometersThreshold: parseInt(values.alertRuleKilometersThreshold),
  healthRiskSuspectedDiseases: selectedSuspectedDiseases.map((sd) => ({
    suspectedDiseaseId: sd.suspectedDiseaseId,
  })),
  languageContent: contentLanguages.map((lang) => ({
    languageId: lang.id,
    name: values[`contentLanguage_${lang.id}_name`],
    caseDefinition: values[`contentLanguage_${lang.id}_caseDefinition`],
    feedbackMessage: values[`contentLanguage_${lang.id}_feedbackMessage`],
  })),
});
