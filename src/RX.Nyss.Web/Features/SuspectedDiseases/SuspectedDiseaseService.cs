using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.SuspectedDiseases.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.SuspectedDiseases
{
    public interface ISuspectedDiseaseService
    {
        Task<Result<IEnumerable<SuspectedDiseaseListItemResponseDto>>> List();
        Task<Result<SuspectedDiseaseResponseDto>> Get(int id);
        Task<Result> Create(SuspectedDiseaseRequestDto suspectedDiseaseRequestDto);
        Task<Result> Edit(int id, SuspectedDiseaseRequestDto suspectedDiseaseRequestDto);
        Task<Result> Delete(int id);
    }

    public class SuspectedDiseaseService : ISuspectedDiseaseService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;

        public SuspectedDiseaseService(INyssContext nyssContext, IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
        }

        public async Task<Result<IEnumerable<SuspectedDiseaseListItemResponseDto>>> List()
        {
            var userName = _authorizationService.GetCurrentUserName();

            var languageCode = await _nyssContext.Users.FilterAvailable()
                .Where(u => u.EmailAddress == userName)
                .Select(u => u.ApplicationLanguage.LanguageCode)
                .SingleOrDefaultAsync() ?? "en";

            var suspectedDisease = await _nyssContext.SuspectedDiseases
                .Select(sd => new SuspectedDiseaseListItemResponseDto
                {
                    Id = sd.Id,
                    Name = sd.LanguageContents
                        .Where(lc => lc.ContentLanguage.LanguageCode == languageCode)
                        .Select(lc => lc.Name)
                        .FirstOrDefault(),
                    SuspectedDiseaseCode = sd.SuspectedDiseaseCode
                })
                .OrderBy(sd => sd.SuspectedDiseaseCode)
                .ToListAsync();

            return Success<IEnumerable<SuspectedDiseaseListItemResponseDto>>(suspectedDisease);
        }

        public async Task<Result<SuspectedDiseaseResponseDto>> Get(int id)
        {
            var suspectedDiseaseResponse = await _nyssContext.SuspectedDiseases
                .Where(suspectedDisease => suspectedDisease.Id == id)
                .Select(suspectedDisease => new SuspectedDiseaseResponseDto
                {
                    Id = suspectedDisease.Id,
                    SuspectedDiseaseCode = suspectedDisease.SuspectedDiseaseCode,
                    LanguageContent = suspectedDisease.LanguageContents.Select(lc => new SuspectedDiseaseLanguageContentDto
                    {
                        LanguageId = lc.ContentLanguage.Id,
                        Name = lc.Name
                    })
                }).SingleOrDefaultAsync();

            if (suspectedDiseaseResponse == null)
            {
                return Error<SuspectedDiseaseResponseDto>(ResultKey.SuspectedDisease.SuspectedDiseaseNotFound);
            }

            return Success(suspectedDiseaseResponse);
        }

        public async Task<Result> Create(SuspectedDiseaseRequestDto suspectedDiseaseRequestDto)
        {
            if (await _nyssContext.SuspectedDiseases.AnyAsync(sd => sd.SuspectedDiseaseCode == suspectedDiseaseRequestDto.SuspectedDiseaseCode))
            {
                return Error(ResultKey.SuspectedDisease.SuspectedDiseaseNumberAlreadyExists);
            }

            var languageContentIds = suspectedDiseaseRequestDto.LanguageContent.Select(lc => lc.LanguageId).ToArray();
            var contentLanguages = await _nyssContext.ContentLanguages.Where(cl => languageContentIds.Contains(cl.Id)).ToDictionaryAsync(cl => cl.Id, cl => cl);

            var suspectedDisease = new SuspectedDisease
            {
                SuspectedDiseaseCode = suspectedDiseaseRequestDto.SuspectedDiseaseCode,
                LanguageContents = suspectedDiseaseRequestDto.LanguageContent.Select(lc => new SuspectedDiseaseLanguageContent
                {
                    Name = lc.Name,
                    ContentLanguageId = lc.LanguageId
                }).ToList()
            };

            await _nyssContext.AddAsync(suspectedDisease);
            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(ResultKey.SuspectedDisease.Create.CreationSuccess);
        }

        public async Task<Result> Edit(int id, SuspectedDiseaseRequestDto suspectedDiseaseRequestDto)
        {
            var suspectedDisease = await _nyssContext.SuspectedDiseases
                .Include(sd => sd.LanguageContents)
                .ThenInclude(lc => lc.ContentLanguage)
                .SingleOrDefaultAsync(sd => sd.Id == id);

            if (suspectedDisease == null)
            {
                return Error(ResultKey.SuspectedDisease.SuspectedDiseaseNotFound);
            }

            if (await _nyssContext.SuspectedDiseases.AnyAsync(sd => sd.Id != id && sd.SuspectedDiseaseCode == suspectedDiseaseRequestDto.SuspectedDiseaseCode))
            {
                return Error(ResultKey.SuspectedDisease.SuspectedDiseaseNumberAlreadyExists);
            }

            suspectedDisease.SuspectedDiseaseCode = suspectedDiseaseRequestDto.SuspectedDiseaseCode;

            foreach (var languageContentDto in suspectedDiseaseRequestDto.LanguageContent)
            {
                var languageContent = suspectedDisease.LanguageContents.SingleOrDefault(lc => lc.ContentLanguage?.Id == languageContentDto.LanguageId)
                    ?? CreateNewLanguageContent(suspectedDisease, languageContentDto.LanguageId);

                languageContent.Name = languageContentDto.Name;
            }

            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(ResultKey.SuspectedDisease.Edit.EditSuccess);
        }

        public async Task<Result> Delete(int id)
        {
            var suspectedDisease = await _nyssContext.SuspectedDiseases
                .Include(sd => sd.LanguageContents)
                .SingleOrDefaultAsync(sd => sd.Id == id);

            if (suspectedDisease == null)
            {
                return Error(ResultKey.SuspectedDisease.SuspectedDiseaseNotFound);
            }

            /*if (await SuspectedDiseaseContainsReports(id))
            {
                return Error(ResultKey.SuspectedDisease.SuspectedDiseaseContainsReports);
            }*/

            _nyssContext.SuspectedDiseases.Remove(suspectedDisease);
            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(ResultKey.SuspectedDisease.Remove.RemoveSuccess);
        }

        /*private static bool CodeOrNameWasChanged(SuspectedDiseaseRequestDto suspectedDiseaseRequestDto, SuspectedDisease suspectedDisease) =>
            suspectedDiseaseRequestDto.SuspectedDiseaseCode != suspectedDisease.SuspectedDiseaseCode ||
            suspectedDiseaseRequestDto.LanguageContent.Any(lcDto =>
                suspectedDisease.LanguageContents.Any(lc =>
                    lc.ContentLanguage.Id == lcDto.LanguageId && !string.IsNullOrEmpty(lc.Name)) &&
                lcDto.Name != suspectedDisease.LanguageContents.Single(lc => lc.ContentLanguage.Id == lcDto.LanguageId).Name);*/

        private SuspectedDiseaseLanguageContent CreateNewLanguageContent(SuspectedDisease suspectedDisease, int languageId)
        {
            var newLanguageContent = new SuspectedDiseaseLanguageContent
            {
                SuspectedDisease = suspectedDisease,
                ContentLanguageId = languageId
            };

            suspectedDisease.LanguageContents.Add(newLanguageContent);

            return newLanguageContent;
        }
    }
}
