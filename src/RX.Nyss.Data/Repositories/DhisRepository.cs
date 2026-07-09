using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data.Repositories;

public interface IDhisRepository
{
    DhisDbReport GetReportsForDhis(int reportId);
}

public class DhisRepository : IDhisRepository
{
    private readonly INyssContext _nyssContext;
    private readonly IReportsConverter _reportsConverter;

    public DhisRepository(INyssContext nyssContext, IReportsConverter reportsConverter)
    {
        _nyssContext = nyssContext;
        _reportsConverter = reportsConverter;
    }

    public DhisDbReport GetReportsForDhis(int rawReportId)
    {
        var rawReport = GetRawReport(rawReportId);
        var englishContentLanguageId = GetEnglishContentLanguageId() ?? 1;

        // create template based on report's National Society config
        var dhisDbReportTemplate = GetDhisReportTemplate(rawReport?.NationalSociety?.EidsrConfiguration!);

        // convert NYSS reports to DHIS reports
        var convertedReports = _reportsConverter.ConvertDhisReport(rawReport!, rawReport!.ReceivedAt, englishContentLanguageId);

        var finalReport = new DhisDbReport
        {
            DhisDbReportTemplate = dhisDbReportTemplate,
            DhisDbReportData = convertedReports
        };
        // Pack template and DHIS reports to one, ready to send object
        return finalReport;
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

    private DhisDbReportTemplate GetDhisReportTemplate(EidsrConfiguration config)
    {
        if (config == null)
        {
            throw new Exception("EidsrConfiguration not set.");
        }

        var template = new DhisDbReportTemplate
        {
            EidsrApiProperties = new EidsrApiProperties
            {
                Url = config.ApiBaseUrl,
                UserName = config.Username,
                PasswordHash = config.PasswordHash,
            },
            Program = config.TrackerProgramId,
            ReportLocationDataElementId = config.ReportLocationDataElementId,
            ReportGeoLocationDataElementId = config.ReportGeoLocationDataElementId,
            ReportHealthRiskDataElementId = config.ReportHealthRiskDataElementId,
            ReportSuspectedDiseaseDataElementId = config.ReportSuspectedDiseaseDataElementId,
            ReportGenderDataElementId = config.ReportGenderDataElementId,
            ReportAgeGroupDataElementId = config.ReportAgeGroupDataElementId,
            ReportCaseCountFemaleAgeAtLeastFiveDataElementId = config.ReportCaseCountFemaleAgeAtLeastFiveDataElementId,
            ReportCaseCountMaleAgeAtLeastFiveDataElementId = config.ReportCaseCountMaleAgeAtLeastFiveDataElementId,
            ReportCaseCountFemaleAgeBelowFiveDataElementId = config.ReportCaseCountFemaleAgeBelowFiveDataElementId,
            ReportCaseCountMaleAgeBelowFiveDataElementId = config.ReportCaseCountMaleAgeBelowFiveDataElementId,
            ReportDateDataElementId = config.ReportDateDataElementId,
            ReportTimeDataElementId = config.ReportTimeDataElementId,
            ReportDataCollectorIdDataElementId = config.ReportDataCollectorIdDataElementId,
            ReportStatusDataElementId = config.ReportStatusDataElementId
        };

        return template;
    }

    private RawReport GetRawReport(int rawReportId)
    {
        var query = _nyssContext.RawReports
            // Access OrganizationUnits of Report
            .Include(r => r.Village!)
            .ThenInclude(x => x.District!)
            .ThenInclude(x => x.EidsrOrganisationUnits)

            // Access Region and District of Report
            .Include(r => r.Village!)
            .ThenInclude(x => x.District!)
            .ThenInclude(x => x.Region)

            // Access LanguageContents of SuspectedDisease and Health Risk of Report
            .Include(r => r.Report!)
            .ThenInclude(x => x.ProjectHealthRisk!)
            .ThenInclude(x => x.HealthRisk)
            .ThenInclude(x => x.HealthRiskSuspectedDiseases)
            .ThenInclude(x => x.SuspectedDisease)
            .ThenInclude(x => x.LanguageContents)

            .Include(r => r.Report!)
            .ThenInclude(x => x.ProjectHealthRisk!)
            .ThenInclude(x => x.HealthRisk)
            .ThenInclude(x => x.LanguageContents)

            .Include(r => r.NationalSociety!)
            .ThenInclude(n => n.EidsrConfiguration)

            .Include(r => r.Report!)
            .ThenInclude(c => c.ReportedCase)

            .Include(r => r.Report!)
            .ThenInclude(d => d.DataCollector)

            .FirstOrDefault(x => x.Id == rawReportId);

        return query ?? throw new Exception($"Report with id {rawReportId} not found.");
    }

}