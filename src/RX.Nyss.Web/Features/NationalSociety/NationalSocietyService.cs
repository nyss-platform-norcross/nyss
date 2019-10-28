using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSociety
{
    public interface INationalSocietyService
    {
        Task<IEnumerable<Country>> GetCountries();
        Task<Result<List<NationalSocietyListResponseDto>>> GetNationalSocieties();
        Task<Result<NationalSocietyResponseDto>> GetNationalSociety(int id);
        Task<IEnumerable<ContentLanguage>> GetLanguages();
        Task<Result> CreateNationalSociety(CreateNationalSocietyRequestDto nationalSociety);
        Task<Result> EditNationalSociety(EditNationalSocietyRequestDto nationalSociety);
        Task<Result> RemoveNationalSociety(int id);
    }

    public class NationalSocietyService : INationalSocietyService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;

        public NationalSocietyService(INyssContext context, ILoggerAdapter loggerAdapter)
        {
            _nyssContext = context;
            _loggerAdapter = loggerAdapter;
        }

        public async Task<IEnumerable<Country>> GetCountries()
        {
            return await _nyssContext.Countries.ToListAsync();
        }

        public async Task<IEnumerable<ContentLanguage>> GetLanguages()
        {
            return await _nyssContext.ContentLanguages.ToListAsync();
        }

        public async Task<Result<List<NationalSocietyListResponseDto>>> GetNationalSocieties()
        {
            try
            {
                var list = await _nyssContext.NationalSocieties
                    .Select(n => new NationalSocietyListResponseDto
                    {
                        Id = n.Id,
                        ContentLanguage = n.ContentLanguage.DisplayName,
                        Name = n.Name,
                        Country = n.Country.Name,
                        StartDate = n.StartDate
                    })
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
                        Name = n.Name,
                        CountryId = n.Country.Id,
                        CountryName = n.Country.Name
                    })
                    .FirstOrDefaultAsync(n => n.Id == id);

                return Success(nationalSociety);
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return HandleException(e).Cast<NationalSocietyResponseDto>();
            }
        }

        public async Task<Result> CreateNationalSociety(CreateNationalSocietyRequestDto nationalSocietyReq)
        {
            try
            {
                var nationalSociety = new RX.Nyss.Data.Models.NationalSociety()
                {
                    Name = nationalSocietyReq.Name,
                    ContentLanguage = await GetLanguageById(nationalSocietyReq.ContentLanguageId),
                    Country = await GetCountryById(nationalSocietyReq.CountryId),
                    IsArchived = false,
                    StartDate = DateTime.UtcNow
                };

                if (nationalSociety.ContentLanguage == null)
                {
                    return Error(ResultKey.NationalSociety.Creation.LanguageNotDefined);
                }

                await _nyssContext.AddAsync(nationalSociety);
                await _nyssContext.SaveChangesAsync();
                _loggerAdapter.Info($"A national society {nationalSociety} was created");
                return Success(ResultKey.NationalSociety.Creation.Success);
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return HandleException(e);
            }
        }

        public async Task<Result> EditNationalSociety(EditNationalSocietyRequestDto nationalSocietyDto)
        {
            try
            {
                var nationalSociety = await _nyssContext.NationalSocieties.FindAsync(nationalSocietyDto.Id);

                nationalSociety.Name = nationalSocietyDto.Name;
                nationalSociety.ContentLanguage = await GetLanguageById(nationalSocietyDto.ContentLanguageId);
                nationalSociety.Country = await GetCountryById(nationalSocietyDto.CountryId);

                await _nyssContext.SaveChangesAsync();

                return Success(ResultKey.NationalSociety.Edit.Success);
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
                return Success(ResultKey.NationalSociety.Remove.Success);
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
                    return Error(ResultKey.NationalSociety.Creation.NationalSocietyAlreadyExists);                
                }
            }
            return Error(e.Message);
        }
    }
}
