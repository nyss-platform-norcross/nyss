using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Logging;
using RX.Nyss.Web.Utils.DataContract;

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
                    ContentLanguage = new ContentLanguage()
                    {
                        DisplayName = nationalSocietyReq.ContentLanguage.DisplayName,
                        LanguageCode = nationalSocietyReq.ContentLanguage.LanguageCode
                    },
                    IsArchived = false
                };
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
                return Result.Error(e.Message);
            }
        }
    }
}
