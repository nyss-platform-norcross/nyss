using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.HealthRisk
{
    public class HealthRiskService : IHealthRiskService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;

        public HealthRiskService(INyssContext nyssContext, ILoggerAdapter loggerAdapter)
        {
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
        }

        public async Task<Result<IEnumerable<HealthRiskResponseDto>>> GetHealthRisks(int languageId)
        {
            try
            {
                var healthRisks = new List<HealthRiskResponseDto>();

                foreach (var healthRisk in await _nyssContext.HealthRisks.ToListAsync())
                {
                    var languageContent = await _nyssContext.HealthRiskLanguageContents.FirstOrDefaultAsync(x => x.HealthRisk == healthRisk && x.ContentLanguage.Id == languageId);
                    healthRisks.Add(new HealthRiskResponseDto
                    {
                        Id = healthRisk.Id,
                        HealthRiskCode = healthRisk.HealthRiskCode,
                        HealthRiskType = healthRisk.HealthRiskType,
                        Name = languageContent.Name
                    });
                }

                return Success<IEnumerable<HealthRiskResponseDto>>(healthRisks);
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return Error(ResultKey.Shared.GeneralErrorMessage).Cast<IEnumerable<HealthRiskResponseDto>>();
            }
        }

        public async Task<Result<EditHealthRiskRequestDto>> GetHealthRisk(int id)
        {
            try
            {
                var healthRisk = await _nyssContext.HealthRisks.FindAsync(id);
                var languageContents = _nyssContext.HealthRiskLanguageContents.Where(x => x.HealthRisk.Id == id);
                var healthRiskResponse = new EditHealthRiskRequestDto
                {
                    Id = healthRisk.Id,
                    HealthRiskCode = healthRisk.HealthRiskCode,
                    HealthRiskType = healthRisk.HealthRiskType,
                    LanguageContent = languageContents.Select(l => new HealthRiskLanguageContentDto
                    {
                        LanguageId = l.ContentLanguage.Id,
                        CaseDefinition = l.CaseDefinition,
                        FeedbackMessage = l.FeedbackMessage,
                        Name = l.Name
                    })
                };
                return Success<EditHealthRiskRequestDto>(healthRiskResponse);
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return Error(ResultKey.Shared.GeneralErrorMessage).Cast<EditHealthRiskRequestDto>();
            }
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
                    HealthRiskType = createHealthRiskDto.HealthRiskType,
                    HealthRiskCode = createHealthRiskDto.HealthRiskCode
                };

                var healthRiskLanguageContents = new List<HealthRiskLanguageContent>();

                foreach (var lc in createHealthRiskDto.LanguageContent)
                {
                    healthRiskLanguageContents.Add(new HealthRiskLanguageContent
                    {
                        Name = lc.Name,
                        CaseDefinition = lc.CaseDefinition,
                        FeedbackMessage = lc.FeedbackMessage,
                        ContentLanguage = await _nyssContext.ContentLanguages.FindAsync(lc.LanguageId)
                    });
                }

                healthRisk.LanguageContents = healthRiskLanguageContents;

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

                var healthRiskLanguageContents = await _nyssContext.HealthRiskLanguageContents.Where(lc => lc.HealthRisk == healthRisk)
                    .Select(x => new HealthRiskLanguageContent
                    {
                        Id = x.Id,
                        Name = x.Name,
                        CaseDefinition = x.CaseDefinition,
                        FeedbackMessage = x.FeedbackMessage,
                        ContentLanguage = x.ContentLanguage
                    }).ToListAsync();

                healthRisk.LanguageContents = healthRiskLanguageContents.Select(lc => 
                {
                    var edited = editHealthRiskDto.LanguageContent.FirstOrDefault(x => x.LanguageId == lc.ContentLanguage.Id);
                    lc.CaseDefinition = edited.CaseDefinition;
                    lc.FeedbackMessage = edited.FeedbackMessage;
                    lc.Name = edited.Name;
                    return lc;
                }).ToList();
                
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

        public async Task<Result> RemoveHealthRisk(int id)
        {
            try
            {
                await RemoveLanguageContentsForHealthRisk(id);
                var healthRisk = await _nyssContext.HealthRisks.FindAsync(id);
                _nyssContext.HealthRisks.Remove(healthRisk);
                await _nyssContext.SaveChangesAsync();
                return SuccessMessage(ResultKey.HealthRisk.RemoveSuccess);
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return Error(ResultKey.Shared.GeneralErrorMessage);
            }
        }

        public async Task RemoveLanguageContentsForHealthRisk(int healthRiskId)
        {
            var languageContents = _nyssContext.HealthRiskLanguageContents.Where(x => x.HealthRisk.Id == healthRiskId);
            _nyssContext.HealthRiskLanguageContents.RemoveRange(languageContents);
            await _nyssContext.SaveChangesAsync();
        }
    }
}
