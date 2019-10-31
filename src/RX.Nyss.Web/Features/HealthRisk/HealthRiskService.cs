using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.HealthRisk
{
    public class HealthRiskService : IHealthRiskService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;

        public HealthRiskService(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public async Task<IEnumerable<HealthRiskResponseDto>> GetHealthRisks()
        {

            return await _nyssContext.HealthRisks.Select(x => 
                new HealthRiskResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    HealthRiskCode = x.HealthRiskCode,
                    HealthRiskType = x.HealthRiskType
                })
                .ToListAsync();
        }

        public async Task<EditHealthRiskRequestDto> GetHealthRisk(int id)
        {
            // return whole database object for editing?
            return await _nyssContext.HealthRisks
                .Select(h => new EditHealthRiskRequestDto
                {
                    Id = h.Id,
                    HealthRiskCode = h.HealthRiskCode,
                    HealthRiskType = h.HealthRiskType,
                    Name = h.Name
                }).FirstOrDefaultAsync(_ => _.Id == id);
        }

        public async Task<Result<int>> CreateHealthRisk(CreateHealthRiskRequestDto createHealthRiskDto)
        {
            try
            {
                var healthRisk = await _nyssContext.HealthRisks.FirstOrDefaultAsync(hr => hr.HealthRiskCode == createHealthRiskDto.HealthRiskCode);

                if (healthRisk != null)
                {
                    return Error(ResultKey.HealthRisk.HealthRiskNumberAlreadyExists).Cast<int>();
                }

                healthRisk = new Nyss.Data.Models.HealthRisk()
                {
                    Name = createHealthRiskDto.Name,
                    HealthRiskType = createHealthRiskDto.HealthRiskType,
                    HealthRiskCode = createHealthRiskDto.HealthRiskCode
                };

                if (createHealthRiskDto.AlertRuleCountThreshold > 0)
                {
                    healthRisk.AlertRule = new Nyss.Data.Models.AlertRule()
                    {
                        CountThreshold = createHealthRiskDto.AlertRuleCountThreshold,
                        HoursThreshold = createHealthRiskDto.AlertRuleHoursThreshold,
                        MetersThreshold = createHealthRiskDto.AlertRuleMetersThreshold
                    };
                }

                var entity = await _nyssContext.AddAsync(healthRisk);
                await _nyssContext.SaveChangesAsync();
                return Success(entity.Entity.Id, ResultKey.HealthRisk.CreationSuccess);
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return Error(ResultKey.HealthRisk.CreationError).Cast<int>();
            }
        }

        public async Task<Result> EditHealthRisk(EditHealthRiskRequestDto editHealthRiskDto)
        {
            try
            {
                var healthRisk = await _nyssContext.HealthRisks.FindAsync(editHealthRiskDto.Id);

                if (healthRisk == null)
                {
                    return Error(ResultKey.HealthRisk.HealthRiskNotFound);
                }

                healthRisk.HealthRiskCode = editHealthRiskDto.HealthRiskCode;
                healthRisk.HealthRiskType = editHealthRiskDto.HealthRiskType;
                healthRisk.Name = editHealthRiskDto.Name;
                
                if (editHealthRiskDto.AlertRuleCountThreshold > 0)
                {
                    var alertRule = await _nyssContext.AlertRules.FindAsync(editHealthRiskDto.AlertRuleId) ?? new Nyss.Data.Models.AlertRule();
                    alertRule.CountThreshold = editHealthRiskDto.AlertRuleCountThreshold;
                    alertRule.HoursThreshold = editHealthRiskDto.AlertRuleHoursThreshold;
                    alertRule.MetersThreshold = editHealthRiskDto.AlertRuleMetersThreshold;
                }

                await _nyssContext.SaveChangesAsync();
                return SuccessMessage(ResultKey.HealthRisk.EditSuccess);
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return Error(ResultKey.HealthRisk.EditError);
            }
        }

        public async Task SaveFeedbackMessage(string message, int languageId)
        {
            
        }

        public async Task SaveCaseDefinition(string caseDefinition, int languageId)
        {
            
        }
    }
}
