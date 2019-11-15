
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

        [HttpGet, Route("dataCollector/{dataCollectorId:int}/get"), NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public async Task<Result<GetDataCollectorResponseDto>> GetDataCollector(int dataCollectorId) =>
            await _dataCollectorService.GetDataCollector(dataCollectorId);

        //ToDo: Add project access policy once projects are ready
        [HttpGet, Route("project/{projectId:int}/dataCollector/list"), NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor)]
        public async Task<Result<IEnumerable<DataCollectorResponseDto>>> ListDataCollectors(int projectId) =>
            await _dataCollectorService.ListDataCollectors(projectId);

        //ToDo: Add project access policy once projects are ready
        [HttpPost, Route("project/{projectId:int}/dataCollector/create"), NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor)]
        public async Task<Result> CreateDataCollector(int projectId, [FromBody]CreateDataCollectorRequestDto createDataCollectorDto) => 
            await _dataCollectorService.CreateDataCollector(projectId, createDataCollectorDto);

        [HttpPost, Route("dataCollector/{dataCollectorId:int}/edit"), NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public async Task<Result> EditDataCollector([FromBody]EditDataCollectorRequestDto editDataCollectorDto) =>
            await _dataCollectorService.EditDataCollector(editDataCollectorDto);

        [HttpPost, Route("dataCollector/{dataCollectorId:int}/remove"), NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public async Task<Result> RemoveDataCollector(int dataCollectorId) =>
            await _dataCollectorService.RemoveDataCollector(dataCollectorId);

        [HttpGet, Route("project/{projectId:int}/dataCollector/formData"), NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor)]
        public async Task<Result<DataCollectorFormDataResponse>> GetDataCollectorFormData(int projectId) =>
            await _dataCollectorService.GetFormData(projectId, User.Identity.Name);

        //ToDo: Add project access policy once projects are ready. Additionally move it to project controller (?)
        [HttpGet, Route("project/{projectId:int}/basicData"), NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor)]
        public async Task<Result> GetProjectBasicData(int projectId) =>
            await _dataCollectorService.GetProjectBasicData(projectId);
    }
}