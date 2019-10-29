using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.AppData.Dtos;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.AppData
{
    public interface IAppDataService
    {
        Task<Result<AppDataResponseDto>> GetAppData();
    }

    public class AppDataService : IAppDataService
    {
        private readonly INyssContext _nyssContext;

        public AppDataService(INyssContext context)
        {
            _nyssContext = context;
        }

        public async Task<Result<AppDataResponseDto>> GetAppData() => 
            Result.Success(new AppDataResponseDto
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
                        .ToListAsync()
            });
    }
}
