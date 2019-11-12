using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.HealthRisk.Dto;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.HealthRisk
{
    public interface IHealthRiskService
    {
        Task<Result<IEnumerable<HealthRiskResponseDto>>> GetHealthRisks(string userName);
        Task<Result<GetHealthRiskResponseDto>> GetHealthRisk(int id);
        Task<Result> CreateHealthRisk(CreateHealthRiskRequestDto createDto);
        Task<Result> EditHealthRisk(EditHealthRiskRequestDto editDto);
        Task<Result> RemoveHealthRisk(int id);
    }

    public class HealthRiskService : IHealthRiskService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;

        public HealthRiskService(INyssContext nyssContext, ILoggerAdapter loggerAdapter)
        {
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
        }

        public async Task<Result<IEnumerable<HealthRiskResponseDto>>> GetHealthRisks(string userName)
        {
            var languageCode = await _nyssContext.Users
                .Where(u => u.EmailAddress == userName)
                .Select(u => u.ApplicationLanguage.LanguageCode)
                .SingleOrDefaultAsync() ?? "EN";
                
            var healthRisks = await _nyssContext.HealthRisks
                .Select(hr => new HealthRiskResponseDto
                {
                    Id = hr.Id,
                    HealthRiskCode = hr.HealthRiskCode,
                    HealthRiskType = hr.HealthRiskType,
                    Name = hr.LanguageContents
                        .Where(lc => lc.ContentLanguage.LanguageCode == languageCode)
                        .Select(lc => lc.Name).FirstOrDefault()
                })
                .OrderBy(hr => hr.HealthRiskCode)
                .ToListAsync();

            return Success<IEnumerable<HealthRiskResponseDto>>(healthRisks);
        }

        public async Task<Result<GetHealthRiskResponseDto>> GetHealthRisk(int id)
        {
            var healthRiskResponse = await _nyssContext.HealthRisks
                .Where(healthRisk => healthRisk.Id == id)
                .Select(healthRisk => new GetHealthRiskResponseDto
                {
                    Id = healthRisk.Id,
                    HealthRiskCode = healthRisk.HealthRiskCode,
                    HealthRiskType = healthRisk.HealthRiskType,
                    AlertRuleCountThreshold = healthRisk.AlertRule != null ? healthRisk.AlertRule.CountThreshold : (int?) null,
                    AlertRuleDaysThreshold = healthRisk.AlertRule != null ? healthRisk.AlertRule.HoursThreshold / 24 : null,
                    AlertRuleMetersThreshold = healthRisk.AlertRule != null ? healthRisk.AlertRule.MetersThreshold : null,
                    LanguageContent = healthRisk.LanguageContents.Select(lc => new HealthRiskLanguageContentDto
                    {
                        LanguageId = lc.ContentLanguage.Id,
                        CaseDefinition = lc.CaseDefinition,
                        FeedbackMessage = lc.FeedbackMessage,
                        Name = lc.Name
                    })
                }).SingleOrDefaultAsync();

            if (healthRiskResponse == null)
            {
                return Error(ResultKey.HealthRisk.HealthRiskNotFound).Cast<GetHealthRiskResponseDto>();
            }

            return Success(healthRiskResponse);
        }

        public async Task<Result> CreateHealthRisk(CreateHealthRiskRequestDto createDto)
        {
            if (await _nyssContext.HealthRisks.AnyAsync(hr => hr.HealthRiskCode == createDto.HealthRiskCode))
            {
                return Error(ResultKey.HealthRisk.HealthRiskNumberAlreadyExists).Cast<int>();
            }

            var languageContentIds = createDto.LanguageContent.Select(lc => lc.LanguageId).ToArray();
            var contentLanguages = await _nyssContext.ContentLanguages.Where(cl => languageContentIds.Contains(cl.Id)).ToDictionaryAsync(cl => cl.Id, cl => cl);

            var healthRisk = new Nyss.Data.Models.HealthRisk
            {
                HealthRiskType = createDto.HealthRiskType,
                HealthRiskCode = createDto.HealthRiskCode,
                LanguageContents = createDto.LanguageContent.Select(lc => new HealthRiskLanguageContent
                {
                    Name = lc.Name,
                    FeedbackMessage = lc.FeedbackMessage,
                    CaseDefinition = lc.CaseDefinition,
                    ContentLanguage = contentLanguages[lc.LanguageId]
                }).ToList(),
                AlertRule = createDto.AlertRuleCountThreshold.HasValue 
                    ? new AlertRule
                    {
                        CountThreshold = createDto.AlertRuleCountThreshold.Value,
                        HoursThreshold = createDto.AlertRuleDaysThreshold * 24,
                        MetersThreshold = createDto.AlertRuleMetersThreshold
                    }
                    : null
            };

            await _nyssContext.AddAsync(healthRisk);
            await _nyssContext.SaveChangesAsync();
            return SuccessMessage(ResultKey.HealthRisk.CreationSuccess);
        }

        public async Task<Result> EditHealthRisk(EditHealthRiskRequestDto editDto)
        {
            var healthRisk = await _nyssContext.HealthRisks
                .Include(hr => hr.AlertRule)
                .Include(hr => hr.LanguageContents).ThenInclude(lc => lc.ContentLanguage)
                .SingleOrDefaultAsync(hr => hr.Id == editDto.Id);

            if (healthRisk == null)
            {
                return Error(ResultKey.HealthRisk.HealthRiskNotFound);
            }

            if (await _nyssContext.HealthRisks.AnyAsync(hr => hr.Id != editDto.Id && hr.HealthRiskCode == editDto.HealthRiskCode))
            {
                return Error(ResultKey.HealthRisk.HealthRiskNumberAlreadyExists).Cast<int>();
            }

            healthRisk.HealthRiskCode = editDto.HealthRiskCode;
            healthRisk.HealthRiskType = editDto.HealthRiskType;

            if (editDto.AlertRuleCountThreshold.HasValue)
            {
                healthRisk.AlertRule ??= new AlertRule();
                healthRisk.AlertRule.CountThreshold = editDto.AlertRuleCountThreshold.Value;
                healthRisk.AlertRule.HoursThreshold = editDto.AlertRuleDaysThreshold * 24;
                healthRisk.AlertRule.MetersThreshold = editDto.AlertRuleMetersThreshold;
            }
            else
            {
                healthRisk.AlertRule = null;
            }

            foreach (var languageContentDto in editDto.LanguageContent)
            {
                var languageContent = healthRisk.LanguageContents.SingleOrDefault(lc => lc.ContentLanguage.Id == languageContentDto.LanguageId)
                    ?? CreateNewLanguageContent(healthRisk, languageContentDto.LanguageId);

                languageContent.FeedbackMessage = languageContentDto.FeedbackMessage;
                languageContent.CaseDefinition = languageContentDto.CaseDefinition;
                languageContent.Name = languageContentDto.Name;
            }

            await _nyssContext.SaveChangesAsync();
            return SuccessMessage(ResultKey.HealthRisk.EditSuccess);
        }

        private HealthRiskLanguageContent CreateNewLanguageContent(Nyss.Data.Models.HealthRisk healthRisk, int languageId)
        {
            var newLanguageContent = new HealthRiskLanguageContent
            {
                HealthRisk = healthRisk,
                ContentLanguage = _nyssContext.ContentLanguages.Attach(new ContentLanguage { Id = languageId }).Entity
            };

            healthRisk.LanguageContents.Add(newLanguageContent);

            return newLanguageContent;
        }

        public async Task<Result> RemoveHealthRisk(int id)
        {
            var healthRisk = await _nyssContext.HealthRisks
                .Include(hr => hr.AlertRule)
                .Include(hr => hr.LanguageContents)
                .SingleOrDefaultAsync(hr => hr.Id == id);

            if (healthRisk.AlertRule != null)
            {
                _nyssContext.AlertRules.Remove(healthRisk.AlertRule);
            }

            _nyssContext.HealthRisks.Remove(healthRisk);
            await _nyssContext.SaveChangesAsync();
            return SuccessMessage(ResultKey.HealthRisk.RemoveSuccess);
        }
    }
}
