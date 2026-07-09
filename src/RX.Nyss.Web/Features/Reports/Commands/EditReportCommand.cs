using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Alerts;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Reports.Commands;

public class EditReportCommand : IRequest<Result>
{
    public int Id { get; set; }

    public ReportRequestDto Body { get; set; }

    public EditReportCommand(int id, ReportRequestDto dto)
    {
        Id = id;
        Body = dto;
    }

    public class Handler : IRequestHandler<EditReportCommand, Result>
    {
        private readonly INyssContext _nyssContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAlertReportService _alertReportService;

        public Handler(
            INyssContext nyssContext,
            IDateTimeProvider dateTimeProvider,
            IAuthorizationService authorizationService,
            IAlertReportService alertReportService)
        {
            _nyssContext = nyssContext ?? throw new ArgumentNullException(nameof(nyssContext));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _alertReportService = alertReportService ?? throw new ArgumentNullException(nameof(alertReportService));
        }

        public async Task<Result> Handle(EditReportCommand command, CancellationToken cancellationToken)
        {
            var locationChanged = false;

            try
            {
                var rawReport = await _nyssContext.RawReports
                    .Include(raw => raw.Zone)
                    .Include(r => r.Report)
                        .ThenInclude(r => r.ProjectHealthRisk)
                    .Include(r => r.DataCollector)
                        .ThenInclude(phr => phr.Project)
                    .Include(r => r.DataCollector)
                        .ThenInclude(dc => dc.DataCollectorLocations!)
                        .ThenInclude(dcl => dcl.Village)
                    .Include(r => r.DataCollector)
                        .ThenInclude(dc => dc.DataCollectorLocations!)
                        .ThenInclude(dcl => dcl.Zone)
                    .SingleOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

                if (rawReport == null)
                {
                    return Error(ResultKey.Report.ReportNotFound);
                }

                if (rawReport.Report == null)
                {
                    return Error(ResultKey.Report.ReportNotFound);
                }

                if (rawReport.Report.ProjectHealthRisk == null)
                {
                    return Error(ResultKey.Report.Edit.HealthRiskNotAssignedToProject);
                }

                if (rawReport.Report.Status != ReportStatus.New)
                {
                    return Error(ResultKey.Report.Edit.OnlyNewReportsEditable);
                }

                if (rawReport.Report.ProjectHealthRisk.HealthRiskId != command.Body.HealthRiskId)
                {
                    await SetProjectHealthRisk(rawReport.Report, command.Body.HealthRiskId, command.Body.DataCollectorId, cancellationToken);
                }

                if (rawReport.DataCollector == null || rawReport.DataCollector.Id != command.Body.DataCollectorId)
                {
                    await UpdateDataCollectorForReport(rawReport.Report, command.Body.DataCollectorId, cancellationToken);
                }

                var dataCollectorLocations = rawReport.DataCollector?.DataCollectorLocations?.ToList()
                    ?? new List<DataCollectorLocation>();

                if (LocationNeedsUpdate(rawReport.Report, command.Body, dataCollectorLocations))
                {
                    await UpdateLocationForReport(rawReport.Report, command.Body.DataCollectorLocationId, command.Body.DataCollectorId, cancellationToken);
                    locationChanged = true;
                }

                UpdateReportedCaseCountForReport(rawReport.Report, command.Body);

                var updatedReceivedAt = new DateTime(
                    command.Body.Date.Year,
                    command.Body.Date.Month,
                    command.Body.Date.Day,
                    rawReport.ReceivedAt.Hour,
                    rawReport.ReceivedAt.Minute,
                    rawReport.ReceivedAt.Second);

                rawReport.ReceivedAt = updatedReceivedAt;
                rawReport.Report.ReceivedAt = updatedReceivedAt;

                if (rawReport.Report.ReportType != ReportType.DataCollectionPoint &&
                    rawReport.Report.ReportType != ReportType.Aggregate)
                {
                    rawReport.Report.Status = command.Body.ReportStatus;
                }

                rawReport.Report.ModifiedAt = _dateTimeProvider.UtcNow;
                rawReport.Report.ModifiedBy = _authorizationService.GetCurrentUserName();

                await _nyssContext.SaveChangesAsync(cancellationToken);

                var correctionAttempted = command.Body.DataCollectorLocationId > 0;
                if (ShouldRecalculateAlerts(rawReport.Report, locationChanged, correctionAttempted))
                {
                    try
                    {
                        await _alertReportService.RecalculateAlertForReport(rawReport.Id);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Failed to recalculate alerts for report {rawReport.Id}: {ex}");
                    }
                }

                return SuccessMessage(ResultKey.Report.Edit.EditSuccess);
            }
            catch (ResultException e)
            {
                return Error(e.GetBaseException() + ResultKey.Report.Edit.SenderDoesNotExist);
            }
        }

        private async Task SetProjectHealthRisk(Report report, int projectHealthRiskId, int dataCollectorId, CancellationToken cancellationToken)
        {
            var projectId = report.DataCollector?.Project?.Id ?? await _nyssContext.DataCollectors
                .Where(dc => dc.Id == dataCollectorId)
                .Select(dc => dc.Project!.Id)
                .SingleOrDefaultAsync(cancellationToken);

            var projectHealthRisk = await _nyssContext.ProjectHealthRisks
                .Include(phr => phr.HealthRisk)
                .SingleOrDefaultAsync(phr => phr.Id == projectHealthRiskId
                    && phr.Project.Id == projectId, cancellationToken);

            if (projectHealthRisk == null)
            {
                throw new ResultException(ResultKey.Report.Edit.HealthRiskNotAssignedToProject);
            }

            report.ProjectHealthRisk = projectHealthRisk;
        }

