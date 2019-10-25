using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.AppData
{
    public class AppDataService : IAppDataService
    {
        private readonly INyssContext _nyssContext;

        public AppDataService(INyssContext context)
        {
            _nyssContext = context;
        }

        public async Task<IEnumerable<ContentLanguage>> GetLanguages()
        {
            return await _nyssContext.ContentLanguages.ToListAsync();
        }
    }
}
