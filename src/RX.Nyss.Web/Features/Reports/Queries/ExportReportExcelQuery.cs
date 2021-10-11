using MediatR;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Features.Reports.Dto;

namespace RX.Nyss.Web.Features.Reports
{
    public class ExportReportExcelQuery : IRequest<FileResultDto>
    {
        public ExportReportExcelQuery(int projectId, ReportListFilterRequestDto filter)
        {
            ProjectId = projectId;
            Filter = filter;
        }

        public int ProjectId { get; }

        public ReportListFilterRequestDto Filter { get; }
    }
}
