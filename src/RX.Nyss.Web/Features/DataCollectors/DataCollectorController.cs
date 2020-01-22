using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.DataCollectors
{
    [Route("api/dataCollector")]
    public class DataCollectorController : BaseController
    {
        private readonly IDataCollectorService _dataCollectorService;
        private readonly IDataCollectorExportService _dataCollectorExportService;

        public DataCollectorController(IDataCollectorService dataCollectorService, IDataCollectorExportService dataCollectorExportService)
        {
            _dataCollectorService = dataCollectorService;
            _dataCollectorExportService = dataCollectorExportService;
        }

        [HttpGet, Route("{dataCollectorId:int}/get")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public async Task<Result<GetDataCollectorResponseDto>> GetDataCollector(int dataCollectorId) =>
            await _dataCollectorService.GetDataCollector(dataCollectorId);

        [HttpGet, Route("list")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<IEnumerable<DataCollectorResponseDto>>> ListDataCollectors(int projectId) =>
            await _dataCollectorService.ListDataCollectors(projectId);

        [HttpPost, Route("create")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result> CreateDataCollector(int projectId, [FromBody]CreateDataCollectorRequestDto createDataCollectorDto) =>
            await _dataCollectorService.CreateDataCollector(projectId, createDataCollectorDto);

        [HttpPost, Route("{dataCollectorId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public async Task<Result> EditDataCollector([FromBody]EditDataCollectorRequestDto editDataCollectorDto) =>
            await _dataCollectorService.EditDataCollector(editDataCollectorDto);

        [HttpPost, Route("{dataCollectorId:int}/setTrainingState")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public async Task<Result> SetTrainingState(int dataCollectorId, bool isInTraining) =>
            await _dataCollectorService.SetTrainingState(dataCollectorId, isInTraining);

        [HttpPost, Route("{dataCollectorId:int}/remove")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public async Task<Result> RemoveDataCollector(int dataCollectorId) =>
            await _dataCollectorService.RemoveDataCollector(dataCollectorId);

        [HttpGet, Route("formData")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor)]
        public async Task<Result<DataCollectorFormDataResponse>> GetDataCollectorFormData(int projectId) =>
            await _dataCollectorService.GetFormData(projectId);

        [HttpGet, Route("mapOverview")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<MapOverviewResponseDto>> GetMapOverview(int projectId, DateTime from, DateTime to) =>
            await _dataCollectorService.GetMapOverview(projectId, from, to);

        [HttpGet, Route("mapOverviewDetails")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<List<MapOverviewDataCollectorResponseDto>>> GetMapOverviewDetails(int projectId, DateTime from, DateTime to, double lat, double lng) =>
            await _dataCollectorService.GetMapOverviewDetails(projectId, @from, to, lat, lng);

        [HttpGet, Route("performance")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<List<DataCollectorPerformanceResponseDto>>> GetDataCollectorPerformance(int projectId) =>
            await _dataCollectorService.GetDataCollectorPerformance(projectId);

        [HttpPost, Route("export")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<IActionResult> Export(int projectId) =>
            File(await _dataCollectorExportService.Export(projectId), "text/csv");
    }
}
