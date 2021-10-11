using MediatR;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Features.Reports.Dto;

namespace RX.Nyss.Web.Features.Reports
{
    public interface IExportReportQuery : IRequest<FileResultDto>
    {
        int ProjectId { get; }

        ReportListFilterRequestDto Filter { get; }
    }
}
