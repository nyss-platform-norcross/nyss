using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data.Tests.Repositories;

public static class ReportsData
{
    public static RawReport CreateRawReport()
    {
        var rawReport = new RawReport();
        return rawReport;
    }

    public static RawReport AddLocation(
        this RawReport rawReport,
        string? village,
        string? district,
        string? orgUnit,
        string? region)
    {
        rawReport.Village = new Village
        {
            Name = village,
            District = new District
            {
                Name = district,
                EidsrOrganisationUnits = new EidsrOrganisationUnits { OrganisationUnitId = orgUnit },
                Region = new Region { Name = region }
            }
        };

        return rawReport;
    }

    public static Report AddReport(
        this RawReport rawReport,
        string phoneNumber,
        DateTime? receivedAt,
        bool? female)
    {
        rawReport.Report = new Report
        {
            PhoneNumber = phoneNumber,
            ReceivedAt = receivedAt.GetValueOrDefault(),
        };

        if (female != null && female.Value)
        {
            rawReport.Report.ReportedCase = new ReportCase
            {
                CountFemalesBelowFive = 1,
            };
        }

        if (female != null && !female.Value)
        {
            rawReport.Report.ReportedCase = new ReportCase
            {
                CountMalesBelowFive = 1,
            };
        }

        return rawReport.Report;
    }

    public static HealthRisk AddHealthRisk(
        this Report report,
        bool? human)
    {
        report.ProjectHealthRisk = new ProjectHealthRisk
        {
            HealthRisk = new HealthRisk()
        };

        if (human != null && human.Value)
        {
            report.ProjectHealthRisk.HealthRisk.HealthRiskType = HealthRiskType.Human;
        }
        if (human != null && !human.Value)
        {
            report.ProjectHealthRisk.HealthRisk.HealthRiskType = HealthRiskType.NonHuman;
        }

        return report.ProjectHealthRisk.HealthRisk;
    }

    public static HealthRisk AddSuspectedDiseases(
        this HealthRisk healthRisk,
        int contentLanguageId,
        List<string> diseases)
    {
        healthRisk.HealthRiskSuspectedDiseases = new List<HealthRiskSuspectedDisease>();

        foreach (var disease in diseases)
        {
            healthRisk.HealthRiskSuspectedDiseases.Add(
                new HealthRiskSuspectedDisease
                {
                    SuspectedDisease = new SuspectedDisease
                    {
                        LanguageContents = new List<SuspectedDiseaseLanguageContent>
                    {
                        new SuspectedDiseaseLanguageContent
                        {
                            ContentLanguageId = contentLanguageId,
                            Name = disease
                        }
                    }
                    },
                });
        }

        return healthRisk;
    }
}
