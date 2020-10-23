using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.DataCollectors.Access;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.DataCollectors
{
    [Route("api/dataCollector")]
    public class DataCollectorController : BaseController
    {
        private readonly IDataCollectorService _dataCollectorService;
        private readonly IDataCollectorExportService _dataCollectorExportService;
        private readonly IDataCollectorAccessService _dataCollectorAccessService;

        public DataCollectorController(
            IDataCollectorService dataCollectorService,
            IDataCollectorExportService dataCollectorExportService,
            IDataCollectorAccessService dataCollectorAccessService)
        {
            _dataCollectorService = dataCollectorService;
            _dataCollectorExportService = dataCollectorExportService;
            _dataCollectorAccessService = dataCollectorAccessService;
        }

        [HttpGet, Route("{dataCollectorId:int}/get")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public Task<Result<GetDataCollectorResponseDto>> Get(int dataCollectorId) =>
            _dataCollectorService.Get(dataCollectorId);

        [HttpGet, Route("filters")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<DataCollectorFiltersReponseDto>> Filters(int projectId) =>
            _dataCollectorService.GetFiltersData(projectId);

        [HttpPost, Route("list")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<PaginatedList<DataCollectorResponseDto>>> List(int projectId, [FromBody] DataCollectorsFiltersRequestDto dataCollectorsFiltersDto) =>
            _dataCollectorService.List(projectId, dataCollectorsFiltersDto);

        [HttpPost, Route("listAll")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<List<DataCollectorResponseDto>>> ListAll(int projectId, [FromBody] DataCollectorsFiltersRequestDto dataCollectorsFiltersDto) =>
            _dataCollectorService.ListAll(projectId, dataCollectorsFiltersDto);

        [HttpPost, Route("create")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result> Create(int projectId, [FromBody] CreateDataCollectorRequestDto createDataCollectorDto) =>
            _dataCollectorService.Create(projectId, createDataCollectorDto);

        [HttpPost, Route("{dataCollectorId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public Task<Result> Edit([FromBody] EditDataCollectorRequestDto editDataCollectorDto) =>
            _dataCollectorService.Edit(editDataCollectorDto);

        [HttpPost, Route("setTrainingState")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.MultipleDataCollectorsAccess)]
        public Task<Result> SetTrainingState(SetDataCollectorsTrainingStateRequestDto dto) =>
            _dataCollectorService.SetTrainingState(dto);

        [HttpPost, Route("{dataCollectorId:int}/delete")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.DataCollectorAccess)]
        public Task<Result> Delete(int dataCollectorId) =>
            _dataCollectorService.Delete(dataCollectorId);

        [HttpGet, Route("formData")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor)]
        public Task<Result<DataCollectorFormDataResponse>> FormData(int projectId) =>
            _dataCollectorService.GetFormData(projectId);

        [HttpGet, Route("mapOverview")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<MapOverviewResponseDto>> MapOverview(int projectId, DateTime from, DateTime to) =>
            _dataCollectorService.MapOverview(projectId, from, to);

        [HttpGet, Route("mapOverviewDetails")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<List<MapOverviewDataCollectorResponseDto>>> MapOverviewDetails(int projectId, DateTime from, DateTime to, double lat, double lng) =>
            _dataCollectorService.MapOverviewDetails(projectId, from, to, lat, lng);

        [HttpPost, Route("performance")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<PaginatedList<DataCollectorPerformanceResponseDto>>> Performance(int projectId, [FromBody] DataCollectorPerformanceFiltersRequestDto dataCollectorsFiltersDto) =>
            await _dataCollectorService.Performance(projectId, dataCollectorsFiltersDto);

        [HttpPost, Route("exportToExcel")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<IActionResult> ExportToExcel(int projectId, [FromBody] DataCollectorsFiltersRequestDto dataCollectorsFiltersDto) =>
            File(await _dataCollectorExportService.ExportAsXls(projectId, dataCollectorsFiltersDto), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        [HttpPost, Route("exportToCsv")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<IActionResult> ExportToCsv(int projectId, [FromBody] DataCollectorsFiltersRequestDto dataCollectorsFiltersDto) =>
            File(await _dataCollectorExportService.ExportAsCsv(projectId, dataCollectorsFiltersDto), "text/csv");

        [HttpPost, Route("replaceSupervisor")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.MultipleDataCollectorsAccess)]
        public async Task<Result> ReplaceSupervisor([FromBody] ReplaceSupervisorRequestDto replaceSupervisorRequestDto) =>
            await _dataCollectorService.ReplaceSupervisor(replaceSupervisorRequestDto);
    }
}
