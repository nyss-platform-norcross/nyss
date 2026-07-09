using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Features.Users;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Reports
{
    public interface IReportExportService
    {
        Task<IReadOnlyList<ExportReportListResponseDto>> FetchData(int projectId, ReportListFilterRequestDto filter);
    }

    public class ReportExportService : IReportExportService
    {
        /// <summary>Stay under SQL Server's ~2100 parameter limit for IN clauses.</summary>
        private const int ExportIdBatchSize = 500;

        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserService _userService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IStringsService _stringsService;

        public ReportExportService(
            INyssContext nyssContext,
            IAuthorizationService authorizationService,
            IUserService userService,
            IDateTimeProvider dateTimeProvider,
            IStringsService stringsService) 
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
            _userService = userService;
            _dateTimeProvider = dateTimeProvider;
            _stringsService = stringsService; 
        }

        public async Task<IReadOnlyList<ExportReportListResponseDto>> FetchData(int projectId, ReportListFilterRequestDto filter)
        {
            var currentUser = await _authorizationService.GetCurrentUser();
            if (currentUser == null)
                return Array.Empty<ExportReportListResponseDto>();

            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(
                _authorizationService.GetCurrentUserName() ?? string.Empty);

            var currentUserOrganization = await _nyssContext.UserNationalSocieties
                .Where(nsu => nsu.UserId == currentUser.Id
                    && _nyssContext.Projects
                        .Where(p => p.Id == projectId)
                        .Select(p => p.NationalSocietyId)
                        .Contains(nsu.NationalSocietyId))
                .Select(nsu => nsu.Organization)
                .SingleOrDefaultAsync();

            var currentUserOrganizationId = currentUserOrganization?.Id;

            var epiWeekStartDay = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => p.NationalSociety.EpiWeekStartDay)
                .SingleAsync();

            var strings = await _stringsService.GetForCurrentUser();
            if (strings == null)
                return Array.Empty<ExportReportListResponseDto>();

            var baseQuery = BuildRawReportsBaseQuery(filter, projectId, currentUser);

            var orderedQuery = filter.SortAscending
                ? baseQuery.OrderBy(r => r.ReceivedAt)
                : baseQuery.OrderByDescending(r => r.ReceivedAt);

            // Phase 1: all matching ids with sort only (no heavy joins).
            var candidateIds = await orderedQuery
                .AsNoTracking()
                .Select(r => r.Id)
                .ToListAsync();

            if (candidateIds.Count == 0)
                return Array.Empty<ExportReportListResponseDto>();

            // Phase 2: load export projection only for those ids (batched for SQL parameter limits).
            var rawReportsById = new Dictionary<int, RawReportExportRow>(candidateIds.Count);
            foreach (var idBatch in candidateIds.Chunk(ExportIdBatchSize))
            {
                var batchIds = idBatch.ToList();
                var batchRows = await SelectExportProjection(
                        _nyssContext.RawReports.AsNoTracking().Where(r => batchIds.Contains(r.Id)),
                        userApplicationLanguageCode,
                        currentUserOrganizationId)
                    .ToListAsync();

                foreach (var row in batchRows)
                    rawReportsById[row.Id] = row;
            }

            var currentRole = currentUser.Role;

            var reports = candidateIds
                .Where(id => rawReportsById.ContainsKey(id))
                .Select(id => rawReportsById[id])
                .Select(r =>
                {
                    var isAnonymized = currentRole switch
                    {
                        Role.Supervisor => r.DataCollectorSupervisorId.HasValue
                            && r.DataCollectorSupervisorId != currentUser.Id,
                        Role.HeadSupervisor => r.DataCollectorHeadSupervisorId.HasValue
                            && r.DataCollectorHeadSupervisorId != currentUser.Id,
                        Role.Administrator => false,
                        _ => currentUserOrganizationId.HasValue
                            && r.DataCollectorSupervisorId.HasValue
                            && r.IsAnonymizedByOrg
                    };

                    return new ExportReportListResponseDto
                    {
                        Id = r.Id,
                        DateTime = r.ReceivedAt.AddHours(filter.UtcOffset),
                        HealthRiskName = r.HealthRiskName,
                        IsValid = r.ReportId.HasValue,
                        IsAnonymized = isAnonymized,
                        OrganizationName = r.OrganizationName,
                        SupervisorName = r.DataCollectorSupervisorName,
                        Status = r.ReportId.HasValue && !r.ReportIsActivityReport && r.ReportStatus.HasValue
                            ? r.ReportStatus.Value.ToString()
                            : string.Empty,
                        Region = r.RegionName ?? string.Empty,
                        District = r.DistrictName ?? string.Empty,
                        Village = r.VillageName ?? string.Empty,
                        Zone = r.ZoneName,
                        Location = r.ReportLocation,
                        DataCollectorDisplayName = r.DataCollectorType switch
                        {
                            DataCollectorType.CollectionPoint => r.DataCollectorName ?? string.Empty,
                            DataCollectorType.Human => r.DataCollectorDisplayName ?? string.Empty,
                            _ => string.Empty
                        },
                        CountMalesBelowFive = r.CountMalesBelowFive,
                        CountMalesAtLeastFive = r.CountMalesAtLeastFive,
                        CountFemalesBelowFive = r.CountFemalesBelowFive,
                        CountFemalesAtLeastFive = r.CountFemalesAtLeastFive,
                        ReferredCount = r.ReferredCount,
                        EpiWeek = r.ReportEpiWeek ?? _dateTimeProvider.GetEpiWeek(r.ReceivedAt, epiWeekStartDay),
                        EpiYear = r.ReportEpiYear ?? _dateTimeProvider.GetEpiDate(r.ReceivedAt, epiWeekStartDay).EpiYear,
                        ReportAlertId = r.ReportAlertId,
                        ErrorType = r.ErrorType,
                        Message = r.Text,
                    };
                })
                .ToList();

            if (filter?.DataCollectorType != ReportListDataCollectorType.UnknownSender
                && currentUserOrganization?.Name != null)
            {
                ReportService.AnonymizeCrossOrganizationReports(
                    reports.Cast<IReportListResponseDto>().ToList(),
                    currentUserOrganization.Name,
                    strings);
            }

            return reports;
        }

        private static IQueryable<RawReportExportRow> SelectExportProjection(
            IQueryable<RawReport> query,
            string userApplicationLanguageCode,
            int? currentUserOrganizationId) =>
            query.Select(r => new RawReportExportRow
            {
                Id = r.Id,
                ReceivedAt = r.ReceivedAt,
                Text = r.Text,
                ErrorType = r.ErrorType,
                VillageName = r.Village!.Name,
                DistrictName = r.Village!.District!.Name,
                RegionName = r.Village!.District!.Region!.Name,
                ZoneName = r.Zone!.Name,
                DataCollectorType = (DataCollectorType?)r.DataCollector!.DataCollectorType,
                DataCollectorName = r.DataCollector!.Name,
                DataCollectorDisplayName = r.DataCollector!.DisplayName,
                DataCollectorSupervisorId = (int?)r.DataCollector!.Supervisor!.Id,
                DataCollectorSupervisorName = r.DataCollector!.Supervisor!.Name,
                DataCollectorHeadSupervisorId = (int?)r.DataCollector!.Supervisor!.HeadSupervisor!.Id,
                OrganizationName = r.NationalSociety!.NationalSocietyUsers != null
                    ? r.NationalSociety.NationalSocietyUsers
                        .Where(nsu => nsu.UserId == r.DataCollector!.Supervisor!.Id)
                        .Select(nsu => nsu.Organization!.Name)
                        .FirstOrDefault()
                    : null,
                IsAnonymizedByOrg = currentUserOrganizationId != null
                    && !r.NationalSociety!.NationalSocietyUsers!.Any(
                        nsu => nsu.UserId == r.DataCollector!.Supervisor!.Id
                            && nsu.OrganizationId == currentUserOrganizationId),
                ReportId = (int?)r.Report!.Id,
                ReportLocation = r.Report!.Location,
                ReportStatus = (ReportStatus?)r.Report!.Status,
                ReportIsActivityReport = r.Report != null
                    && r.Report.ProjectHealthRisk != null
                    && r.Report.ProjectHealthRisk.HealthRisk != null
                    && (r.Report.ProjectHealthRisk.HealthRisk.HealthRiskCode == 99
                        || r.Report.ProjectHealthRisk.HealthRisk.HealthRiskCode == 98),
                ReportEpiWeek = (int?)r.Report!.EpiWeek,
                ReportEpiYear = (int?)r.Report!.EpiYear,
                CountMalesBelowFive = r.Report!.ReportedCase!.CountMalesBelowFive,
                CountMalesAtLeastFive = r.Report!.ReportedCase!.CountMalesAtLeastFive,
                CountFemalesBelowFive = r.Report!.ReportedCase!.CountFemalesBelowFive,
                CountFemalesAtLeastFive = r.Report!.ReportedCase!.CountFemalesAtLeastFive,
                ReferredCount = r.Report!.DataCollectionPointCase!.ReferredCount,
                ReportAlertId = r.Report!.ReportAlerts
                    .OrderByDescending(ar => ar.AlertId)
                    .Select(ar => (int?)ar.AlertId)
                    .FirstOrDefault(),
                HealthRiskName = r.Report!.ProjectHealthRisk!.HealthRisk!.LanguageContents
                    .Where(lc => lc.ContentLanguage!.LanguageCode == userApplicationLanguageCode)
                    .Select(lc => lc.Name)
                    .FirstOrDefault() ?? string.Empty,
            });

        private IQueryable<RawReport> BuildRawReportsBaseQuery(ReportListFilterRequestDto filter, int projectId, User currentUser)
        {
            var baseQuery = _nyssContext.RawReports
                .AsNoTracking()
                .FilterByHealthRisks(filter.HealthRisks)
                .FilterByErrorType(filter.ErrorType)
                .FilterByArea(filter.Locations)
                .FilterByDate(filter.StartDate, filter.EndDate.AddDays(1))
                .FilterByReportStatus(filter.ReportStatus)
                .FilterByTrainingMode(filter.TrainingStatus)
                .FilterByDataCollectorType(filter.DataCollectorType);

            return filter.DataCollectorType == ReportListDataCollectorType.UnknownSender
                ? baseQuery.Where(r => _nyssContext.Projects
                    .Where(p => p.Id == projectId)
                    .Select(p => p.NationalSocietyId)
                    .Contains(r.NationalSociety!.Id))
                : baseQuery
                    .FilterByProject(projectId)
                    .FilterByCurrentUserSupervisorAssignment(currentUser);
        }

        private sealed class RawReportExportRow
        {
            public int Id { get; init; }
            public DateTime ReceivedAt { get; init; }
            public string? Text { get; init; }
            public ReportErrorType? ErrorType { get; init; }
            public string? VillageName { get; init; }
            public string? DistrictName { get; init; }
            public string? RegionName { get; init; }
            public string? ZoneName { get; init; }
            public DataCollectorType? DataCollectorType { get; init; }
            public string? DataCollectorName { get; init; }
            public string? DataCollectorDisplayName { get; init; }
            public int? DataCollectorSupervisorId { get; init; }
            public string? DataCollectorSupervisorName { get; init; }
            public int? DataCollectorHeadSupervisorId { get; init; }
            public string? OrganizationName { get; init; }
            public bool IsAnonymizedByOrg { get; init; }
            public int? ReportId { get; init; }
            public Point? ReportLocation { get; init; }
            public ReportStatus? ReportStatus { get; init; }
            public bool ReportIsActivityReport { get; init; }
            public int? ReportEpiWeek { get; init; }
            public int? ReportEpiYear { get; init; }
            public int? CountMalesBelowFive { get; init; }
            public int? CountMalesAtLeastFive { get; init; }
            public int? CountFemalesBelowFive { get; init; }
            public int? CountFemalesAtLeastFive { get; init; }
            public int? ReferredCount { get; init; }
            public int? ReportAlertId { get; init; }
            public string HealthRiskName { get; init; } = string.Empty;
        }
    }
}