using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Features.Users;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Reports
{
    public interface IReportExportService
    {
        Task<byte[]> Export(int projectId, ReportListFilterRequestDto filter, bool useExcelFormat = false);
    }

    public class ReportExportService : IReportExportService
    {
        private readonly INyssContext _nyssContext;
        private readonly IExcelExportService _excelExportService;
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserService _userService;

        public ReportExportService(
            INyssContext nyssContext,
            IExcelExportService excelExportService,
            IStringsResourcesService stringsResourcesService,
            IAuthorizationService authorizationService,
            IUserService userService)
        {
            _nyssContext = nyssContext;
            _excelExportService = excelExportService;
            _stringsResourcesService = stringsResourcesService;
            _authorizationService = authorizationService;
            _userService = userService;
        }

        public async Task<byte[]> Export(int projectId, ReportListFilterRequestDto filter, bool useExcelFormat = false)
        {
            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(_authorizationService.GetCurrentUserName());
            var stringResources = (await _stringsResourcesService.GetStringsResources(userApplicationLanguageCode)).Value;
            var currentRole = (await _authorizationService.GetCurrentUser()).Role;

            var currentUser = await _authorizationService.GetCurrentUser();

            var currentUserOrganization = await _nyssContext
                .Projects
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.NationalSociety.NationalSocietyUsers)
                .Where(uns => uns.User.Id == currentUser.Id)
                .Select(uns => uns.Organization)
                .SingleOrDefaultAsync();

            var reportsQuery = _nyssContext.RawReports
                .FilterByProject(projectId)
                .FilterByHealthRisk(filter.HealthRiskId)
                .FilterByTrainingMode(filter.IsTraining)
                .FilterByDataCollectorType(filter.ReportsType == ReportListType.FromDcp
                    ? DataCollectorType.CollectionPoint
                    : DataCollectorType.Human)
                .FilterByArea(ReportService.MapToArea(filter.Area))
                .Where(r => filter.Status
                    ? r.Report != null && !r.Report.MarkedAsError
                    : r.Report == null || (r.Report != null && r.Report.MarkedAsError))
                .Select(r => new ExportReportListResponseDto
                {
                    Id = r.Id,
                    DateTime = r.ReceivedAt.AddHours(filter.UtcOffset),
                    HealthRiskName = r.Report.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.LanguageCode == userApplicationLanguageCode)
                        .Select(lc => lc.Name)
                        .Single(),
                    IsValid = r.Report != null,
                    IsAnonymized = currentRole == Role.Supervisor || currentRole == Role.HeadSupervisor
                        ? (currentRole == Role.Supervisor && r.DataCollector.Supervisor.Id != currentUser.Id)
                        || (currentRole == Role.HeadSupervisor && r.DataCollector.Supervisor.HeadSupervisor.Id != currentUser.Id)
                        : currentRole != Role.Administrator && !r.NationalSociety.NationalSocietyUsers.Any(
                            nsu => nsu.UserId == r.DataCollector.Supervisor.Id && nsu.OrganizationId == currentUserOrganization.Id),
                    OrganizationName = r.NationalSociety.NationalSocietyUsers
                        .Where(nsu => nsu.UserId == r.DataCollector.Supervisor.Id)
                        .Select(nsu => nsu.Organization.Name)
                        .FirstOrDefault(),
                    SupervisorName = r.DataCollector.Supervisor.Name,
                    Status = GetReportStatus(r.Report != null, r.Report.MarkedAsError, stringResources),
                    MarkedAsError = r.Report.MarkedAsError,
                    Region = r.Village.District.Region.Name,
                    District = r.Village.District.Name,
                    Village = r.Village.Name,
                    Zone = r.Zone.Name,
                    Location = r.Report != null
                        ? r.Report.Location
                        : null,
                    DataCollectorDisplayName = r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint
                        ? r.DataCollector.Name
                        : r.DataCollector.DisplayName,
                    PhoneNumber = r.Sender,
                    Message = r.Text.Trim(),
                    CountMalesBelowFive = r.Report.ReportedCase.CountMalesBelowFive,
                    CountMalesAtLeastFive = r.Report.ReportedCase.CountMalesAtLeastFive,
                    CountFemalesBelowFive = r.Report.ReportedCase.CountFemalesBelowFive,
                    CountFemalesAtLeastFive = r.Report.ReportedCase.CountFemalesAtLeastFive,
                    ReferredCount = r.Report.DataCollectionPointCase.ReferredCount,
                    DeathCount = r.Report.DataCollectionPointCase.DeathCount,
                    FromOtherVillagesCount = r.Report.DataCollectionPointCase.FromOtherVillagesCount,
                    EpiWeek = r.Report.EpiWeek,
                    EpiYear = r.Report.EpiYear,
                    ReportAlertId = r.Report.ReportAlerts
                        .OrderByDescending(ar => ar.AlertId)
                        .Select(ar => ar.AlertId)
                        .FirstOrDefault(),
                    ReportStatus = GetReportStatusString(stringResources, r.Report.Status)
                })
                //ToDo: order base on filter.OrderBy property
                .OrderBy(r => r.DateTime, filter.SortAscending);

            var reports = await reportsQuery.ToListAsync<IReportListResponseDto>();

            ReportService.AnonymizeCrossOrganizationReports(reports, currentUserOrganization?.Name, stringResources);

            return useExcelFormat
                ? GetExcelData(reports, stringResources, filter.ReportsType)
                : GetCsvData(reports, stringResources, filter.ReportsType);
        }

        private byte[] GetCsvData(List<IReportListResponseDto> reports, IDictionary<string, StringResourceValue> stringResources, ReportListType reportListType)
        {
            var columnLabels = GetColumnLabels(stringResources, reportListType);

            if (reportListType == ReportListType.FromDcp)
            {
                var dcpReportData = reports.Select(r =>
                {
                    var report = (ExportReportListResponseDto)r;
                    return new ExportDcpReportListCsvContentDto
                    {
                        Date = r.DateTime.ToString("yyyy-MM-dd"),
                        Time = report.DateTime.ToString("HH:mm"),
                        EpiWeek = report.EpiYear,
                        EpiYear = report.EpiWeek,
                        Status = report.Status,
                        Region = report.Region,
                        District = report.District,
                        Village = report.Village,
                        Zone = report.Zone,
                        HealthRiskName = report.HealthRiskName,
                        CountMalesBelowFive = report.CountMalesBelowFive,
                        CountMalesAtLeastFive = report.CountMalesAtLeastFive,
                        CountFemalesBelowFive = report.CountFemalesBelowFive,
                        CountFemalesAtLeastFive = report.CountFemalesAtLeastFive,
                        TotalBelowFive = report.CountFemalesBelowFive + report.CountMalesBelowFive,
                        TotalAtLeastFive = report.CountMalesAtLeastFive + report.CountFemalesAtLeastFive,
                        TotalMale = report.CountMalesAtLeastFive + report.CountMalesBelowFive,
                        TotalFemale = report.CountFemalesAtLeastFive + report.CountFemalesBelowFive,
                        Total = report.CountMalesBelowFive + report.CountMalesAtLeastFive + report.CountFemalesBelowFive + report.CountFemalesAtLeastFive,
                        ReferredCount = report.ReferredCount,
                        DeathCount = report.DeathCount,
                        FromOtherVillagesCount = report.FromOtherVillagesCount,
                        DataCollectorDisplayName = report.DataCollectorDisplayName,
                        PhoneNumber = report.PhoneNumber,
                        Message = report.Message,
                        Location = report.Location != null
                            ? $"{report.Location.Y}/{report.Location.X}"
                            : ""
                    };
                });
                return _excelExportService.ToCsv(dcpReportData, columnLabels);
            }

            var reportData = reports.Select(r =>
            {
                var report = (ExportReportListResponseDto)r;
                return new ExportReportListCsvContentDto
                {
                    Date = r.DateTime.ToString("yyyy-MM-dd"),
                    Time = report.DateTime.ToString("HH:mm"),
                    EpiWeek = report.EpiYear,
                    EpiYear = report.EpiWeek,
                    Status = report.Status,
                    Region = report.Region,
                    District = report.District,
                    Village = report.Village,
                    Zone = report.Zone,
                    HealthRiskName = report.HealthRiskName,
                    CountMalesBelowFive = report.CountMalesBelowFive,
                    CountMalesAtLeastFive = report.CountMalesAtLeastFive,
                    CountFemalesBelowFive = report.CountFemalesBelowFive,
                    CountFemalesAtLeastFive = report.CountFemalesAtLeastFive,
                    TotalBelowFive = report.CountFemalesBelowFive + report.CountMalesBelowFive,
                    TotalAtLeastFive = report.CountMalesAtLeastFive + report.CountFemalesAtLeastFive,
                    TotalMale = report.CountMalesAtLeastFive + report.CountMalesBelowFive,
                    TotalFemale = report.CountFemalesAtLeastFive + report.CountFemalesBelowFive,
                    Total = report.CountMalesBelowFive + report.CountMalesAtLeastFive + report.CountFemalesBelowFive + report.CountFemalesAtLeastFive,
                    DataCollectorDisplayName = report.DataCollectorDisplayName,
                    PhoneNumber = report.PhoneNumber,
                    Message = report.Message,
                    ReportStatus = report.ReportStatus,
                    ReportAlertId = report.ReportAlertId,
                    Location = report.Location != null
                        ? $"{report.Location.Y}/{report.Location.X}"
                        : ""
                };
            });
            return _excelExportService.ToCsv(reportData, columnLabels);
        }

        private static string GetReportStatusString(IDictionary<string, StringResourceValue> stringResources, ReportStatus status) =>
            status switch
            {
                ReportStatus.New => GetStringResource(stringResources, ResultKey.Report.Status.New),
                ReportStatus.Pending => GetStringResource(stringResources, ResultKey.Report.Status.Pending),
                ReportStatus.Rejected => GetStringResource(stringResources, ResultKey.Report.Status.Rejected),
                ReportStatus.Accepted => GetStringResource(stringResources, ResultKey.Report.Status.Accepted),
                ReportStatus.Closed => GetStringResource(stringResources, ResultKey.Report.Status.Closed),
                _ => null
            };

        private static string GetStringResource(IDictionary<string, StringResourceValue> stringResources, string key) =>
            stringResources.Keys.Contains(key)
                ? stringResources[key]
                    .Value
                : key;

        private byte[] GetExcelData(List<IReportListResponseDto> reports, IDictionary<string, StringResourceValue> stringResources, ReportListType reportListType)
        {
            var documentTitle = GetStringResource(stringResources, "reports.export.title");
            var columnLabels = GetColumnLabels(stringResources, reportListType);
            var excelDoc = _excelExportService.ToExcel(reports, columnLabels, documentTitle, reportListType);
            return excelDoc.GetAsByteArray();
        }

        private static string GetReportStatus(bool isValid, bool markedAsError, IDictionary<string, StringResourceValue> stringResources) =>
            markedAsError switch
            {
                true => GetStringResource(stringResources, "reports.list.markedAsError"),
                false => isValid
                    ? GetStringResource(stringResources, "reports.list.success")
                    : GetStringResource(stringResources, "reports.list.error")
            };

        private List<string> GetColumnLabels(IDictionary<string, StringResourceValue> stringResources, ReportListType reportListType)
        {
            if (reportListType == ReportListType.FromDcp)
            {
                return new List<string>
                {
                    GetStringResource(stringResources, "reports.export.date"),
                    GetStringResource(stringResources, "reports.export.time"),
                    GetStringResource(stringResources, "reports.export.epiYear"),
                    GetStringResource(stringResources, "reports.export.epiWeek"),
                    GetStringResource(stringResources, "reports.list.status"),
                    GetStringResource(stringResources, "reports.list.region"),
                    GetStringResource(stringResources, "reports.list.district"),
                    GetStringResource(stringResources, "reports.list.village"),
                    GetStringResource(stringResources, "reports.list.zone"),
                    GetStringResource(stringResources, "reports.list.healthRisk"),
                    GetStringResource(stringResources, "reports.list.malesBelowFive"),
                    GetStringResource(stringResources, "reports.list.malesAtLeastFive"),
                    GetStringResource(stringResources, "reports.list.femalesBelowFive"),
                    GetStringResource(stringResources, "reports.list.femalesAtLeastFive"),
                    GetStringResource(stringResources, "reports.export.totalBelowFive"),
                    GetStringResource(stringResources, "reports.export.totalAtLeastFive"),
                    GetStringResource(stringResources, "reports.export.totalMale"),
                    GetStringResource(stringResources, "reports.export.totalFemale"),
                    GetStringResource(stringResources, "reports.export.total"),
                    GetStringResource(stringResources, "reports.export.referredCount"),
                    GetStringResource(stringResources, "reports.export.deathCount"),
                    GetStringResource(stringResources, "reports.export.fromOtherVillagesCount"),
                    GetStringResource(stringResources, "reports.list.dataCollectorDisplayName"),
                    GetStringResource(stringResources, "reports.list.dataCollectorPhoneNumber"),
                    GetStringResource(stringResources, "reports.export.message"),
                    GetStringResource(stringResources, "reports.export.location")
                };
            }

            return new List<string>
            {
                GetStringResource(stringResources, "reports.export.date"),
                GetStringResource(stringResources, "reports.export.time"),
                GetStringResource(stringResources, "reports.export.epiYear"),
                GetStringResource(stringResources, "reports.export.epiWeek"),
                GetStringResource(stringResources, "reports.list.status"),
                GetStringResource(stringResources, "reports.list.region"),
                GetStringResource(stringResources, "reports.list.district"),
                GetStringResource(stringResources, "reports.list.village"),
                GetStringResource(stringResources, "reports.list.zone"),
                GetStringResource(stringResources, "reports.list.healthRisk"),
                GetStringResource(stringResources, "reports.list.malesBelowFive"),
                GetStringResource(stringResources, "reports.list.malesAtLeastFive"),
                GetStringResource(stringResources, "reports.list.femalesBelowFive"),
                GetStringResource(stringResources, "reports.list.femalesAtLeastFive"),
                GetStringResource(stringResources, "reports.export.totalBelowFive"),
                GetStringResource(stringResources, "reports.export.totalAtLeastFive"),
                GetStringResource(stringResources, "reports.export.totalMale"),
                GetStringResource(stringResources, "reports.export.totalFemale"),
                GetStringResource(stringResources, "reports.export.total"),
                GetStringResource(stringResources, "reports.list.dataCollectorDisplayName"),
                GetStringResource(stringResources, "reports.list.dataCollectorPhoneNumber"),
                GetStringResource(stringResources, "reports.export.message"),
                GetStringResource(stringResources, "reports.export.reportStatus"),
                GetStringResource(stringResources, "reports.export.reportAlertId"),
                GetStringResource(stringResources, "reports.export.location")
            };
        }
    }
}
