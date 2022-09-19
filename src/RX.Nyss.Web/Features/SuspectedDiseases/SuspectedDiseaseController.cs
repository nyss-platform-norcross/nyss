using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.SuspectedDiseases.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.SuspectedDiseases
{
    [Route("api/suspectedDisease")]
    public class SuspectedDiseaseController : BaseController
    {
        private readonly ISuspectedDiseaseService _suspectedDiseaseService;

        public SuspectedDiseaseController(ISuspectedDiseaseService suspectedDiseaseService)
        {
            _suspectedDiseaseService = suspectedDiseaseService;
        }

        /// <summary>
        /// Gets a list with basic information of all suspected diseases.
        /// </summary>
        /// <returns>A list of basic information of suspected disease</returns>
        [HttpGet, Route("list")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result<IEnumerable<SuspectedDiseaseListItemResponseDto>>> List() =>
            await _suspectedDiseaseService.List();

        /// <summary>
        /// Gets a suspected disease with all values for editing.
        /// </summary>
        /// <param name="id">An identifier of a suspected disease</param>
        /// <returns>A suspected disease</returns>
        [HttpGet, Route("{id:int}/get")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result<SuspectedDiseaseResponseDto>> Get(int id) =>
            await _suspectedDiseaseService.Get(id);

        /// <summary>
        /// Creates a new suspected disease.
        /// </summary>
        /// <param name="suspectedDiseaseRequestDto"></param>
        /// <returns>An identifier of the created suspected disease</returns>
        [HttpPost, Route("create"), NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result> Create([FromBody] SuspectedDiseaseRequestDto suspectedDiseaseRequestDto) =>
            await _suspectedDiseaseService.Create(suspectedDiseaseRequestDto);

        /// <summary>
        /// Edits a suspected disease.
        /// </summary>
        /// <param name="id">An identifier of a suspected disease</param>
        /// <param name="suspectedDiseaseRequestDto">A suspected disease</param>
        /// <returns></returns>
        [HttpPost, Route("{id:int}/edit"), NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result> Edit(int id, [FromBody] SuspectedDiseaseRequestDto suspectedDiseaseRequestDto) =>
            await _suspectedDiseaseService.Edit(id, suspectedDiseaseRequestDto);

        /// <summary>
        /// Deletes a suspected disease.
        /// </summary>
        /// <param name="id">An identifier of a suspected disease</param>
        /// <returns></returns>
        [HttpPost, Route("{id:int}/delete"), NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result> Delete(int id) =>
            await _suspectedDiseaseService.Delete(id);
    }
}
