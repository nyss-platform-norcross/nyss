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
using RX.Nyss.Web.Features.Common.Dto;
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
                .FilterByProject(projectId, filter.DataCollectorType == ReportListDataCollectorType.UnknownSender)
                .FilterByHealthRisk(filter.HealthRiskId)
                .FilterByReportType(filter.ReportType)
                .FilterByDataCollectorType(filter.DataCollectorType)
                .FilterByReportStatus(filter.ReportStatus)
                .FilterByArea(ReportService.MapToArea(filter.Area))
                .FilterByFormatCorrectness(filter.FormatCorrect)
                .FilterByErrorType(filter.ErrorType)
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
                    Status = GetReportStatusString(stringResources, r.Report.Status),
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
                    ErrorType = GetReportErrorTypeString(stringResources, r.ErrorType),
                    Corrected = r.Report.CorrectedAt.HasValue
                })
                //ToDo: order base on filter.OrderBy property
                .OrderBy(r => r.DateTime, filter.SortAscending);

            var reports = await reportsQuery.ToListAsync<IReportListResponseDto>();

            ReportService.AnonymizeCrossOrganizationReports(reports, currentUserOrganization?.Name, stringResources);

            if (!filter.FormatCorrect)
            {
                return useExcelFormat
                    ? GetIncorrectReportsExcelData(reports, stringResources)
                    : GetIncorrectReportsCsvData(reports, stringResources);
            }

            return useExcelFormat
                ? GetCorrectReportsExcelData(reports, stringResources, filter.DataCollectorType)
                : GetCorrectReportsCsvData(reports, stringResources, filter.DataCollectorType);
        }

        private byte[] GetCorrectReportsCsvData(List<IReportListResponseDto> reports, IDictionary<string, StringResourceValue> stringResources, ReportListDataCollectorType reportListDataCollectorType)
        {
            var columnLabels = GetCorrectReportsColumnLabels(stringResources, reportListDataCollectorType);

            if (reportListDataCollectorType == ReportListDataCollectorType.CollectionPoint)
            {
                var dcpReportData = reports.Select(r =>
                {
                    var report = (ExportReportListResponseDto)r;
                    return new ExportCorrectDcpReportListCsvContentDto
                    {
                        Id = r.Id,
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
                            : "",
                        Corrected = report.Corrected
                        ? GetStringResource(stringResources, "report.export.corrected")
                        : null
                    };
                });
                return _excelExportService.ToCsv(dcpReportData, columnLabels);
            }

            var reportData = reports.Select(r =>
            {
                var report = (ExportReportListResponseDto)r;
                return new ExportCorrectReportListCsvContentDto
                {
                    Id = r.Id,
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
                    ReportAlertId = report.ReportAlertId,
                    Location = report.Location != null
                        ? $"{report.Location.Y}/{report.Location.X}"
                        : "",
                    Corrected = report.Corrected
                        ? GetStringResource(stringResources, "reports.export.corrected")
                        : null
                };
            });
            return _excelExportService.ToCsv(reportData, columnLabels);
        }

        private byte[] GetIncorrectReportsCsvData(List<IReportListResponseDto> reports, IDictionary<string, StringResourceValue> stringResources)
        {
            var columnLabels = GetIncorrectReportsColumnLabels(stringResources);

            var reportData = reports.Select(r =>
            {
                var report = (ExportReportListResponseDto)r;
                return new ExportIncorrectReportListCsvContentDto
                {
                    Id = r.Id,
                    Date = r.DateTime.ToString("yyyy-MM-dd"),
                    Time = report.DateTime.ToString("HH:mm"),
                    EpiWeek = report.EpiYear,
                    EpiYear = report.EpiWeek,
                    Region = report.Region,
                    District = report.District,
                    Village = report.Village,
                    Zone = report.Zone,
                    DataCollectorDisplayName = report.DataCollectorDisplayName,
                    PhoneNumber = report.PhoneNumber,
                    Message = report.Message,
                    Location = report.Location != null
                        ? $"{report.Location.Y}/{report.Location.X}"
                        : "",
                    ErrorType = report.ErrorType,
                    Corrected = report.Corrected
                        ? GetStringResource(stringResources, "report.export.corrected")
                        : null
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

        private static string GetReportErrorTypeString(IDictionary<string, StringResourceValue> stringResources, ReportErrorType? errorType) =>
            errorType switch
            {
                ReportErrorType.HealthRiskNotFound => GetStringResource(stringResources, ResultKey.Report.ErrorType.HealthRiskNotFound),
                ReportErrorType.GlobalHealthRiskCodeNotFound => GetStringResource(stringResources, ResultKey.Report.ErrorType.GlobalHealthRiskCodeNotFound),
                ReportErrorType.FormatError => GetStringResource(stringResources, ResultKey.Report.ErrorType.FormatError),
                ReportErrorType.EventReportHumanHealthRisk => GetStringResource(stringResources, ResultKey.Report.ErrorType.EventReportHumanHealthRisk),
                ReportErrorType.AggregateReportNonHumanHealthRisk => GetStringResource(stringResources, ResultKey.Report.ErrorType.AggregateReportNonHumanHealthRisk),
                ReportErrorType.CollectionPointNonHumanHealthRisk => GetStringResource(stringResources, ResultKey.Report.ErrorType.CollectionPointNonHumanHealthRisk),
                ReportErrorType.CollectionPointUsedDataCollectorFormat => GetStringResource(stringResources, ResultKey.Report.ErrorType.CollectionPointUsedDataCollectorFormat),
                ReportErrorType.DataCollectorUsedCollectionPointFormat => GetStringResource(stringResources, ResultKey.Report.ErrorType.DataCollectorUsedCollectionPointFormat),
                ReportErrorType.SingleReportNonHumanHealthRisk => GetStringResource(stringResources, ResultKey.Report.ErrorType.SingleReportNonHumanHealthRisk),
                ReportErrorType.GenderAndAgeNonHumanHealthRisk => GetStringResource(stringResources, ResultKey.Report.ErrorType.GenderAndAgeNonHumanHealthRisk),
                ReportErrorType.TooLong => GetStringResource(stringResources, ResultKey.Report.ErrorType.TooLong),
                ReportErrorType.Gateway => GetStringResource(stringResources, ResultKey.Report.ErrorType.Gateway),
                ReportErrorType.Other => GetStringResource(stringResources, ResultKey.Report.ErrorType.Other),
                _ => null
            };

        private static string GetStringResource(IDictionary<string, StringResourceValue> stringResources, string key) =>
            stringResources.Keys.Contains(key)
                ? stringResources[key]
                    .Value
                : key;

        private byte[] GetCorrectReportsExcelData(List<IReportListResponseDto> reports, IDictionary<string, StringResourceValue> stringResources,
            ReportListDataCollectorType reportListDataCollectorType)
        {
            var documentTitle = GetStringResource(stringResources, "reports.export.title");
            var columnLabels = GetCorrectReportsColumnLabels(stringResources, reportListDataCollectorType);
            var excelDoc = _excelExportService.CorrectReportsToExcel(reports, columnLabels, documentTitle, reportListDataCollectorType);
            return excelDoc.GetAsByteArray();
        }

        private byte[] GetIncorrectReportsExcelData(List<IReportListResponseDto> reports, IDictionary<string, StringResourceValue> stringResources)
        {
            var documentTitle = GetStringResource(stringResources, "reports.export.title");
            var columnLabels = GetIncorrectReportsColumnLabels(stringResources);
            var excelDoc = _excelExportService.IncorrectReportsToExcel(reports, columnLabels, documentTitle);
            return excelDoc.GetAsByteArray();
        }

        private List<string> GetCorrectReportsColumnLabels(IDictionary<string, StringResourceValue> stringResources, ReportListDataCollectorType reportListDataCollectorType)
        {
            if (reportListDataCollectorType == ReportListDataCollectorType.CollectionPoint)
            {
                return new List<string>
                {
                    GetStringResource(stringResources, "reports.export.id"),
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
                    GetStringResource(stringResources, "reports.export.location"),
                    GetStringResource(stringResources, "reports.export.corrected")
                };
            }

            return new List<string>
            {
                GetStringResource(stringResources, "reports.export.id"),
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
                GetStringResource(stringResources, "reports.export.reportAlertId"),
                GetStringResource(stringResources, "reports.export.location"),
                GetStringResource(stringResources, "reports.export.corrected")
            };
        }

        private List<string> GetIncorrectReportsColumnLabels(IDictionary<string, StringResourceValue> stringResources) =>
            new List<string>
            {
                GetStringResource(stringResources, "reports.export.id"),
                GetStringResource(stringResources, "reports.export.date"),
                GetStringResource(stringResources, "reports.export.time"),
                GetStringResource(stringResources, "reports.export.epiYear"),
                GetStringResource(stringResources, "reports.export.epiWeek"),
                GetStringResource(stringResources, "reports.export.message"),
                GetStringResource(stringResources, "reports.list.errorType"),
                GetStringResource(stringResources, "reports.list.region"),
                GetStringResource(stringResources, "reports.list.district"),
                GetStringResource(stringResources, "reports.list.village"),
                GetStringResource(stringResources, "reports.list.zone"),
                GetStringResource(stringResources, "reports.list.dataCollectorDisplayName"),
                GetStringResource(stringResources, "reports.list.dataCollectorPhoneNumber"),
                GetStringResource(stringResources, "reports.export.location"),
                GetStringResource(stringResources, "reports.export.corrected")
            };
    }
}
