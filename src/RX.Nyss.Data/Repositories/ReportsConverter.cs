using System;
using System.Collections.Generic;
using System.Linq;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data.Repositories;

public interface IReportsConverter
{
    List<EidsrDbReportData> ConvertReports(List<RawReport> reports, DateTime alertDate, int englishContentLanguageId);
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

        // format for Eidsr purposes data of the reports
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

        // generate aggregated reports - group them by the organisation unit associated with RawReport District
        return SquashReports(eidsrDbReportData);
    }

    private EidsrDbReportData ConvertSingleReport(RawReport rawReport, DateTime alertDate, int englishContentLanguageId)
    {
        if (rawReport.Village.District.EidsrOrganisationUnits.OrganisationUnitId == null)
        {
            throw new ArgumentException("Report's location has no organisation unit.");
        }

        return new EidsrDbReportData
        {
            OrgUnit = rawReport.Village.District.EidsrOrganisationUnits.OrganisationUnitId,
            EventDate = alertDate.ToString("yyyy-MM-dd"),
            Location = $"{rawReport?.Village?.District?.Region?.Name}/{rawReport?.Village?.District?.Name}/{rawReport?.Village?.Name}",
            DateOfOnset = rawReport?.Report?.ReceivedAt.ToString("u"),
            PhoneNumber = rawReport?.Report?.PhoneNumber,
            SuspectedDisease = ExtractSuspectedDiseases(englishContentLanguageId, rawReport?.Report),
            EventType = rawReport?.Report?.ProjectHealthRisk?.HealthRisk?.HealthRiskType.ToString(),
            Gender = ExtractGender(report: rawReport?.Report)
        };
    }

    private string ExtractGender(Report report)
    {
        if (report?.ReportedCase != null && (report.ReportedCase.CountFemalesBelowFive == 1 || report.ReportedCase.CountFemalesAtLeastFive == 1))
        {
            return "female";
        }

        if (report?.ReportedCase != null && (report.ReportedCase.CountMalesBelowFive == 1 || report.ReportedCase.CountMalesAtLeastFive == 1))
        {
            return "male";
        }

        return "";
    }

    private static string ExtractSuspectedDiseases(int englishContentLanguageId, Report report)
    {
        try
        {
            var suspectedDiseasesLanguageContents = report
                .ProjectHealthRisk
                .HealthRisk
                .HealthRiskSuspectedDiseases
                .SelectMany(s => s.SuspectedDisease.LanguageContents
                    .Where(c => c.ContentLanguageId == englishContentLanguageId))
                .Select(x => x.Name);

            var suspectedDiseases = string.Join('/', suspectedDiseasesLanguageContents);
            return suspectedDiseases;
        }
        catch (Exception e)
        {
            return "";
        }
    }

    private List<EidsrDbReportData> SquashReports(List<EidsrDbReportData> reports)
    {
        // for each reports group (grouped by org unit) create one EidsrDbReportData
        // (EventDate should be the same for all alerts, but we aggregate by that for safety)
        return reports.GroupBy(x => new
            {
                x.OrgUnit,
                x.EventDate
            })
            .Select(orgUnitGroup =>
            {
                return new EidsrDbReportData
                {
                    OrgUnit = orgUnitGroup.Key.OrgUnit,
                    EventDate = orgUnitGroup.Key.EventDate,
                    Gender = CreateValuesAndCountsString(orgUnitGroup.Select(x => x.Gender).ToList()),
                    Location = CreateValuesAndString(orgUnitGroup.Select(x => x.Location).ToList()),
                    EventType = CreateValuesAndCountsString(orgUnitGroup.Select(x => x.EventType).ToList()),
                    PhoneNumber = CreateValuesAndCountsString(orgUnitGroup.Select(x => x.PhoneNumber).ToList()),
                    SuspectedDisease = CreateValuesAndString(orgUnitGroup.Select(x => x.SuspectedDisease).ToList()),
                    DateOfOnset = CreateValuesAndCountsString(orgUnitGroup.Select(x => x.DateOfOnset).ToList())
                };
            })
            .ToList();
    }

    private string CreateValuesAndCountsString(List<string> list)
    {
        var valueCountPairs = from x in list
            group x by x into g
            let count = g.Count()
            orderby count descending
            select new { Value = g.Key, Count = count };

        return string.Join(", ", valueCountPairs
            .Where(valueCountPair => !string.IsNullOrEmpty(valueCountPair.Value))
            .Select(valueCountPair =>
                valueCountPair.Count == 1
                    ? $"{valueCountPair.Value}"
                    : $"{valueCountPair.Value} ({valueCountPair.Count})"));
    }

    private string CreateValuesAndString(List<string> list)
    {
        var valuePairs = from x in list
                              group x by x into g
                              select new { Value = g.Key};

        return string.Join(", ", valuePairs
            .Where(valuePairs => !string.IsNullOrEmpty(valuePairs.Value))
            .Select(valuePairs => $"{valuePairs.Value}"));
    }
}
