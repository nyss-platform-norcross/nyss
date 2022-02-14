using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
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
            var report = await _nyssContext.RawReports
                .Include(r => r.Report)
                .ThenInclude(r => r.ProjectHealthRisk)
                .ThenInclude(r => r.HealthRisk)
                .Select(r => new ReportResponseDto
                {
                    Id = r.Id,
                    DataCollectorId = r.DataCollector.Id,
                    ReportType = r.Report.ReportType,
                    ReportStatus = r.Report.Status,
                    LocationId = r.DataCollector.DataCollectorLocations
                        .Where(dcl => dcl.Village == r.Village && (dcl.Zone == null || dcl.Zone == r.Zone))
                        .Select(dcl => dcl.Id)
                        .FirstOrDefault(),
                    Date = r.ReceivedAt.Date,
                    HealthRiskId = r.Report.ProjectHealthRisk.HealthRiskId,
                    CountMalesBelowFive = r.Report.ReportedCase.CountMalesBelowFive,
                    CountMalesAtLeastFive = r.Report.ReportedCase.CountMalesAtLeastFive,
                    CountFemalesBelowFive = r.Report.ReportedCase.CountFemalesBelowFive,
                    CountFemalesAtLeastFive = r.Report.ReportedCase.CountFemalesAtLeastFive,
                    CountUnspecifiedSexAndAge = r.Report.ReportedCase.CountUnspecifiedSexAndAge,
                    ReferredCount = r.Report.DataCollectionPointCase.ReferredCount,
                    DeathCount = r.Report.DataCollectionPointCase.DeathCount,
                    FromOtherVillagesCount = r.Report.DataCollectionPointCase.FromOtherVillagesCount,
                    IsActivityReport = r.Report.IsActivityReport(),
                })
                .FirstOrDefaultAsync(r => r.Id == query.ReportId, cancellationToken);

            if (report == null)
            {
                return Error<ReportResponseDto>(ResultKey.Report.ReportNotFound);
            }

            return Success(report);
        }
    }
}
