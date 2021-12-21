using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.Reports.Dto;

namespace RX.Nyss.Web.Features.Reports.Queries
{
    public class GetReportStatusQuery : IRequest<ReportStatusDto>
    {
        public string Timestamp { get; set; }

        public int DataCollectorId { get; set; }

        public class Handler : IRequestHandler<GetReportStatusQuery, ReportStatusDto>
        {
            private readonly INyssContext _context;

            public Handler(INyssContext context)
            {
                _context = context;
            }

            public async Task<ReportStatusDto> Handle(GetReportStatusQuery request, CancellationToken cancellationToken)
            {
                var rawReport = await _context.RawReports
                    .AsNoTracking()
                    .Include(r => r.DataCollector)
                    .FirstOrDefaultAsync(r => r.Timestamp == request.Timestamp && r.DataCollector.Id == request.DataCollectorId, cancellationToken);

                if (rawReport == null)
                {
                    return null;
                }

                return new ReportStatusDto
                {
                    ErrorType = rawReport.ErrorType,
                };
            }
        }
    }
}
