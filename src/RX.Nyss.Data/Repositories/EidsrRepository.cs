using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data.Repositories;

public interface IEidsrRepository
{
    List<EidsrDbReport> GetReportsForEidsr(int reportId);
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

        // create template based on alert's National Society config
        var eidsrDbReportTemplate = GetReportTemplate(alert?.ProjectHealthRisk?.Project?.NationalSociety?.EidsrConfiguration);

        // convert alert's reports to Eidsr reports
        var squashedReports = GetReports(alertId, alert.EscalatedAt ?? alert.CreatedAt, englishContentLanguageId.Value);

        // Pack template and Eidsr reports to one, ready to send object
        return squashedReports.Select(aggregatedReport => new EidsrDbReport
        {
            EidsrDbReportTemplate = eidsrDbReportTemplate,
            EidsrDbReportData = aggregatedReport
        }).ToList();
    }

    private List<EidsrDbReportData> GetReports(int alertId, DateTime alertDate, int englishContentLanguageId)
    {
        var reportsOfAlertIds = _nyssContext.AlertReports
            .Where(x=>x.AlertId == alertId).Select(x => x.ReportId).ToList();

        var reports = GetReportFilterQuery()
            .Where(queriedReport => reportsOfAlertIds.Contains(queriedReport.Report.Id))
            .ToList();

        return _reportsConverter.ConvertReports(reports, alertDate, englishContentLanguageId);
    }

    private IQueryable<RawReport> GetReportFilterQuery()
    {
        var query = _nyssContext.RawReports

            // Access EidsrOrganisationUnits of Village of Report
            .Include(r => r.Village)
            .ThenInclude(x => x.District)
            .ThenInclude(x => x.EidsrOrganisationUnits)

            // Access Region and District of Village of Report
            .Include(r => r.Village)
            .ThenInclude(x => x.District)
            .ThenInclude(x => x.Region)

            // Access LanguageContents of SuspectedDisease of Health Risk of Report
            .Include(r => r.Report)
            .ThenInclude(x => x.ProjectHealthRisk)
            .ThenInclude(x => x.HealthRisk)
            .ThenInclude(x => x.HealthRiskSuspectedDiseases)
            .ThenInclude(x => x.SuspectedDisease)
            .ThenInclude(x => x.LanguageContents)
            .Where(x => x.Report.Status == ReportStatus.Accepted);

        return query;
    }

    private int? GetEnglishContentLanguageId()
    {
        var englishContentLanguageId = _nyssContext.ContentLanguages.FirstOrDefault(x => x.LanguageCode == "en")?.Id;

        if (englishContentLanguageId == null)
        {
            throw new Exception("English language is required to properly push reports.");
        }

        return englishContentLanguageId;
    }

    private Alert GetAlert(int alertId)
    {
        // Access EidsrConfiguration of National Society of Project of Alert
        // Alerts -> ProjectHealthRisk -> Project -> NationalSociety -> EidsrConfiguration
        var alert = _nyssContext.Alerts
            .Include(x => x.ProjectHealthRisk)
            .ThenInclude(x => x.Project)
            .ThenInclude(x => x.NationalSociety)
            .ThenInclude(x => x.EidsrConfiguration)
            .FirstOrDefault(x => x.Id == alertId);

        return alert;
    }

    private EidsrDbReportTemplate GetReportTemplate(EidsrConfiguration config)
    {
        if (config == null)
        {
            throw new Exception("EidsrConfiguration not set.");
        }

        var template = new EidsrDbReportTemplate
        {
            EidsrApiProperties = new EidsrApiProperties
            {
                Url = config.ApiBaseUrl,
                UserName = config.Username,
                PasswordHash = config.PasswordHash,
            },
            Program = config.TrackerProgramId,
            LocationDataElementId = config.LocationDataElementId,
            DateOfOnsetDataElementId = config.DateOfOnsetDataElementId,
            PhoneNumberDataElementId = config.PhoneNumberDataElementId,
            SuspectedDiseaseDataElementId = config.SuspectedDiseaseDataElementId,
            EventTypeDataElementId = config.EventTypeDataElementId,
            GenderDataElementId = config.GenderDataElementId
        };

        return template;
    }
}
