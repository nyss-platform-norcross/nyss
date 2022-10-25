using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data.Repositories;

public interface IEidsrRepository
{
    List<EidsrReport> GetReportsForEidsr(int alertId);
    EidsrReport GetReportForEidsr(int reportId);
}

public class EidsrRepository : IEidsrRepository
{
    private readonly INyssContext _nyssContext;

    public EidsrRepository(INyssContext nyssContext)
    {
        _nyssContext = nyssContext;
    }

    public List<EidsrReport> GetReportsForEidsr(int alertId)
    {
        var reportsOfAlertIds = _nyssContext.AlertReports
            .Where(x=>x.AlertId == alertId).Select(x => x.ReportId);

        var reports = GetReportFilterQuery()
            .Where(x => reportsOfAlertIds.Contains(x.Id)).ToList();

        return reports.Select(report => new EidsrReport
        {
            EidsrReportData = GetReportData(report),
            EidsrReportTemplate = GetReportTemplate(report)
        }).ToList();
    }

    public EidsrReport GetReportForEidsr(int reportId)
    {
        var rawReport = GetReportFilterQuery()
            .FirstOrDefault(x => x.Id == reportId);

        return new EidsrReport
        {
            EidsrReportData = GetReportData(rawReport),
            EidsrReportTemplate = GetReportTemplate(rawReport)
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

    private EidsrReportTemplate GetReportTemplate(RawReport rawReport)
    {
        var config = rawReport.NationalSociety.EidsrConfiguration;

        var template = new EidsrReportTemplate
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

    private EidsrReportData GetReportData(RawReport rawReport)
    {
        var report = rawReport.Report;
        var organizationUnit = rawReport.Village.District.EidsrOrganisationUnits;

        var data = new EidsrReportData //TODO: verify how to fill that fields
        {
            OrgUnit = organizationUnit?.OrganisationUnitId,
            EventDate = report.CreatedAt,
            Location = report.Location.ToString(),
            DateOfOnset = report.ReceivedAt,
            PhoneNumber = report.PhoneNumber,
            SuspectedDisease = "some disease",
            EventType = "some type",
            Gender = "Male",
        };

        return data;
    }
}
