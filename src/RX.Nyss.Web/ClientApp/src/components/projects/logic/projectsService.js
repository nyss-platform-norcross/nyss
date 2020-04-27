export const getSaveFormModel = (values, healthRisks, alertRecipients) =>
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
        alertNotificationRecipients: alertRecipients.map(ar => ({
            role: values[`alertRecipientRole${ar}`],
            organization: values[`alertRecipientOrganization${ar}`],
            email: values[`alertRecipientEmail${ar}`],
            phoneNumber: values[`alertRecipientPhone${ar}`]
        }))
    });