        private static bool LocationNeedsUpdate(Report report, ReportRequestDto requestDto, List<DataCollectorLocation> dataCollectorLocations)
        {
            if (report.RawReport == null || report.RawReport.Village == null)
            {
                return true;
            }

            var currentVillageId = report.RawReport.Village.Id;
            var currentZoneId = report.RawReport.Zone?.Id;

            var location = ResolveCurrentLocation(dataCollectorLocations, currentVillageId, currentZoneId);

            return requestDto.DataCollectorLocationId != location?.Id;
        }

        private static DataCollectorLocation ResolveCurrentLocation(
            IEnumerable<DataCollectorLocation> dataCollectorLocations,
            int currentVillageId,
            int? currentZoneId)
        {
            if (currentZoneId.HasValue)
            {
                return dataCollectorLocations
                    .Where(dcl => dcl.Village?.Id == currentVillageId && dcl.Zone?.Id == currentZoneId.Value)
                    .OrderBy(dcl => dcl.Id)
                    .FirstOrDefault();
            }

            return dataCollectorLocations
                .Where(dcl => dcl.Village?.Id == currentVillageId && dcl.Zone == null)
                .OrderBy(dcl => dcl.Id)
                .FirstOrDefault();
        }

        private async Task UpdateDataCollectorForReport(Report report, int dataCollectorId, CancellationToken cancellationToken)
        {
            var newDataCollector = await _nyssContext.DataCollectors
                .Include(dc => dc.DataCollectorLocations!).ThenInclude(dcl => dcl.Village)
                .Include(dc => dc.DataCollectorLocations!).ThenInclude(dcl => dcl.Zone)
                .SingleOrDefaultAsync(dc => dc.Id == dataCollectorId, cancellationToken);

            if (newDataCollector == null)
            {
                throw new ResultException(ResultKey.Report.Edit.SenderDoesNotExist);
            }

            if ((newDataCollector.DataCollectorType == DataCollectorType.CollectionPoint && report.ReportType != ReportType.DataCollectionPoint)
                || (newDataCollector.DataCollectorType == DataCollectorType.Human && report.ReportType == ReportType.DataCollectionPoint))
            {
                throw new ResultException(ResultKey.Report.Edit.DataCollectorTypeCannotBeChanged);
            }

            report.DataCollector = newDataCollector;
            report.RawReport.DataCollector = newDataCollector;
        }

        private async Task UpdateLocationForReport(Report report, int locationId, int dataCollectorId, CancellationToken cancellationToken)
        {
            var location = await _nyssContext.DataCollectorLocations
                .Include(dcl => dcl.Village)
                .Include(dcl => dcl.Zone)
                .Where(dcl => dcl.DataCollectorId == dataCollectorId && dcl.Id == locationId)
                .SingleOrDefaultAsync(cancellationToken);

            if (location == null)
            {
                throw new ResultException(ResultKey.Report.Edit.LocationNotFound);
            }

            if (location.Location == null)
            {
                Console.Error.WriteLine(
                    $"Selected location {location.Id} for data collector {dataCollectorId} has no coordinates. Report {report.Id} cannot be corrected to this location.");
                throw new ResultException(ResultKey.Report.Edit.LocationNotFound);
            }

            report.Location = location.Location;
            report.RawReport.Village = location.Village;
            report.RawReport.Zone = location.Zone;
        }

        private static bool ShouldRecalculateAlerts(Report report, bool locationChanged, bool correctionAttempted)
        {
            if (report.Location == null)
            {
                return false;
            }

            if (!StatusConstants.ReportStatusesConsideredForAlertProcessing.Contains(report.Status))
            {
                return false;
            }

            return locationChanged || correctionAttempted;
        }

        private void UpdateReportedCaseCountForReport(Report report, ReportRequestDto requestDto)
        {
            if (report.ReportType != ReportType.Event)
            {
                report.ReportedCase.CountMalesBelowFive = requestDto.CountMalesBelowFive;
                report.ReportedCase.CountMalesAtLeastFive = requestDto.CountMalesAtLeastFive;
                report.ReportedCase.CountFemalesBelowFive = requestDto.CountFemalesBelowFive;
                report.ReportedCase.CountFemalesAtLeastFive = requestDto.CountFemalesAtLeastFive;
                report.ReportedCase.CountUnspecifiedSexAndAge = requestDto.CountUnspecifiedSexAndAge;
            }

            report.ReportedCaseCount = CalculateReportedCaseCount(report.ReportType, requestDto);

            if (report.ReportType != ReportType.DataCollectionPoint)
            {
                return;
            }

            if (report.DataCollectionPointCase == null)
            {
                throw new ResultException(ResultKey.Report.Edit.DataCollectionPointCaseNull);
            }

            report.DataCollectionPointCase.ReferredCount = requestDto.ReferredCount;
        }

        private static int CalculateReportedCaseCount(ReportType reportType, ReportRequestDto requestDto)
        {
            if (reportType == ReportType.Event)
            {
                return 1;
            }

            return (requestDto.CountMalesBelowFive ?? 0) +
                (requestDto.CountMalesAtLeastFive ?? 0) +
                (requestDto.CountFemalesBelowFive ?? 0) +
                (requestDto.CountFemalesAtLeastFive ?? 0) +
                (requestDto.CountUnspecifiedSexAndAge ?? 0);
        }
    }
}