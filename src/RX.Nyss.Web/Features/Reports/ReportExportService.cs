using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
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
        Task<IReadOnlyCollection<IReportListResponseDto>> FetchData(int projectId, ReportListFilterRequestDto filter);
    }

    public class ReportExportService : IReportExportService
    {
        private readonly INyssContext _nyssContext;

        private readonly IAuthorizationService _authorizationService;

        private readonly IUserService _userService;

        private readonly IStringsService _stringsService;

        private readonly IDateTimeProvider _dateTimeProvider;

        public ReportExportService(
            INyssContext nyssContext,
            IAuthorizationService authorizationService,
            IUserService userService,
            IStringsService stringsService,
            IDateTimeProvider dateTimeProvider)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
            _userService = userService;
            _stringsService = stringsService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<IReadOnlyCollection<IReportListResponseDto>> FetchData(int projectId, ReportListFilterRequestDto filter)
        {
            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(_authorizationService.GetCurrentUserName());
            var strings = await _stringsService.GetForCurrentUser();
            var currentUser = await _authorizationService.GetCurrentUser();
            var currentRole = currentUser.Role;

            var currentUserOrganization = await _nyssContext
                .Projects
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.NationalSociety.NationalSocietyUsers)
                .Where(uns => uns.User.Id == currentUser.Id)
                .Select(uns => uns.Organization)
                .SingleOrDefaultAsync();

            var epiWeekStartDay = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => p.NationalSociety.EpiWeekStartDay)
                .SingleAsync();

            var baseQuery = await BuildRawReportsBaseQuery(filter, projectId);

            var reportsQuery =  baseQuery.Select(r => new ExportReportListResponseDto
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
                    Status = r.Report != null && !r.Report.IsActivityReport()
                        ? GetReportStatusString(strings, r.Report.Status)
                        : null,
                    MarkedAsError = r.Report != null && r.Report.MarkedAsError,
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
                    EpiWeek = r.Report != null ? r.Report.EpiWeek : _dateTimeProvider.GetEpiWeek(r.ReceivedAt, epiWeekStartDay),
                    EpiYear = r.Report != null ? r.Report.EpiYear : _dateTimeProvider.GetEpiDate(r.ReceivedAt, epiWeekStartDay).EpiYear,
                    ReportAlertId = r.Report.ReportAlerts
                        .OrderByDescending(ar => ar.AlertId)
                        .Select(ar => ar.AlertId)
                        .FirstOrDefault(),
                    ErrorType = GetReportErrorTypeString(strings, r.ErrorType),
                    Corrected = r.Report.CorrectedAt.HasValue
                })
                //ToDo: order base on filter.OrderBy property
                .OrderBy(r => r.DateTime, filter.SortAscending);

            var reports = await reportsQuery.ToListAsync<IReportListResponseDto>();

            ReportService.AnonymizeCrossOrganizationReports(reports, currentUserOrganization?.Name, strings);

            return reports;
        }

        private static string GetReportStatusString(StringsResourcesVault strings, ReportStatus status) =>
            status switch
            {
                ReportStatus.New => strings[ResultKey.Report.Status.New],
                ReportStatus.Pending => strings[ResultKey.Report.Status.Pending],
                ReportStatus.Rejected => strings[ResultKey.Report.Status.Rejected],
                ReportStatus.Accepted => strings[ResultKey.Report.Status.Accepted],
                ReportStatus.Closed => strings[ResultKey.Report.Status.Closed],
                _ => null
            };

        private static string GetReportErrorTypeString(StringsResourcesVault strings, ReportErrorType? errorType) =>
            errorType switch
            {
                ReportErrorType.HealthRiskNotFound => strings[ResultKey.Report.ErrorType.HealthRiskNotFound],
                ReportErrorType.GlobalHealthRiskCodeNotFound => strings[ResultKey.Report.ErrorType.GlobalHealthRiskCodeNotFound],
                ReportErrorType.FormatError => strings[ResultKey.Report.ErrorType.FormatError],
                ReportErrorType.EventReportHumanHealthRisk => strings[ResultKey.Report.ErrorType.EventReportHumanHealthRisk],
                ReportErrorType.AggregateReportNonHumanHealthRisk => strings[ResultKey.Report.ErrorType.AggregateReportNonHumanHealthRisk],
                ReportErrorType.CollectionPointNonHumanHealthRisk => strings[ResultKey.Report.ErrorType.CollectionPointNonHumanHealthRisk],
                ReportErrorType.CollectionPointUsedDataCollectorFormat => strings[ResultKey.Report.ErrorType.CollectionPointUsedDataCollectorFormat],
                ReportErrorType.DataCollectorUsedCollectionPointFormat => strings[ResultKey.Report.ErrorType.DataCollectorUsedCollectionPointFormat],
                ReportErrorType.SingleReportNonHumanHealthRisk => strings[ResultKey.Report.ErrorType.SingleReportNonHumanHealthRisk],
                ReportErrorType.GenderAndAgeNonHumanHealthRisk => strings[ResultKey.Report.ErrorType.GenderAndAgeNonHumanHealthRisk],
                ReportErrorType.TooLong => strings[ResultKey.Report.ErrorType.TooLong],
                ReportErrorType.Gateway => strings[ResultKey.Report.ErrorType.Gateway],
                ReportErrorType.Other => strings[ResultKey.Report.ErrorType.Other],
                _ => null
            };

        private async Task<IQueryable<RawReport>> BuildRawReportsBaseQuery(ReportListFilterRequestDto filter, int projectId) {
            if(filter.DataCollectorType == ReportListDataCollectorType.UnknownSender)
            {
                var nationalSocietyId = await _nyssContext.Projects
                    .Where(p => p.Id == projectId)
                    .Select(p => p.NationalSocietyId)
                    .SingleOrDefaultAsync();

                return _nyssContext.RawReports
                    .Include(r => r.Report)
                    .ThenInclude(r => r.ProjectHealthRisk)
                    .ThenInclude(r => r.HealthRisk)
                    .Where(r => r.NationalSociety.Id == nationalSocietyId)
                    .FilterByDataCollectorType(filter.DataCollectorType)
                    .FilterByHealthRisks(filter.HealthRisks)
                    .FilterByFormatCorrectness(filter.FormatCorrect)
                    .FilterByErrorType(filter.ErrorType)
                    .FilterByArea(filter.Locations)
                    .FilterByReportStatus(filter.ReportStatus)
                    .FilterByTrainingMode(filter.TrainingStatus);
            }

            return _nyssContext.RawReports
                .Include(r => r.Report)
                .ThenInclude(r => r.ProjectHealthRisk)
                .ThenInclude(r => r.HealthRisk)
                .FilterByProject(projectId)
                .FilterByHealthRisks(filter.HealthRisks)
                .FilterByDataCollectorType(filter.DataCollectorType)
                .FilterByArea(filter.Locations)
                .FilterByFormatCorrectness(filter.FormatCorrect)
                .FilterByErrorType(filter.ErrorType)
                .FilterByReportStatus(filter.ReportStatus)
                .FilterByTrainingMode(filter.TrainingStatus);
        }
    }
}
