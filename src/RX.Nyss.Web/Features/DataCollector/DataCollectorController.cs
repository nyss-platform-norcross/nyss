
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.DataCollector.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.DataCollector
{
    [Route("api")]
    public class DataCollectorController : BaseController
    {
        private readonly IDataCollectorService _dataCollectorService;

        public DataCollectorController(IDataCollectorService dataCollectorService)
        {
            _dataCollectorService = dataCollectorService;
        }

        [HttpGet, Route("dataCollector/{dataCollectorId:int}/get"), NeedsRole(Role.Administrator, Role.DataManager, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public async Task<Result<GetDataCollectorResponseDto>> GetDataCollector(int dataCollectorId) =>
            await _dataCollectorService.GetDataCollector(dataCollectorId);

        [HttpGet, Route("project/{projectId:int}/dataCollector/list"), NeedsRole(Role.Administrator, Role.DataManager, Role.Supervisor)]
        public async Task<Result<IEnumerable<DataCollectorResponseDto>>> ListDataCollectors() =>
            await _dataCollectorService.ListDataCollectors();

        [HttpPost, Route("project/{projectId:int}/dataCollector/create"), NeedsRole(Role.Administrator, Role.DataManager, Role.Supervisor)]
        public async Task<Result<int>> CreateDataCollector(int projectId, [FromBody]CreateDataCollectorRequestDto createDataCollectorDto) => 
            await _dataCollectorService.CreateDataCollector(projectId, createDataCollectorDto);

        [HttpPost, Route("dataCollector/{dataCollectorId:int}/edit"), NeedsRole(Role.Administrator, Role.DataManager, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public async Task<Result> EditDataCollector(int projectId, [FromBody]EditDataCollectorRequestDto editDataCollectorDto) =>
            await _dataCollectorService.EditDataCollector(projectId, editDataCollectorDto);

        [HttpPost, Route("dataCollector/{dataCollectorId:int}/remove"), NeedsRole(Role.Administrator, Role.DataManager, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public async Task<Result> RemoveDataCollector(int dataCollectorId) =>
            await _dataCollectorService.RemoveDataCollector(dataCollectorId);
    }
}
