export const getSaveFormModel = (values, contentLanguages) =>
  ({
    id: values.id,
    healthRiskCode: parseInt(values.healthRiskCode),
    healthRiskType: values.healthRiskType,
    alertRuleCountThreshold: parseInt(values.alertRuleCountThreshold),
    alertRuleHoursThreshold: parseInt(values.alertRuleHoursThreshold),
    alertRuleMetersThreshold: parseInt(values.alertRuleMetersThreshold),
    languageContent: contentLanguages.map(lang => ({
      languageId: lang.id,
      name: values[`contentLanguage_${lang.id}_name`],
      caseDefinition: values[`contentLanguage_${lang.id}_caseDefinition`],
      feedbackMessage: values[`contentLanguage_${lang.id}_feedbackMessage`]
    }))
  });