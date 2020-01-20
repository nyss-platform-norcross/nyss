using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.HealthRisks.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.HealthRisks
{
    public interface IHealthRiskService
    {
        Task<Result<IEnumerable<HealthRiskListItemResponseDto>>> ListHealthRisks();
        Task<Result<HealthRiskResponseDto>> GetHealthRisk(int id);
        Task<Result> CreateHealthRisk(HealthRiskRequestDto healthRiskRequestDto);
        Task<Result> EditHealthRisk(int id, HealthRiskRequestDto healthRiskRequestDto);
        Task<Result> RemoveHealthRisk(int id);
    }

    public class HealthRiskService : IHealthRiskService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;

        public HealthRiskService(INyssContext nyssContext, IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
        }

        public async Task<Result<IEnumerable<HealthRiskListItemResponseDto>>> ListHealthRisks()
        {
            var userName = _authorizationService.GetCurrentUserName();

            var languageCode = await _nyssContext.Users
                .Where(u => u.EmailAddress == userName)
                .Select(u => u.ApplicationLanguage.LanguageCode)
                .SingleOrDefaultAsync() ?? "EN";

            var healthRisks = await _nyssContext.HealthRisks
                .Select(hr => new HealthRiskListItemResponseDto
                {
                    Id = hr.Id,
                    HealthRiskCode = hr.HealthRiskCode,
                    HealthRiskType = hr.HealthRiskType,
                    Name = hr.LanguageContents
                        .Where(lc => lc.ContentLanguage.LanguageCode == languageCode)
                        .Select(lc => lc.Name)
                        .FirstOrDefault()
                })
                .OrderBy(hr => hr.HealthRiskCode)
                .ToListAsync();

            return Success<IEnumerable<HealthRiskListItemResponseDto>>(healthRisks);
        }

        public async Task<Result<HealthRiskResponseDto>> GetHealthRisk(int id)
        {
            var healthRiskResponse = await _nyssContext.HealthRisks
                .Where(healthRisk => healthRisk.Id == id)
                .Select(healthRisk => new HealthRiskResponseDto
                {
                    Id = healthRisk.Id,
                    HealthRiskCode = healthRisk.HealthRiskCode,
                    HealthRiskType = healthRisk.HealthRiskType,
                    AlertRuleCountThreshold = healthRisk.AlertRule != null ? healthRisk.AlertRule.CountThreshold : (int?) null,
                    AlertRuleDaysThreshold = healthRisk.AlertRule != null ? healthRisk.AlertRule.DaysThreshold : null,
                    AlertRuleKilometersThreshold = healthRisk.AlertRule != null ? healthRisk.AlertRule.KilometersThreshold : null,
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
                return Error<HealthRiskResponseDto>(ResultKey.HealthRisk.HealthRiskNotFound);
            }

            return Success(healthRiskResponse);
        }

        public async Task<Result> CreateHealthRisk(HealthRiskRequestDto healthRiskRequestDto)
        {
            if (await _nyssContext.HealthRisks.AnyAsync(hr => hr.HealthRiskCode == healthRiskRequestDto.HealthRiskCode))
            {
                return Error(ResultKey.HealthRisk.HealthRiskNumberAlreadyExists);
            }

            var languageContentIds = healthRiskRequestDto.LanguageContent.Select(lc => lc.LanguageId).ToArray();
            var contentLanguages = await _nyssContext.ContentLanguages.Where(cl => languageContentIds.Contains(cl.Id)).ToDictionaryAsync(cl => cl.Id, cl => cl);

            var healthRisk = new HealthRisk
            {
                HealthRiskType = healthRiskRequestDto.HealthRiskType,
                HealthRiskCode = healthRiskRequestDto.HealthRiskCode,
                LanguageContents = healthRiskRequestDto.LanguageContent.Select(lc => new HealthRiskLanguageContent
                {
                    Name = lc.Name,
                    FeedbackMessage = lc.FeedbackMessage,
                    CaseDefinition = lc.CaseDefinition,
                    ContentLanguage = contentLanguages[lc.LanguageId]
                }).ToList(),
                AlertRule = healthRiskRequestDto.AlertRuleCountThreshold.HasValue
                    ? new AlertRule
                    {
                        CountThreshold = healthRiskRequestDto.AlertRuleCountThreshold.Value,
                        DaysThreshold = healthRiskRequestDto.AlertRuleDaysThreshold,
                        KilometersThreshold = healthRiskRequestDto.AlertRuleKilometersThreshold
                    }
                    : null
            };

            await _nyssContext.AddAsync(healthRisk);
            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(ResultKey.HealthRisk.CreationSuccess);
        }

        public async Task<Result> EditHealthRisk(int id, HealthRiskRequestDto healthRiskRequestDto)
        {
            var healthRisk = await _nyssContext.HealthRisks
                .Include(hr => hr.AlertRule)
                .Include(hr => hr.LanguageContents)
                    .ThenInclude(lc => lc.ContentLanguage)
                .SingleOrDefaultAsync(hr => hr.Id == id);

            if (healthRisk == null)
            {
                return Error(ResultKey.HealthRisk.HealthRiskNotFound);
            }

            if (await _nyssContext.HealthRisks.AnyAsync(hr => hr.Id != id && hr.HealthRiskCode == healthRiskRequestDto.HealthRiskCode))
            {
                return Error(ResultKey.HealthRisk.HealthRiskNumberAlreadyExists);
            }

            if (CodeOrNameWasChanged(healthRiskRequestDto, healthRisk) && await HealthRiskContainsReports(id))
            {
                return Error(ResultKey.HealthRisk.HealthRiskContainsReports);
            }

            healthRisk.HealthRiskCode = healthRiskRequestDto.HealthRiskCode;
            healthRisk.HealthRiskType = healthRiskRequestDto.HealthRiskType;

            if (healthRiskRequestDto.AlertRuleCountThreshold.HasValue)
            {
                healthRisk.AlertRule ??= new AlertRule();
                healthRisk.AlertRule.CountThreshold = healthRiskRequestDto.AlertRuleCountThreshold.Value;
                healthRisk.AlertRule.DaysThreshold = healthRiskRequestDto.AlertRuleDaysThreshold;
                healthRisk.AlertRule.KilometersThreshold = healthRiskRequestDto.AlertRuleKilometersThreshold;
            }
            else
            {
                if (healthRisk.AlertRule != null)
                {
                    _nyssContext.AlertRules.Remove(healthRisk.AlertRule);
                }

                healthRisk.AlertRule = null;
            }

            foreach (var languageContentDto in healthRiskRequestDto.LanguageContent)
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

        public async Task<Result> RemoveHealthRisk(int id)
        {
            var healthRisk = await _nyssContext.HealthRisks
                .Include(hr => hr.AlertRule)
                .Include(hr => hr.LanguageContents)
                .SingleOrDefaultAsync(hr => hr.Id == id);

            if (healthRisk == null)
            {
                return Error(ResultKey.HealthRisk.HealthRiskNotFound);
            }

            if (await HealthRiskContainsReports(id))
            {
                return Error(ResultKey.HealthRisk.HealthRiskContainsReports);
            }

            if (healthRisk.AlertRule != null)
            {
                _nyssContext.AlertRules.Remove(healthRisk.AlertRule);
            }

            _nyssContext.HealthRisks.Remove(healthRisk);
            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(ResultKey.HealthRisk.RemoveSuccess);
        }

        private static bool CodeOrNameWasChanged(HealthRiskRequestDto healthRiskRequestDto, HealthRisk healthRisk) =>
            healthRiskRequestDto.HealthRiskCode != healthRisk.HealthRiskCode  ||
            healthRiskRequestDto.LanguageContent.Any(lcDto =>
                healthRisk.LanguageContents.Any(lc =>
                    lc.ContentLanguage.Id == lcDto.LanguageId && !string.IsNullOrEmpty(lc.Name)) &&
                lcDto.Name != healthRisk.LanguageContents.Single(lc => lc.ContentLanguage.Id == lcDto.LanguageId).Name);

        private async Task<bool> HealthRiskContainsReports(int healthRiskId) =>
            await _nyssContext.ProjectHealthRisks.AnyAsync(phr => phr.HealthRiskId == healthRiskId && phr.Reports.Any(r => !r.IsTraining));

        private HealthRiskLanguageContent CreateNewLanguageContent(HealthRisk healthRisk, int languageId)
        {
            var newLanguageContent = new HealthRiskLanguageContent
            {
                HealthRisk = healthRisk,
                ContentLanguageId = languageId,
            };

            healthRisk.LanguageContents.Add(newLanguageContent);

            return newLanguageContent;
        }
    }
}
