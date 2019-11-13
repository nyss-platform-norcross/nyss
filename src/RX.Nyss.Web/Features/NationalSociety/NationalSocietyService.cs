using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.NationalSociety.Dto;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSociety
{
    public interface INationalSocietyService
    {
        Task<Result<List<NationalSocietyListResponseDto>>> GetNationalSocieties(string userEmail, IEnumerable<string> userRoles);
        Task<Result<NationalSocietyResponseDto>> GetNationalSociety(int id);
        Task<Result<int>> CreateNationalSociety(CreateNationalSocietyRequestDto nationalSociety);
        Task<Result> EditNationalSociety(int nationalSocietyId, EditNationalSocietyRequestDto nationalSociety);
        Task<Result> RemoveNationalSociety(int id);
    }

    public class NationalSocietyService : INationalSocietyService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IConfig _config;
        private readonly IUserService _userService;

        public NationalSocietyService(INyssContext context, ILoggerAdapter loggerAdapter, IConfig config, IUserService userService)
        {
            _nyssContext = context;
            _loggerAdapter = loggerAdapter;
            _config = config;
            _userService = userService;
        }

        public async Task<Result<List<NationalSocietyListResponseDto>>> GetNationalSocieties(string userEmail, IEnumerable<string> userRoles)
        {
            try
            {
                var nationalSocietiesQuery = _nyssContext.NationalSocieties
                    .Include(x => x.HeadManager)
                    .Include(x => x.PendingHeadManager)
                    .Select(n => new NationalSocietyListResponseDto
                    {
                        Id = n.Id,
                        ContentLanguage = n.ContentLanguage.DisplayName,
                        Name = n.Name,
                        Country = n.Country.Name,
                        StartDate = n.StartDate,
                        HeadManagerName = n.HeadManager.Name,
                        PendingHeadManagerName = n.PendingHeadManager.Name
                    });

                if (!_userService.HasAccessToAllNationalSocieties(userRoles))
                {
                    var availableNationalSocieties = await _userService.GetUserNationalSocietyIds(userEmail);
                    nationalSocietiesQuery = nationalSocietiesQuery.Where(ns => availableNationalSocieties.Contains(ns.Id));
                }

                var list = await nationalSocietiesQuery
                    .OrderBy(n => n.Name)
                    .ToListAsync();

                return Success(list);
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return HandleException(e).Cast<List<NationalSocietyListResponseDto>>();
            }
        }

        public async Task<Result<NationalSocietyResponseDto>> GetNationalSociety(int id)
        {
            try
            {
                var nationalSociety = await _nyssContext.NationalSocieties
                    .Select(n => new NationalSocietyResponseDto
                    {
                        Id = n.Id,
                        ContentLanguageId = n.ContentLanguage.Id,
                        ContentLanguageName = n.ContentLanguage.DisplayName,
                        Name = n.Name,
                        CountryId = n.Country.Id,
                        CountryName = n.Country.Name
                    })
                    .FirstOrDefaultAsync(n => n.Id == id);

                if (nationalSociety == null)
                {
                    return Error<NationalSocietyResponseDto>(ResultKey.NationalSociety.NotFound);
                }

                return Success(nationalSociety);
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return HandleException(e).Cast<NationalSocietyResponseDto>();
            }
        }

        public async Task<Result<int>> CreateNationalSociety(CreateNationalSocietyRequestDto dto)
        {
            try
            {
                if (_nyssContext.NationalSocieties.Any(ns => ns.Name.ToLower() == dto.Name.ToLower()))
                {
                    return Error<int>(ResultKey.NationalSociety.Creation.NameAlreadyExists);
                }

                var nationalSociety = new Nyss.Data.Models.NationalSociety
                {
                    Name = dto.Name,
                    ContentLanguage = await GetLanguageById(dto.ContentLanguageId),
                    Country = await GetCountryById(dto.CountryId),
                    IsArchived = false,
                    StartDate = DateTime.UtcNow
                };

                if (nationalSociety.ContentLanguage == null)
                {
                    return Error<int>(ResultKey.NationalSociety.Creation.LanguageNotFound);
                }

                if (nationalSociety.Country == null)
                {
                    return Error<int>(ResultKey.NationalSociety.Creation.CountryNotFound);
                }

                var entity = await _nyssContext.AddAsync(nationalSociety);
                await _nyssContext.SaveChangesAsync();
                _loggerAdapter.Info($"A national society {nationalSociety} was created");
                return Success(entity.Entity.Id, ResultKey.NationalSociety.Creation.Success);
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return HandleException(e).Cast<int>();
            }
        }

        public async Task<Result> EditNationalSociety(int nationalSocietyId, EditNationalSocietyRequestDto dto)
        {
            try
            {
                if (_nyssContext.NationalSocieties.Any(ns => ns.Id != nationalSocietyId && ns.Name.ToLower() == dto.Name.ToLower()))
                {
                    return Error<int>(ResultKey.NationalSociety.Creation.NameAlreadyExists);
                }

                var nationalSociety = await _nyssContext.NationalSocieties.FindAsync(nationalSocietyId);

                nationalSociety.Name = dto.Name;
                nationalSociety.ContentLanguage = await GetLanguageById(dto.ContentLanguageId);
                nationalSociety.Country = await GetCountryById(dto.CountryId);

                await _nyssContext.SaveChangesAsync();

                return SuccessMessage(ResultKey.NationalSociety.Edit.Success);
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        public async Task<Result> RemoveNationalSociety(int id)
        {
            try
            {
                var nationalSociety = await _nyssContext.NationalSocieties.FindAsync(id);
                _nyssContext.NationalSocieties.Remove(nationalSociety);
                await _nyssContext.SaveChangesAsync();
                return SuccessMessage(ResultKey.NationalSociety.Remove.Success);
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        public async Task<ContentLanguage> GetLanguageById(int id) =>
            await _nyssContext.ContentLanguages.FindAsync(id);

        public async Task<Country> GetCountryById(int id) =>
            await _nyssContext.Countries.FindAsync(id);

        public Result HandleException(Exception e)
        {
            if (e.InnerException is SqlException sqlException)
            {
                if (sqlException.Number == 2627 || sqlException.Number == 2601) // national society name already exists
                {
                    return Error(ResultKey.NationalSociety.Creation.NameAlreadyExists);
                }
            }

            return Error(ResultKey.UnexpectedError);
        }
    }
}
