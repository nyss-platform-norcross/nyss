using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Utils.DataContract;
using static RX.Nyss.Web.Utils.DataContract.Result;

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

        public async Task<Result> CreateHealthRisk(CreateHealthRiskRequestDto healthRiskReq)
        {
            try
            {
                var healthRisk = new RX.Nyss.Data.Models.HealthRisk()
                {
                    Name = healthRiskReq.Name,
                    HealthRiskType = healthRiskReq.HealthRiskType,
                    HealthRiskCode = healthRiskReq.HealthRiskCode
                };

                if (!string.IsNullOrEmpty(healthRiskReq.AlertRuleInformation))
                {
                    healthRisk.AlertRule = new Nyss.Data.Models.AlertRule()
                    {
                        CountThreshold = healthRiskReq.AlertRuleCountThreshold,
                        HoursThreshold = healthRiskReq.AlertRuleHoursThreshold,
                        MetersThreshold = healthRiskReq.AlertRuleDistanceBetweenCases
                    };
                }

                await _nyssContext.AddAsync(healthRisk);
                await _nyssContext.SaveChangesAsync();
                return Success(ResultKey.HealthRisk.CreationSuccess);
            }
            catch (Exception e)
            {
                return Error(ResultKey.HealthRisk.CreationError);
            }
        }
    }
}
