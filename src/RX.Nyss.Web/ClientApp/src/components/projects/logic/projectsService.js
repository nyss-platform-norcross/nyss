export const getSaveFormModel = (values, healthRisks, emailNotifications, smsNotifications) =>
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
        emailAlertRecipients: emailNotifications.map(emailNotification => ({
            id: values[`email_notification_${emailNotification.key}_id`],
            email: values[`email_notification_${emailNotification.key}_email`],
        })),
        smsAlertRecipients: smsNotifications.map(smsNotification => ({
            id: values[`sms_notification_${smsNotification.key}_id`],
            phoneNumber: values[`sms_notification_${smsNotification.key}_phone_number`],
        }))
    });