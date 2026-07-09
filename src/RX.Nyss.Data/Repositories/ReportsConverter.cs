using System;
using System.Collections.Generic;
using System.Linq;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data.Repositories;

public interface IReportsConverter
{
    List<EidsrDbReportData> ConvertReports(List<RawReport> reports, DateTime alertDate, int englishContentLanguageId);
    DhisDbReportData ConvertDhisReport(RawReport rawReport, DateTime reportDate, int englishContentLanguageId);
}

public class ReportsConverter : IReportsConverter
{
    private readonly ILoggerAdapter _logger;

    public ReportsConverter(ILoggerAdapter logger)
    {
        _logger = logger;
    }

    public List<EidsrDbReportData> ConvertReports(List<RawReport> reports, DateTime alertDate, int englishContentLanguageId)
    {
        var eidsrDbReportData = new List<EidsrDbReportData>();

        foreach (var report in reports)
        {
            try
            {
                eidsrDbReportData.Add(ConvertSingleReport(report, alertDate, englishContentLanguageId));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error during converting report to EIDSR report, skipping malformed report.");
            }
        }

        return SquashReports(eidsrDbReportData);
    }

    private EidsrDbReportData ConvertSingleReport(RawReport rawReport, DateTime alertDate, int englishContentLanguageId)
    {
        if (rawReport.Village?.District?.EidsrOrganisationUnits?.OrganisationUnitId == null)
        {
            throw new ArgumentException("Report's location has no organisation unit.");
        }

        return new EidsrDbReportData
        {
            OrgUnit = rawReport.Village.District.EidsrOrganisationUnits.OrganisationUnitId,
            EventDate = alertDate.ToString("yyyy-MM-dd"),
            Location = $"{rawReport.Village.District.Region?.Name}/{rawReport.Village.District.Name}/{rawReport.Village.Name}",
            DateOfOnset = rawReport.Report?.ReceivedAt.ToString("u") ?? string.Empty,
            PhoneNumber = rawReport.Report?.PhoneNumber ?? string.Empty,
            SuspectedDisease = ExtractSuspectedDiseases(englishContentLanguageId, rawReport.Report),
            EventType = ExtractHealthRisk(englishContentLanguageId, rawReport.Report),
            Gender = ExtractGender(report: rawReport.Report)
        };
    }

    private string ExtractGender(Report? report)
    {
        if (report?.ReportedCase == null)
        {
            return string.Empty;
        }

        if (report.ReportedCase.CountFemalesBelowFive == 1 || report.ReportedCase.CountFemalesAtLeastFive == 1)
        {
            return "female";
        }

        if (report.ReportedCase.CountMalesBelowFive == 1 || report.ReportedCase.CountMalesAtLeastFive == 1)
        {
            return "male";
        }

        if (report.ReportedCase.CountUnspecifiedSexAndAge == 1)
        {
            return "Unspecified Sex and Age";
        }

        return string.Empty;
    }

    private string ExtractGender(RawReport rawReport)
    {
        return ExtractGender(rawReport.Report);
    }

