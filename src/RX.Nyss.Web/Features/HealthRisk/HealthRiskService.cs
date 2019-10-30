using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.HealthRisk.Dto;

namespace RX.Nyss.Web.Features.HealthRisk
{
    public class HealthRiskService : IHealthRiskService
    {
        private readonly NyssContext _nyssContext;

        public HealthRiskService(NyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public async Task<IEnumerable<HealthRiskDto>> GetHealthRisks()
        {

            return await _nyssContext.HealthRisks.Select(x => 
                new HealthRiskDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    HealthRiskCode = x.HealthRiskCode,
                    HealthRiskType = x.HealthRiskType
                })
                .ToListAsync();
        }
    }
}
