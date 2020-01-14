using System.Collections.Generic;
using RX.Nyss.Data.Models;

namespace RX.Nyss.ReportApi.Tests.Features.Alert.TestData
{
    public class ProjectHealthRiskData
    {
        private const int AlertRuleCountThreshold1 = 1;
        private const int AlertRuleCountThreshold2 = 3;
        private const int AlertRuleCountThreshold0 = 0;
        private const int AlertRuleKilometersThreshold = 1;
        private const int AlertRuleDaysThreshold = 10;

        public static (List<AlertRule>, List<HealthRisk>, List<ProjectHealthRisk>) Create()
        {
            var alertRules = new List<AlertRule>
            {
                new AlertRule{ Id = 1, CountThreshold = AlertRuleCountThreshold1, KilometersThreshold = AlertRuleKilometersThreshold, DaysThreshold = AlertRuleDaysThreshold},
                new AlertRule{ Id = 2, CountThreshold = AlertRuleCountThreshold2, KilometersThreshold = AlertRuleKilometersThreshold, DaysThreshold = AlertRuleDaysThreshold},
                new AlertRule{ Id = 3, CountThreshold = AlertRuleCountThreshold0, KilometersThreshold = AlertRuleKilometersThreshold, DaysThreshold = AlertRuleDaysThreshold}
            };

            var healthRisks = new List<HealthRisk>
            {
                new HealthRisk { Id = 1, AlertRule = alertRules[0] },
                new HealthRisk { Id = 2, AlertRule = alertRules[1] },
                new HealthRisk { Id = 3, AlertRule = alertRules[2] }
            };

            var projectHealthRisks = new List<ProjectHealthRisk>
            {
                new ProjectHealthRisk { Id = 1, AlertRule = alertRules[0] , HealthRiskId = healthRisks[0].Id, HealthRisk = healthRisks[0] },
                new ProjectHealthRisk { Id = 2, AlertRule = alertRules[1] , HealthRiskId = healthRisks[1].Id, HealthRisk = healthRisks[1] },
                new ProjectHealthRisk { Id = 3, AlertRule = alertRules[2] , HealthRiskId = healthRisks[2].Id, HealthRisk = healthRisks[2] }
            };

            return (alertRules, healthRisks, projectHealthRisks);
        }
    }
}
