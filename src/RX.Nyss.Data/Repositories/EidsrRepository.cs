using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data.Repositories;

public interface IEidsrRepository
{
    List<EidsrDbReport> GetReportsForEidsr(int alertId);
}

public class EidsrRepository : IEidsrRepository
{
    private readonly INyssContext _nyssContext;
    private readonly IReportsConverter _reportsConverter;

    public EidsrRepository(INyssContext nyssContext, IReportsConverter reportsConverter)
    {
        _nyssContext = nyssContext;
        _reportsConverter = reportsConverter;
    }

    public List<EidsrDbReport> GetReportsForEidsr(int alertId)
    {
        var alert = GetAlert(alertId);
        var englishContentLanguageId = GetEnglishContentLanguageId();
        var eidsrDbReportTemplate = GetReportTemplate(alert.ProjectHealthRisk?.Project?.NationalSociety?.EidsrConfiguration);
        var squashedReports = GetReports(alertId, alert.EscalatedAt ?? alert.CreatedAt, englishContentLanguageId);

        return squashedReports.Select(aggregatedReport => new EidsrDbReport
        {
            EidsrDbReportTemplate = eidsrDbReportTemplate,
            EidsrDbReportData = aggregatedReport
        }).ToList();
    }

    private List<EidsrDbReportData> GetReports(int alertId, DateTime alertDate, int englishContentLanguageId)
    {
        var reportsOfAlertIds = _nyssContext.AlertReports
            .Where(x => x.AlertId == alertId)
            .Select(x => x.ReportId)
            .ToList();

        var reports = GetReportFilterQuery()
            .Where(queriedReport => reportsOfAlertIds.Contains(queriedReport.Report!.Id))
            .ToList();

        return _reportsConverter.ConvertReports(reports, alertDate, englishContentLanguageId);
    }

    private IQueryable<RawReport> GetReportFilterQuery()
    {
        return _nyssContext.RawReports
            .Include(r => r.Village!)
            .ThenInclude(x => x.District!)
            .ThenInclude(x => x.EidsrOrganisationUnits)

            .Include(r => r.Village!)
            .ThenInclude(x => x.District!)
            .ThenInclude(x => x.Region)

            .Include(r => r.Report!)
            .ThenInclude(x => x.ProjectHealthRisk!)
            .ThenInclude(x => x.HealthRisk!)
            .ThenInclude(x => x.LanguageContents)

            .Include(r => r.Report!)
            .ThenInclude(x => x.ProjectHealthRisk!)
            .ThenInclude(x => x.HealthRisk!)
            .ThenInclude(x => x.HealthRiskSuspectedDiseases)
            .ThenInclude(x => x.SuspectedDisease!)
            .ThenInclude(x => x.LanguageContents)

            .Where(x => x.Report!.Status == ReportStatus.Accepted);
    }

    private int GetEnglishContentLanguageId()
    {
        var englishContentLanguageId = _nyssContext.ContentLanguages
            .FirstOrDefault(x => x.LanguageCode == "en")?.Id;

        if (englishContentLanguageId == null)
        {
            throw new InvalidOperationException("English language is required to properly push reports.");
        }

        return englishContentLanguageId.Value;
    }

    private Alert GetAlert(int alertId)
    {
        var alert = _nyssContext.Alerts
            .Include(x => x.ProjectHealthRisk)
            .ThenInclude(x => x.Project)
            .ThenInclude(x => x.NationalSociety)
            .ThenInclude(x => x.EidsrConfiguration)
            .FirstOrDefault(x => x.Id == alertId);

        if (alert == null)
        {
            throw new InvalidOperationException($"Alert with id {alertId} was not found.");
        }

        return alert;
    }

    private EidsrDbReportTemplate GetReportTemplate(EidsrConfiguration? config)
    {
        if (config == null)
        {
            throw new InvalidOperationException("EidsrConfiguration not set.");
        }

        return new EidsrDbReportTemplate
        {
            EidsrApiProperties = new EidsrApiProperties
            {
                Url = config.ApiBaseUrl ?? string.Empty,
                UserName = config.Username ?? string.Empty,
                PasswordHash = config.PasswordHash ?? string.Empty,
            },
            Program = config.TrackerProgramId ?? string.Empty,
            LocationDataElementId = config.LocationDataElementId ?? string.Empty,
            DateOfOnsetDataElementId = config.DateOfOnsetDataElementId ?? string.Empty,
            PhoneNumberDataElementId = config.PhoneNumberDataElementId ?? string.Empty,
            SuspectedDiseaseDataElementId = config.SuspectedDiseaseDataElementId ?? string.Empty,
            EventTypeDataElementId = config.EventTypeDataElementId ?? string.Empty,
            GenderDataElementId = config.GenderDataElementId ?? string.Empty
        };
    }
}