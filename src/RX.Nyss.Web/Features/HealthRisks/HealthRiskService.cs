using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.HealthRisks.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.HealthRisks
{
    public interface IHealthRiskService
    {
        Task<Result<IEnumerable<HealthRiskListItemResponseDto>>> List();
        Task<Result<HealthRiskResponseDto>> Get(int id);
        Task<Result> Create(HealthRiskRequestDto healthRiskRequestDto);
        Task<Result> Edit(int id, HealthRiskRequestDto healthRiskRequestDto);
        Task<Result> Delete(int id);
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

        public async Task<Result<IEnumerable<HealthRiskListItemResponseDto>>> List()
        {
            var userName = _authorizationService.GetCurrentUserName();

            var languageCode = await _nyssContext.Users.FilterAvailable()
                .Where(u => u.EmailAddress == userName)
                .Select(u => u.ApplicationLanguage.LanguageCode)
                .SingleOrDefaultAsync() ?? "en";

            var healthRisks = await _nyssContext.HealthRisks
                .Select(hr => new HealthRiskListItemResponseDto
                {
                    Id = hr.Id,
                    HealthRiskCode = hr.HealthRiskCode,
                    HealthRiskType = hr.HealthRiskType,
                    Name = hr.LanguageContents
                        .Where(lc => lc.ContentLanguage.LanguageCode == languageCode)
                        .Select(lc => lc.Name)
                        .FirstOrDefault() ?? "en"
                })
                .OrderBy(hr => hr.HealthRiskCode)
                .AsSplitQuery()
                .ToListAsync();

            return Success<IEnumerable<HealthRiskListItemResponseDto>>(healthRisks);
        }

        public async Task<Result<HealthRiskResponseDto>> Get(int id)
        {
            var healthRiskResponse = await _nyssContext.HealthRisks
                .Where(healthRisk => healthRisk.Id == id)
                .Select(healthRisk => new HealthRiskResponseDto
                {
                    Id = healthRisk.Id,
                    HealthRiskCode = healthRisk.HealthRiskCode,
                    HealthRiskType = healthRisk.HealthRiskType,
                    HealthRiskSuspectedDiseases = healthRisk.HealthRiskSuspectedDiseases,
                    SuspectedDiseasesList = null,
                    AlertRuleCountThreshold = _nyssContext.AlertRules
                        .Where(alertRule => alertRule.Id == EF.Property<int?>(healthRisk, "AlertRuleId"))
                        .Select(alertRule => (int?)alertRule.CountThreshold)
                        .FirstOrDefault(),
                    AlertRuleDaysThreshold = _nyssContext.AlertRules
                        .Where(alertRule => alertRule.Id == EF.Property<int?>(healthRisk, "AlertRuleId"))
                        .Select(alertRule => alertRule.DaysThreshold)
                        .FirstOrDefault(),
                    AlertRuleKilometersThreshold = _nyssContext.AlertRules
                        .Where(alertRule => alertRule.Id == EF.Property<int?>(healthRisk, "AlertRuleId"))
                        .Select(alertRule => alertRule.KilometersThreshold)
                        .FirstOrDefault(),
                    LanguageContent = healthRisk.LanguageContents.Select(languageContent => new HealthRiskLanguageContentDto
                    {
                        LanguageId = languageContent.ContentLanguage.Id,
                        CaseDefinition = languageContent.CaseDefinition,
                        FeedbackMessage = languageContent.FeedbackMessage,
                        Name = languageContent.Name
                    })
                }).SingleOrDefaultAsync();

            if (healthRiskResponse == null)
            {
                return Error<HealthRiskResponseDto>(ResultKey.HealthRisk.HealthRiskNotFound);
            }

            return Success(healthRiskResponse);
        }

        public async Task<Result> Create(HealthRiskRequestDto dto)
        {
            if (await _nyssContext.HealthRisks.AnyAsync(hr => hr.HealthRiskCode == dto.HealthRiskCode))
            {
                return Error(ResultKey.HealthRisk.HealthRiskNumberAlreadyExists);
            }
            var languageContentIds = dto.LanguageContent.Select(lc => lc.LanguageId).ToArray();
            var contentLanguages = await _nyssContext.ContentLanguages.Where(cl => languageContentIds.Contains(cl.Id)).ToDictionaryAsync(cl => cl.Id, cl => cl);
            var healthRisk = new HealthRisk
            {
                HealthRiskType = dto.HealthRiskType,
                HealthRiskCode = dto.HealthRiskCode,
                LanguageContents = dto.LanguageContent.Select(lc => new HealthRiskLanguageContent
                {
                    Name = lc.Name,
                    FeedbackMessage = lc.FeedbackMessage,
                    CaseDefinition = lc.CaseDefinition,
                    ContentLanguage = contentLanguages[lc.LanguageId]
                }).ToList(),
                HealthRiskSuspectedDiseases = dto.HealthRiskSuspectedDiseases != null
                    ? dto.HealthRiskSuspectedDiseases
                        .Select(hrsd => new HealthRiskSuspectedDisease
                        {
                            SuspectedDiseaseId = hrsd.SuspectedDiseaseId,
                        }).ToList()
                    : new List<HealthRiskSuspectedDisease>(),
                AlertRule = dto.AlertRuleCountThreshold.HasValue
                    ? new AlertRule
                    {
                        CountThreshold = dto.AlertRuleCountThreshold.Value,
                        DaysThreshold = dto.AlertRuleDaysThreshold,
                        KilometersThreshold = dto.AlertRuleKilometersThreshold
                    } : null
            };

            await _nyssContext.AddAsync(healthRisk);
            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(ResultKey.HealthRisk.Create.CreationSuccess);
        }

        public async Task<Result> Edit(int id, HealthRiskRequestDto healthRiskRequestDto)
        {
            var healthRisk = await _nyssContext.HealthRisks
                .Include(hr => hr.HealthRiskSuspectedDiseases)
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
                var languageContent = healthRisk.LanguageContents.SingleOrDefault(lc => lc.ContentLanguage?.Id == languageContentDto.LanguageId)
                    ?? CreateNewLanguageContent(healthRisk, languageContentDto.LanguageId);

                languageContent.FeedbackMessage = languageContentDto.FeedbackMessage;
                languageContent.CaseDefinition = languageContentDto.CaseDefinition;
                languageContent.Name = languageContentDto.Name;
            }

            // Update suspected diseases
            healthRisk.HealthRiskSuspectedDiseases.Clear();

            if (healthRiskRequestDto.HealthRiskSuspectedDiseases != null &&
                healthRiskRequestDto.HealthRiskSuspectedDiseases.Any())
            {
                foreach (var suspectedDiseaseDto in healthRiskRequestDto.HealthRiskSuspectedDiseases)
                {
                    healthRisk.HealthRiskSuspectedDiseases.Add(new HealthRiskSuspectedDisease
                    {
                        SuspectedDiseaseId = suspectedDiseaseDto.SuspectedDiseaseId
                    });
                }
            }

            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(ResultKey.HealthRisk.Edit.EditSuccess);
        }

        public async Task<Result> Delete(int id)
        {
            var healthRisk = await _nyssContext.HealthRisks
                .Include(hr => hr.AlertRule)
                .Include(hr => hr.LanguageContents)
                .Include(hr => hr.HealthRiskSuspectedDiseases)
                .SingleOrDefaultAsync(hr => hr.Id == id);

            if (healthRisk == null)
            {
                return Error(ResultKey.HealthRisk.HealthRiskNotFound);
            }

            if (await HealthRiskContainsReports(id))
            {
                return Error(ResultKey.HealthRisk.HealthRiskContainsReports);
            }

            // Check for alerts (though this should be covered by reports check, adding for safety)
            if (await HealthRiskHasAlerts(id))
            {
                return Error(ResultKey.HealthRisk.HealthRiskContainsReports);
            }

            // Get all ProjectHealthRisks that reference this HealthRisk
            var projectHealthRisks = await _nyssContext.ProjectHealthRisks
                .Include(phr => phr.AlertRule)
                .Include(phr => phr.ProjectHealthRiskAlertRecipients)
                .Where(phr => phr.HealthRiskId == id)
                .ToListAsync();

            // Delete ProjectHealthRisks and their related entities first
            foreach (var projectHealthRisk in projectHealthRisks)
            {
                // Delete ProjectHealthRiskAlertRecipients (many-to-many relationship)
                if (projectHealthRisk.ProjectHealthRiskAlertRecipients != null && projectHealthRisk.ProjectHealthRiskAlertRecipients.Any())
                {
                    _nyssContext.ProjectHealthRiskAlertRecipients.RemoveRange(projectHealthRisk.ProjectHealthRiskAlertRecipients);
                }

                // Delete AlertRule if it exists
                if (projectHealthRisk.AlertRule != null)
                {
                    var projectHealthRiskAlertRule = projectHealthRisk.AlertRule;
                    _nyssContext.AlertRules.Remove(projectHealthRiskAlertRule);
                }

                // Delete ProjectHealthRisk
                _nyssContext.ProjectHealthRisks.Remove(projectHealthRisk);
            }

            await _nyssContext.SaveChangesAsync();

            var alertRuleToDelete = healthRisk.AlertRule;

            // Delete HealthRisk first using EF (this removes the foreign key reference)
            _nyssContext.HealthRisks.Remove(healthRisk);
            await _nyssContext.SaveChangesAsync();

            // Now delete the AlertRule if it existed (safe to delete now that HealthRisk is gone)
            if (alertRuleToDelete != null)
            {
                _nyssContext.AlertRules.Remove(alertRuleToDelete);
                await _nyssContext.SaveChangesAsync();
            }

            return SuccessMessage(ResultKey.HealthRisk.Remove.RemoveSuccess);
        }

        private async Task<bool> HealthRiskContainsReports(int healthRiskId) =>
            await _nyssContext.ProjectHealthRisks.AnyAsync(phr => phr.HealthRiskId == healthRiskId && phr.Reports.Any(r => !r.IsTraining));

        private async Task<bool> HealthRiskHasAlerts(int healthRiskId) =>
            await _nyssContext.ProjectHealthRisks.AnyAsync(phr => phr.HealthRiskId == healthRiskId && phr.Alerts.Any());

        private HealthRiskLanguageContent CreateNewLanguageContent(HealthRisk healthRisk, int languageId)
        {
            var newLanguageContent = new HealthRiskLanguageContent
            {
                HealthRisk = healthRisk,
                ContentLanguageId = languageId
            };

            healthRisk.LanguageContents.Add(newLanguageContent);

            return newLanguageContent;
        }

    }
}
