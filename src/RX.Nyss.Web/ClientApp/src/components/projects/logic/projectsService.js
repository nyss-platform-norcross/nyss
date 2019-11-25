export const getSaveFormModel = (values, healthRisks, notifications) =>
    ({
        name: values.name,
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
        alertRecipients: notifications.map(notification => ({
            id: values[`notification_${notification.key}_id`],
            email: values[`notification_${notification.key}_email`],
        }))
    });