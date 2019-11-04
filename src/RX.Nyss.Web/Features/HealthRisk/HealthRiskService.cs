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
        Task<Result<int>> CreateHealthRisk(CreateHealthRiskRequestDto createHealthRiskDto);
        Task<Result> EditHealthRisk(EditHealthRiskRequestDto editHealthRiskDto);
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
            try
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
                    }).ToListAsync();

                return Success<IEnumerable<HealthRiskResponseDto>>(healthRisks);
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return Error(ResultKey.Shared.GeneralErrorMessage).Cast<IEnumerable<HealthRiskResponseDto>>();
            }
        }

        public async Task<Result<GetHealthRiskResponseDto>> GetHealthRisk(int id)
        {
            try
            {
                var healthRiskResponse = await _nyssContext.HealthRisks
                    .Where(healthRisk => healthRisk.Id == id)
                    .Select(healthRisk => new GetHealthRiskResponseDto
                    {
                        Id = healthRisk.Id,
                        HealthRiskCode = healthRisk.HealthRiskCode,
                        HealthRiskType = healthRisk.HealthRiskType,
                        LanguageContent = healthRisk.LanguageContents.Select(lc => new HealthRiskLanguageContentDto
                        {
                            LanguageId = lc.ContentLanguage.Id,
                            CaseDefinition = lc.CaseDefinition,
                            FeedbackMessage = lc.FeedbackMessage,
                            Name = lc.Name
                        })
                    }).FirstOrDefaultAsync();

                if (healthRiskResponse == null)
                {
                    return Error(ResultKey.HealthRisk.HealthRiskNotFound).Cast<GetHealthRiskResponseDto>();
                }

                return Success<GetHealthRiskResponseDto>(healthRiskResponse);
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return Error(ResultKey.Shared.GeneralErrorMessage).Cast<GetHealthRiskResponseDto>();
            }
        }

        public async Task<Result<int>> CreateHealthRisk(CreateHealthRiskRequestDto createHealthRiskDto)
        {
            try
            {
                if (await _nyssContext.HealthRisks.AnyAsync(hr => hr.HealthRiskCode == createHealthRiskDto.HealthRiskCode))
                {
                    return Error(ResultKey.HealthRisk.HealthRiskNumberAlreadyExists).Cast<int>();
                }

                var languageContentIds = createHealthRiskDto.LanguageContent.Select(lc => lc.LanguageId).ToArray();
                var contentLanguages = await _nyssContext.ContentLanguages.Where(cl => languageContentIds.Contains(cl.Id)).ToDictionaryAsync(cl => cl.Id, cl => cl);
                var healthRiskLanguageContents = new List<HealthRiskLanguageContent>();

                var healthRisk = new Nyss.Data.Models.HealthRisk()
                {
                    HealthRiskType = createHealthRiskDto.HealthRiskType,
                    HealthRiskCode = createHealthRiskDto.HealthRiskCode,
                    LanguageContents = createHealthRiskDto.LanguageContent.Select(lc => new HealthRiskLanguageContent
                    {
                        Name = lc.Name,
                        FeedbackMessage = lc.FeedbackMessage,
                        CaseDefinition = lc.CaseDefinition,
                        ContentLanguage = contentLanguages[lc.LanguageId]
                    }).ToList(),
                    AlertRule = new Nyss.Data.Models.AlertRule()
                    {
                        CountThreshold = createHealthRiskDto.AlertRuleCountThreshold,
                        HoursThreshold = createHealthRiskDto.AlertRuleHoursThreshold,
                        MetersThreshold = createHealthRiskDto.AlertRuleMetersThreshold
                    }
                };

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
                var healthRisk = await _nyssContext.HealthRisks
                    .Include(hr => hr.AlertRule)
                    .Include(hr => hr.LanguageContents).ThenInclude(lc => lc.ContentLanguage)
                    .SingleOrDefaultAsync(hr => hr.Id == editHealthRiskDto.Id);

                if (healthRisk == null)
                {
                    return Error(ResultKey.HealthRisk.HealthRiskNotFound);
                }

                healthRisk.HealthRiskCode = editHealthRiskDto.HealthRiskCode;
                healthRisk.HealthRiskType = editHealthRiskDto.HealthRiskType;
                healthRisk.AlertRule.CountThreshold = editHealthRiskDto.AlertRuleCountThreshold;
                healthRisk.AlertRule.HoursThreshold = editHealthRiskDto.AlertRuleHoursThreshold;
                healthRisk.AlertRule.MetersThreshold = editHealthRiskDto.AlertRuleMetersThreshold;

                foreach (var languageContentDto in editHealthRiskDto.LanguageContent)
                {
                    var languageContent = healthRisk.LanguageContents.SingleOrDefault(lc => lc.ContentLanguage.Id == languageContentDto.LanguageId);
                    if (languageContent == null)
                    {
                        languageContent = new HealthRiskLanguageContent
                        {
                            HealthRisk = healthRisk,
                            ContentLanguage = await _nyssContext.ContentLanguages.FindAsync(languageContentDto.LanguageId)
                        };
                        healthRisk.LanguageContents.Add(languageContent);
                    }

                    languageContent.FeedbackMessage = languageContentDto.FeedbackMessage;
                    languageContent.CaseDefinition = languageContentDto.CaseDefinition;
                    languageContent.Name = languageContentDto.Name;
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
                var healthRisk = await _nyssContext.HealthRisks
                    .Include(hr => hr.AlertRule)
                    .Include(hr => hr.LanguageContents).ThenInclude(lc => lc.ContentLanguage)
                    .SingleOrDefaultAsync(hr => hr.Id == id);

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
    }
}
