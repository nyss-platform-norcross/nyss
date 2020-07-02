using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.NationalSocieties.Validation
{
    public interface INationalSocietyValidationService
    {
        Task<bool> NameExists(string name);
        Task<bool> LanguageExists(int languageId);
        Task<bool> CountryExists(int countryId);
    }

    public class NationalSocietyValidationService : INationalSocietyValidationService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;

        public NationalSocietyValidationService(
            INyssContext nyssContext,
            IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
        }

        public async Task<bool> NameExists(string name) => 
            await _nyssContext.NationalSocieties.AnyAsync(ns => ns.Name.ToLower() == name.ToLower());

        public async Task<bool> LanguageExists(int languageId) =>
            await _nyssContext.ContentLanguages.AnyAsync(cl => cl.Id == languageId);

        public async Task<bool> CountryExists(int countryId) =>
            await _nyssContext.Countries.AnyAsync(c => c.Id == countryId);
    }
}
