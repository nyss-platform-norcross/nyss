using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Web.Utils;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Features.HealthRisk.Dto;

namespace RX.Nyss.Web.Features.HealthRisk
{
    [Route("api/healthRisk")]
    public class HealthRiskController : BaseController
    {
        private readonly IHealthRiskService _healthRiskService;

        public HealthRiskController(IHealthRiskService healthRiskService)
        {
            _healthRiskService = healthRiskService;
        }

        /// <summary>
        /// Gets a list of all health risks.
        /// </summary>
        /// <returns>A list of health risks</returns>
        [HttpGet, Route("list")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result<IEnumerable<HealthRiskListItemResponseDto>>> GetHealthRisks() => 
            await _healthRiskService.GetHealthRisks(User.Identity.Name);

        /// <summary>
        /// Gets a health risk with all values for editing.
        /// </summary>
        /// <param name="id">An identifier of a health risk</param>
        /// <returns>A health risk</returns>
        [HttpGet, Route("{id:int}/get")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result<HealthRiskResponseDto>> GetHealthRisk(int id) => 
            await _healthRiskService.GetHealthRisk(id);

        /// <summary>
        /// Creates a new health risk.
        /// </summary>
        /// <param name="healthRiskDto"></param>
        /// <returns>An identifier of the created health risk</returns>
        [HttpPost, Route("create"), NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result> Create([FromBody]HealthRiskRequestDto healthRiskDto) => 
            await _healthRiskService.CreateHealthRisk(healthRiskDto);

        /// <summary>
        /// Edits a health risk.
        /// </summary>
        /// <param name="id">An identifier of a health risk</param>
        /// <param name="healthRiskDto"></param>
        /// <returns></returns>
        [HttpPost, Route("{id:int}/edit"), NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result> Edit(int id, [FromBody]HealthRiskRequestDto healthRiskDto) =>
            await _healthRiskService.EditHealthRisk(id, healthRiskDto);

        /// <summary>
        /// Removes a health risk.
        /// </summary>
        /// <param name="id">An identifier of a health risk</param>
        /// <returns></returns>
        [HttpPost, Route("{id:int}/remove"), NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result> Remove(int id) =>
            await _healthRiskService.RemoveHealthRisk(id);
    }
}
