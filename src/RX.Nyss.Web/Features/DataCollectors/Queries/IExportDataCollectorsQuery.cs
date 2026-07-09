using MediatR;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollectors.Dto;

namespace RX.Nyss.Web.Features.DataCollectors.Queries
{
    public interface IExportDataCollectorsQuery : IRequest<FileResultDto>
    {
        public int ProjectId { get; }
        public DataCollectorsFiltersRequestDto Filters { get; }
    }
}
