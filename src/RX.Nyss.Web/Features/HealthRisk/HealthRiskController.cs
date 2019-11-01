using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Web.Utils;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Utils.DataContract;

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
        [HttpGet, Route("getHealthRisks")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<IEnumerable<HealthRiskResponseDto>> GetHealthRisks(int languageId) => 
            await _healthRiskService.GetHealthRisks(languageId);

        /// <summary>
        /// Gets a health risk with all values for editing.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("getHealthRisk")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<EditHealthRiskRequestDto> GetHealthRisk(int id) => 
            await _healthRiskService.GetHealthRisk(id);

        /// <summary>
        /// Creates a global health risk.
        /// </summary>
        /// <param name="healthRisk"></param>
        /// <returns></returns>
        [HttpPost, Route("create"), NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result> Create([FromBody]CreateHealthRiskRequestDto healthRisk) => 
            await _healthRiskService.CreateHealthRisk(healthRisk);

        /// <summary>
        /// Edits a global health risk.
        /// </summary>
        /// <param name="healthRisk"></param>
        /// <returns></returns>
        [HttpPost, Route("edit"), NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result> Edit([FromBody]EditHealthRiskRequestDto healthRisk) =>
            await _healthRiskService.EditHealthRisk(healthRisk);

        /// <summary>
        /// Removes a global health risk.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, Route("remove"), NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result> Remove(int id) =>
            await _healthRiskService.RemoveHealthRisk(id);
    }
}
