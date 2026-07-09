using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.Reports.Dto;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Reports.Queries;

public class GetReportQuery : IRequest<Result<ReportResponseDto>>
{
    public int ReportId { get; }

    public GetReportQuery(int reportId)
    {
        ReportId = reportId;
    }

    public class Handler : IRequestHandler<GetReportQuery, Result<ReportResponseDto>>
    {
        private readonly INyssContext _nyssContext;

        public Handler(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public async Task<Result<ReportResponseDto>> Handle(GetReportQuery query, CancellationToken cancellationToken)
        {
            var rawReport = await _nyssContext.RawReports
                .Where(r => r.Id == query.ReportId)
                .Include(r => r.Village)
                .Include(r => r.Zone)
                .Include(r => r.DataCollector!)
                    .ThenInclude(dc => dc.DataCollectorLocations!)
                        .ThenInclude(dcl => dcl.Village)
                .Include(r => r.DataCollector!)
                    .ThenInclude(dc => dc.DataCollectorLocations!)
                        .ThenInclude(dcl => dcl.Zone)
                .Include(r => r.Report!)
                    .ThenInclude(report => report.ProjectHealthRisk)
                .Include(r => r.Report!)
                    .ThenInclude(report => report.ReportedCase)
                .Include(r => r.Report!)
                    .ThenInclude(report => report.DataCollectionPointCase)
                .FirstOrDefaultAsync(cancellationToken);

            if (rawReport == null)
            {
                return Error<ReportResponseDto>(ResultKey.Report.ReportNotFound);
            }

            var locationId = rawReport.DataCollector?.DataCollectorLocations?
                .Where(dcl => dcl.Village?.Id == rawReport.Village?.Id
                    && (dcl.Zone == null || dcl.Zone?.Id == rawReport.Zone?.Id))
                .Select(dcl => (int?)dcl.Id)
                .FirstOrDefault();

            var report = new ReportResponseDto
            {
                Id = rawReport.Id,
                DataCollectorId = rawReport.DataCollector?.Id,
                ReportType = rawReport.Report?.ReportType,
                ReportStatus = rawReport.Report?.Status,
                LocationId = locationId,
                Date = rawReport.ReceivedAt.Date,
                HealthRiskId = rawReport.Report?.ProjectHealthRisk?.HealthRiskId,
                CountMalesBelowFive = rawReport.Report?.ReportedCase?.CountMalesBelowFive,
                CountMalesAtLeastFive = rawReport.Report?.ReportedCase?.CountMalesAtLeastFive,
                CountFemalesBelowFive = rawReport.Report?.ReportedCase?.CountFemalesBelowFive,
                CountFemalesAtLeastFive = rawReport.Report?.ReportedCase?.CountFemalesAtLeastFive,
                CountUnspecifiedSexAndAge = rawReport.Report?.ReportedCase?.CountUnspecifiedSexAndAge,
                ReferredCount = rawReport.Report?.DataCollectionPointCase?.ReferredCount,
                DeathCount = rawReport.Report?.DataCollectionPointCase?.DeathCount,
                FromOtherVillagesCount = rawReport.Report?.DataCollectionPointCase?.FromOtherVillagesCount,
                IsActivityReport = rawReport.Report != null
                    && rawReport.Report.ReportType != Nyss.Data.Concepts.ReportType.Aggregate
                    && rawReport.Report.ReportType != Nyss.Data.Concepts.ReportType.DataCollectionPoint
                    && rawReport.Report.ReportType != Nyss.Data.Concepts.ReportType.Event
                    && rawReport.Report.ReportType != Nyss.Data.Concepts.ReportType.Single,
            };

            return Success(report);
        }
    }
}