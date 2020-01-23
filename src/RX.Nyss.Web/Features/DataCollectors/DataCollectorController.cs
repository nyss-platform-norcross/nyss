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
        public async Task<Result<GetDataCollectorResponseDto>> Get(int dataCollectorId) =>
            await _dataCollectorService.Get(dataCollectorId);

        [HttpGet, Route("list")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<IEnumerable<DataCollectorResponseDto>>> List(int projectId) =>
            await _dataCollectorService.List(projectId);

        [HttpPost, Route("create")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result> Create(int projectId, [FromBody]CreateDataCollectorRequestDto createDataCollectorDto) =>
            await _dataCollectorService.Create(projectId, createDataCollectorDto);

        [HttpPost, Route("{dataCollectorId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public async Task<Result> Edit([FromBody]EditDataCollectorRequestDto editDataCollectorDto) =>
            await _dataCollectorService.Edit(editDataCollectorDto);

        [HttpPost, Route("{dataCollectorId:int}/setTrainingState")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public async Task<Result> SetTrainingState(int dataCollectorId, bool isInTraining) =>
            await _dataCollectorService.SetTrainingState(dataCollectorId, isInTraining);

        [HttpPost, Route("{dataCollectorId:int}/delete")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public async Task<Result> Delete(int dataCollectorId) =>
            await _dataCollectorService.Delete(dataCollectorId);

        [HttpGet, Route("formData")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor)]
        public async Task<Result<DataCollectorFormDataResponse>> FormData(int projectId) =>
            await _dataCollectorService.GetFormData(projectId);

        [HttpGet, Route("mapOverview")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<MapOverviewResponseDto>> MapOverview(int projectId, DateTime from, DateTime to) =>
            await _dataCollectorService.MapOverview(projectId, from, to);

        [HttpGet, Route("mapOverviewDetails")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<List<MapOverviewDataCollectorResponseDto>>> MapOverviewDetails(int projectId, DateTime from, DateTime to, double lat, double lng) =>
            await _dataCollectorService.MapOverviewDetails(projectId, @from, to, lat, lng);

        [HttpGet, Route("performance")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<List<DataCollectorPerformanceResponseDto>>> Performance(int projectId) =>
            await _dataCollectorService.Performance(projectId);

        [HttpPost, Route("export")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<IActionResult> Export(int projectId) =>
            File(await _dataCollectorExportService.Export(projectId), "text/csv");
    }
}
