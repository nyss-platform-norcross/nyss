using MediatR;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Reports.Dto;

namespace RX.Nyss.Web.Features.Reports.Queries
{
    public interface IExportReportQuery : IRequest<FileResultDto>
    {
        int ProjectId { get; }

        ReportListFilterRequestDto Filter { get; }
    }
}
