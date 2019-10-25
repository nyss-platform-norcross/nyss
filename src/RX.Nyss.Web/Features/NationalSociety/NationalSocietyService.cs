using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Features.NationalSociety
{
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
            using (StreamReader r = new StreamReader("Features/NationalSociety/CountryData/countries.json"))
            {
                string json = await r.ReadToEndAsync();
                return JsonConvert.DeserializeObject<List<Country>>(json);
            }
        }

        public async Task<IEnumerable<ContentLanguage>> GetLanguages()
        {
            return await _nyssContext.ContentLanguages.ToListAsync();
        }

        public async Task<Result> CreateAndSaveNationalSociety(NationalSocietyRequest nationalSocietyReq)
        {
            try
            {
                var nationalSociety = new RX.Nyss.Data.Models.NationalSociety()
                {
                    Name = nationalSocietyReq.Name,
                    ContentLanguage = await GetLanguageByCode(nationalSocietyReq.ContentLanguage.LanguageCode),
                    CountryCode = nationalSocietyReq.Country.CountryCode,
                    CountryName = nationalSocietyReq.Country.Name,
                    IsArchived = false
                };

                if (nationalSociety.ContentLanguage == null)
                {
                    throw new ResultException(ResultKey.NationalSociety.Creation.LanguageNotDefined);
                }

                if (string.IsNullOrEmpty(nationalSociety.CountryName))
                {
                    throw new ResultException(ResultKey.NationalSociety.Creation.CountryNotDefined);
                }

                await _nyssContext.AddAsync(nationalSociety);
                await _nyssContext.SaveChangesAsync();
                _loggerAdapter.Info($"A national society {nationalSociety} was created");
                return Result.Success(ResultKey.NationalSociety.Creation.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return HandleException(e);
            }
        }

        public async Task<ContentLanguage> GetLanguageByCode(string languageCode) =>
            await _nyssContext.ContentLanguages.FirstOrDefaultAsync(_ => _.LanguageCode == languageCode);

        public Result HandleException(Exception e)
        {
            if (e.InnerException is SqlException sqlException)
            {
                if (sqlException.Number == 2627 || sqlException.Number == 2601) // national society name already exists
                {
                    return Result.Error(ResultKey.NationalSociety.Creation.NationalSocietyAlreadyExists);                
                }
            }
            return Result.Error(e.Message);
        }
    }
}
