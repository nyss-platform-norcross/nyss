using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Organizations.Validation
{
    public interface IOrganizationValidationService
    {
        Task<bool> NameExists(string name);
    }

    public class OrganizationValidationService : IOrganizationValidationService
    {
        private readonly INyssContext _nyssContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrganizationValidationService(INyssContext nyssContext, IHttpContextAccessor httpContextAccessor)
        {
            _nyssContext = nyssContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> NameExists(string name)
        {
            var nationalSocietyId = _httpContextAccessor.GetResourceParameter("nationalSocietyId") ?? await _nyssContext.Organizations
                .Where(o => o.Id == _httpContextAccessor.GetResourceParameter("organizationId"))
                .Select(o => o.NationalSocietyId)
                .FirstOrDefaultAsync();

            return await _nyssContext.Organizations.AnyAsync(o => o.NationalSocietyId == nationalSocietyId && o.Name == name);
        }
    }
}
