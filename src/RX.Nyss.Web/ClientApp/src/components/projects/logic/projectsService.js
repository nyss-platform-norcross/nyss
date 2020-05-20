export const getSaveFormModel = (values, healthRisks) =>
  ({
    name: values.name,
    allowMultipleOrganizations: values.allowMultipleOrganizations,
    timeZoneId: values.timeZoneId,
    healthRisks: healthRisks.map(healthRisk => ({
      id: values[`healthRisk_${healthRisk.healthRiskId}_projectHealthRiskId`],
      healthRiskId: healthRisk.healthRiskId,
      feedbackMessage: values[`healthRisk_${healthRisk.healthRiskId}_feedbackMessage`],
      caseDefinition: values[`healthRisk_${healthRisk.healthRiskId}_caseDefinition`],
      alertRuleCountThreshold: parseInt(values[`healthRisk_${healthRisk.healthRiskId}_alertRuleCountThreshold`]),
      alertRuleDaysThreshold: parseInt(values[`healthRisk_${healthRisk.healthRiskId}_alertRuleDaysThreshold`]),
      alertRuleKilometersThreshold: parseInt(values[`healthRisk_${healthRisk.healthRiskId}_alertRuleKilometersThreshold`])
    })),
    organizationId: values.organizationId ? parseInt(values.organizationId) : null
  });