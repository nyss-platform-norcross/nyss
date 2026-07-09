export const getSaveFormModel = (values, healthRisks) =>
  ({
    name: values.name,
    allowMultipleOrganizations: values.allowMultipleOrganizations,
    timeZoneId: values.timeZoneId,
    healthRisks: healthRisks.map(healthRisk => ({
      id: values[`healthRisk.${healthRisk.healthRiskId}.projectHealthRiskId`],
      healthRiskId: healthRisk.healthRiskId,
      feedbackMessage: values[`healthRisk.${healthRisk.healthRiskId}.feedbackMessage`],
      caseDefinition: values[`healthRisk.${healthRisk.healthRiskId}.caseDefinition`],
      alertRuleCountThreshold: parseInt(values[`healthRisk.${healthRisk.healthRiskId}.alertRuleCountThreshold`]),
      alertRuleDaysThreshold: parseInt(values[`healthRisk.${healthRisk.healthRiskId}.alertRuleDaysThreshold`]),
      alertRuleKilometersThreshold: parseInt(values[`healthRisk.${healthRisk.healthRiskId}.alertRuleKilometersThreshold`])
    })),
    organizationId: values.organizationId ? parseInt(values.organizationId) : null,
    alertNotHandledNotificationRecipientIds: values.alertNotHandledNotificationRecipientIds.map(recipient => parseInt(recipient.id))
  });
