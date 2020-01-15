using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Configuration;
using RX.Nyss.Data;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.AppData.Dto;
using RX.Nyss.Web.Utils.DataContract;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.AppData
{
    public interface IAppDataService
    {
        Task<Result<AppDataResponseDto>> GetAppData();
    }

    public class AppDataService : IAppDataService
    {
        private readonly INyssContext _nyssContext;
        private readonly INyssWebConfig _config;

        public AppDataService(
            INyssContext context, 
            INyssWebConfig config)
        {
            _nyssContext = context;
            _config = config;
        }

        public async Task<Result<AppDataResponseDto>> GetAppData() => 
            Success(new AppDataResponseDto
            {
                Countries = await _nyssContext.Countries
                        .Select(cl => new AppDataResponseDto.CountryDto
                        {
                            Id = cl.Id,
                            Name = cl.Name
                        })
                        .ToListAsync(),
                ContentLanguages = await _nyssContext.ContentLanguages
                        .Select(cl => new AppDataResponseDto.ContentLanguageDto
                        {
                            Id = cl.Id,
                            Name = cl.DisplayName
                        })
                        .ToListAsync(),
                IsDevelopment = !_config.IsProduction ? true : (bool?)null,
                AuthCookieExpiration = _config.Authentication.CookieExpirationTime
            });
    }
}
