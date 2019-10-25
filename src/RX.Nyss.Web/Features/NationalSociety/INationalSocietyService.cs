using System.Collections.Generic;
using System.Threading.Tasks;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.NationalSociety
{
    public interface INationalSocietyService
    {
        Task<IEnumerable<Country>> GetCountries();
        Task<IEnumerable<ContentLanguage>> GetLanguages();
        Task<Result> CreateNationalSociety(CreateNationalSocietyRequestDto nationalSociety);
        Task<Result> EditNationalSociety(EditNationalSocietyRequestDto nationalSociety);
    }
}