    private static string ExtractSuspectedDiseases(int englishContentLanguageId, Report? report)
    {
        try
        {
            if (report?.ProjectHealthRisk?.HealthRisk?.HealthRiskSuspectedDiseases == null)
            {
                return string.Empty;
            }

            var suspectedDiseasesLanguageContents = report.ProjectHealthRisk.HealthRisk.HealthRiskSuspectedDiseases
                .SelectMany(s => s.SuspectedDisease.LanguageContents! 
                    .Where(c => c.ContentLanguageId == englishContentLanguageId))
                .Select(x => x.Name);

            return string.Join('/', suspectedDiseasesLanguageContents);
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    private static string ExtractHealthRisk(int englishContentLanguageId, Report? report)
    {
        try
        {
            if (report?.ProjectHealthRisk?.HealthRisk?.LanguageContents == null)
            {
                return string.Empty;
            }

            var healthRiskLanguageContent = report.ProjectHealthRisk.HealthRisk.LanguageContents
                .Where(c => c.ContentLanguageId == englishContentLanguageId)
                .Select(x => x.Name);

            return healthRiskLanguageContent.FirstOrDefault() ?? string.Empty;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    private List<EidsrDbReportData> SquashReports(List<EidsrDbReportData> reports)
    {
        return reports.GroupBy(x => new
        {
            x.OrgUnit,
            x.EventDate
        })
            .Select(orgUnitGroup => new EidsrDbReportData
            {
                OrgUnit = orgUnitGroup.Key.OrgUnit,
                EventDate = orgUnitGroup.Key.EventDate,
                Gender = CreateValuesAndCountsString(orgUnitGroup.Select(x => x.Gender).ToList()),
                Location = CreateValuesAndString(orgUnitGroup.Select(x => x.Location).ToList()),
                EventType = CreateValuesAndString(orgUnitGroup.Select(x => x.EventType).ToList()),
                PhoneNumber = CreateValuesAndCountsString(orgUnitGroup.Select(x => x.PhoneNumber).ToList()),
                SuspectedDisease = CreateValuesAndString(orgUnitGroup.Select(x => x.SuspectedDisease).ToList()),
                DateOfOnset = CreateValuesAndCountsString(orgUnitGroup.Select(x => x.DateOfOnset).ToList())
            })
            .ToList();
    }

    private string CreateValuesAndCountsString(List<string?> list)
    {
        var valueCountPairs = from x in list
                              where x != null
                              group x by x into g
                              let count = g.Count()
                              orderby count descending
                              select new { Value = g.Key, Count = count };

        return string.Join(", ", valueCountPairs
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => p.Count == 1 ? p.Value : $"{p.Value} ({p.Count})"));
    }

    private string CreateValuesAndString(List<string?> list)
    {
        var valuePairs = from x in list
                         where x != null
                         group x by x into g
                         select new { Value = g.Key };

        return string.Join(", ", valuePairs
            .Where(vp => !string.IsNullOrEmpty(vp.Value))
            .Select(vp => vp.Value));
    }

    public DhisDbReportData ConvertDhisReport(RawReport rawReport, DateTime reportDate, int englishContentLanguageId)
    {
        if (rawReport.Village?.District?.EidsrOrganisationUnits?.OrganisationUnitId == null)
        {
            throw new ArgumentException("Report's location has no organisation unit.");
        }

        return new DhisDbReportData
        {
            OrgUnit = rawReport.Village.District.EidsrOrganisationUnits.OrganisationUnitId,
            EventDate = reportDate.ToString("yyyy-MM-dd"),
            ReportLocation = $"{rawReport.Village.District.Region?.Name}/{rawReport.Village.District.Name}/{rawReport.Village.Name}",
            ReportGeoLocation = rawReport.Report?.Location ?? new NetTopologySuite.Geometries.Point(0, 0),
            ReportSuspectedDisease = ExtractSuspectedDiseases(englishContentLanguageId, rawReport.Report),
            ReportHealthRisk = ExtractHealthRisk(englishContentLanguageId, rawReport.Report),
            ReportStatus = rawReport.Report?.Status.ToString() ?? string.Empty,
            ReportGender = rawReport.Report?.ReportedCase != null &&
                           (rawReport.Report.ReportedCase.CountFemalesAtLeastFive > 0 || rawReport.Report.ReportedCase.CountFemalesBelowFive > 0)
                ? "Female"
                : "Male",
            ReportAgeGroup = rawReport.Report?.ReportedCase != null &&
                             (rawReport.Report.ReportedCase.CountFemalesAtLeastFive > 0 || rawReport.Report.ReportedCase.CountMalesAtLeastFive > 0)
                ? ">5 years"
                : "0-4 years",
            ReportCaseCountFemaleAgeAtLeastFive = rawReport.Report?.ReportedCase?.CountFemalesAtLeastFive ?? 0,
            ReportCaseCountMaleAgeAtLeastFive = rawReport.Report?.ReportedCase?.CountMalesAtLeastFive ?? 0,
            ReportCaseCountFemaleAgeBelowFive = rawReport.Report?.ReportedCase?.CountFemalesBelowFive ?? 0,
            ReportCaseCountMaleAgeBelowFive = rawReport.Report?.ReportedCase?.CountMalesBelowFive ?? 0,
            ReportDate = rawReport.Report?.ReceivedAt.Date.ToString("yyyy-MM-dd") ?? DateTime.Now.Date.ToString("yyyy-MM-dd"),
            ReportTime = rawReport.Report?.ReceivedAt.ToString("HH:mm:ss") ?? DateTime.Now.ToString("HH:mm:ss"),
            ReportDataCollectorId = rawReport.Report?.DataCollector?.Id ?? 0
        };
    }
}