using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data.Repositories;

public interface IEidsrRepository
{
    List<EidsrDbReport> GetReportsForEidsr(int alertId);
    EidsrDbReport GetReportForEidsr(int reportId);
}

public class EidsrRepository : IEidsrRepository
{
    private readonly INyssContext _nyssContext;

    public EidsrRepository(INyssContext nyssContext)
    {
        _nyssContext = nyssContext;
    }

    public List<EidsrDbReport> GetReportsForEidsr(int alertId)
    {
        var reportsOfAlertIds = _nyssContext.AlertReports
            .Where(x=>x.AlertId == alertId).Select(x => x.ReportId);

        var reports = GetReportFilterQuery()
            .Where(x => reportsOfAlertIds.Contains(x.Id)).ToList();

        return reports.Select(report => new EidsrDbReport
        {
            EidsrDbReportData = GetReportData(report),
            EidsrDbReportTemplate = GetReportTemplate(report)
        }).ToList();
    }

    public EidsrDbReport GetReportForEidsr(int reportId)
    {
        var rawReport = GetReportFilterQuery()
            .FirstOrDefault(x => x.Id == reportId);

        return new EidsrDbReport
        {
            EidsrDbReportData = GetReportData(rawReport),
            EidsrDbReportTemplate = GetReportTemplate(rawReport)
        };
    }

    private IIncludableQueryable<RawReport, Report> GetReportFilterQuery()
    {
        var query = _nyssContext.RawReports
            .Include(r => r.NationalSociety)
            .ThenInclude(x => x.EidsrConfiguration)
            .Include(r => r.Village)
            .ThenInclude(x => x.District)
            .ThenInclude(x => x.EidsrOrganisationUnits)
            .Include(r => r.Report);

        return query;
    }

    private EidsrDbReportTemplate GetReportTemplate(RawReport rawReport)
    {
        var config = rawReport.NationalSociety.EidsrConfiguration;

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

    private EidsrDbReportData GetReportData(RawReport rawReport)
    {
        var report = rawReport.Report;
        var organizationUnit = rawReport.Village.District.EidsrOrganisationUnits;

        // TODO: missing specification - this is the unified way of converting report to Eidsr event, this conversion was not discussed
        var data = new EidsrDbReportData
        {
            OrgUnit = organizationUnit?.OrganisationUnitId,
            EventDate = report.CreatedAt.ToString("yyyy-MM-dd"),
            Location = report.Location.ToString(),
            DateOfOnset = report.ReceivedAt.ToString("u"),
            PhoneNumber = report.PhoneNumber,
            SuspectedDisease = "some disease",
            EventType = "some type",
        };

        if (report.ReportedCase.CountFemalesBelowFive == 1 || report.ReportedCase.CountFemalesAtLeastFive == 1)
        {
            data.Gender = "female";
        }

        if (report.ReportedCase.CountMalesBelowFive == 1 || report.ReportedCase.CountMalesAtLeastFive == 1)
        {
            data.Gender = "male";
        }

        return data;
    }
}
