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
        Task<bool> NameExists(int nationalSocietyId, int? organizationId, string name);
    }

    public class OrganizationValidationService : IOrganizationValidationService
    {
        private readonly INyssContext _nyssContext;

        public OrganizationValidationService(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public async Task<bool> NameExists(int nationalSocietyId, int? organizationId, string name) =>
            await _nyssContext.Organizations.AnyAsync(o => o.NationalSocietyId == nationalSocietyId && (organizationId.HasValue
                ? o.Id != organizationId && o.Name == name
                : o.Name == name));
    }
}
