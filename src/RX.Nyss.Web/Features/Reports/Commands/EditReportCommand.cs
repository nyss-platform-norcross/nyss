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
            _nyssContext = nyssContext;
            _dateTimeProvider = dateTimeProvider;
            _authorizationService = authorizationService;
            _alertReportService = alertReportService;
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
                    .Include(r => r.DataCollector).ThenInclude(phr => phr.Project)
                    .Include(r => r.DataCollector).ThenInclude(dc => dc.DataCollectorLocations).ThenInclude(dcl => dcl.Village)
                    .Include(r => r.DataCollector).ThenInclude(dc => dc.DataCollectorLocations).ThenInclude(dcl => dcl.Zone)
                    .SingleOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

                if (rawReport?.Report == null)
                {
                    return Error<ReportResponseDto>(ResultKey.Report.ReportNotFound);
                }

                if (rawReport.Report.Status != ReportStatus.New)
                {
                    return Error<ReportResponseDto>(ResultKey.Report.Edit.OnlyNewReportsEditable);
                }

                if (rawReport.Report.ProjectHealthRisk.Id != command.Body.HealthRiskId)
                {
                    await SetProjectHealthRisk(rawReport.Report, command.Body.HealthRiskId, command.Body.DataCollectorId);
                }

                if (rawReport.DataCollector == null || rawReport.DataCollector.Id != command.Body.DataCollectorId)
                {
                    await UpdateDataCollectorForReport(rawReport.Report, command.Body.DataCollectorId);
                }

                var dataCollectorLocations = rawReport.DataCollector != null
                    ? rawReport.DataCollector.DataCollectorLocations.ToList()
                    : new List<DataCollectorLocation>();

                if (LocationNeedsUpdate(rawReport.Report, command.Body, dataCollectorLocations))
                {
                    await UpdateLocationForReport(rawReport.Report, command.Body.DataCollectorLocationId, command.Body.DataCollectorId);
                    locationChanged = true;
                }

                UpdateReportedCaseCountForReport(rawReport.Report, command.Body);

                var updatedReceivedAt = new DateTime(command.Body.Date.Year, command.Body.Date.Month, command.Body.Date.Day,
                    rawReport.ReceivedAt.Hour, rawReport.ReceivedAt.Minute, rawReport.ReceivedAt.Second);

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

                if (locationChanged && rawReport.Report.Status != ReportStatus.Rejected)
                {
                    await _alertReportService.RecalculateAlertForReport(rawReport.Id);
                }

                return SuccessMessage(ResultKey.Report.Edit.EditSuccess);
            }
            catch (ResultException e)
            {
                return Error(e.Result.Message.Key);
            }
        }

        private async Task SetProjectHealthRisk(Report report, int projectHealthRiskId, int dataCollectorId)
        {
            var projectId = report.DataCollector?.Project.Id ?? await _nyssContext.DataCollectors
                .Where(dc => dc.Id == dataCollectorId)
                .Select(dc => dc.Project.Id)
                .SingleOrDefaultAsync();

            var projectHealthRisk = await _nyssContext.ProjectHealthRisks
                .Include(phr => phr.HealthRisk)
                .SingleOrDefaultAsync(phr => phr.HealthRiskId == projectHealthRiskId
                    && phr.Project.Id == projectId);

            if (projectHealthRisk == null)
            {
                throw new ResultException(ResultKey.Report.Edit.HealthRiskNotAssignedToProject);
            }

            report.ProjectHealthRisk = projectHealthRisk;
        }

        private static bool LocationNeedsUpdate(Report report, ReportRequestDto requestDto, List<DataCollectorLocation> dataCollectorLocations)
        {
            if (report.RawReport.Village == null && report.RawReport.Zone == null)
            {
                return true;
            }

            var location = dataCollectorLocations
                .SingleOrDefault(dcl => dcl.Village == report.RawReport.Village && (dcl.Zone == null || dcl.Zone == report.RawReport.Zone));

            return requestDto.DataCollectorLocationId != location?.Id;
        }

        private async Task UpdateDataCollectorForReport(Report report, int dataCollectorId)
        {
            var newDataCollector = await _nyssContext.DataCollectors
                .Include(dc => dc.DataCollectorLocations).ThenInclude(dcl => dcl.Village)
                .Include(dc => dc.DataCollectorLocations).ThenInclude(dcl => dcl.Zone)
                .SingleOrDefaultAsync(dc => dc.Id == dataCollectorId);

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

        private async Task UpdateLocationForReport(Report report, int locationId, int dataCollectorId)
        {
            var location = await _nyssContext.DataCollectorLocations
                .Include(dcl => dcl.Village)
                .Include(dcl => dcl.Zone)
                .Where(dcl => dcl.DataCollectorId == dataCollectorId && dcl.Id == locationId)
                .SingleOrDefaultAsync();

            if (location == null)
            {
                throw new ResultException(ResultKey.Report.Edit.LocationNotFound);
            }

            report.Location = location.Location;
            report.RawReport.Village = location.Village;
            report.RawReport.Zone = location.Zone;
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

            report.ReportedCaseCount = (requestDto.CountMalesBelowFive + requestDto.CountMalesAtLeastFive + requestDto.CountFemalesBelowFive + requestDto.CountFemalesAtLeastFive) ?? 0;

            if (report.ReportType != ReportType.DataCollectionPoint)
            {
                return;
            }

            report.DataCollectionPointCase.ReferredCount = requestDto.ReferredCount;
            report.DataCollectionPointCase.DeathCount = requestDto.DeathCount;
            report.DataCollectionPointCase.FromOtherVillagesCount = requestDto.FromOtherVillagesCount;
        }
    }
}
