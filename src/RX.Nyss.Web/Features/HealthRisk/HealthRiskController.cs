using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authorization;

namespace RX.Nyss.Web.Features.HealthRisk
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HealthRiskController : ControllerBase
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
        [HttpGet]
        //[Roles(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<IEnumerable<HealthRiskDto>> GetHealthRisks() => await _healthRiskService.GetHealthRisks();
    }
}
