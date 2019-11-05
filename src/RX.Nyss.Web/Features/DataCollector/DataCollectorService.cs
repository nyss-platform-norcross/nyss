
using System.Collections.Generic;
using System.Threading.Tasks;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.DataCollector
{
    public interface IDataCollectorService
    {
        Task<Result<int>> CreateDataCollector(CreateDataCollectorRequestDto createDataCollectorDto);
        Task<Result> EditDataCollector(EditDataCollectorRequestDto editDataCollectorDto);
        Task<Result> RemoveDataCollector(int dataCollectorId);
        Task<Result<GetDataCollectorResponseDto>> GetDataCollector(int dataCollectorId);
        Task<Result<IEnumerable<DataCollectorResponseDto>>> ListDataCollectors();
    }
}
