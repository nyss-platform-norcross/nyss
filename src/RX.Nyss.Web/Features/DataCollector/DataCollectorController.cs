
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.DataCollector
{
    [Route("api/dataCollector")]
    public class DataCollectorController : BaseController
    {
        private readonly IDataCollectorService _dataCollectorService;

        public DataCollectorController(IDataCollectorService dataCollectorService)
        {
            _dataCollectorService = dataCollectorService;
        }

        [Route("get"), NeedsRole(Role.DataManager, Role.Supervisor)]
        public async Task<Result<GetDataCollectorResponseDto>> GetDataCollector(int dataCollectorId) =>
            await _dataCollectorService.GetDataCollector(dataCollectorId);

        [Route("list"), NeedsRole(Role.DataManager, Role.Supervisor)]
        public async Task<Result<IEnumerable<DataCollectorResponseDto>>> ListDataCollectors() =>
            await _dataCollectorService.ListDataCollectors();

        [Route("create"), NeedsRole(Role.DataManager, Role.Supervisor)]
        public async Task<Result<int>> CreateDataCollector([FromBody]CreateDataCollectorRequestDto createDataCollectorDto) => 
            await _dataCollectorService.CreateDataCollector(createDataCollectorDto);

        [Route("edit"), NeedsRole(Role.DataManager, Role.Supervisor)]
        public async Task<Result> EditDataCollector([FromBody]EditDataCollectorRequestDto editDataCollectorDto) =>
            await _dataCollectorService.EditDataCollector(editDataCollectorDto);

        [Route("remove"), NeedsRole(Role.DataManager, Role.Supervisor)]
        public async Task<Result> RemoveDataCollector(int dataCollectorId) =>
            await _dataCollectorService.RemoveDataCollector(dataCollectorId);
    }
}
